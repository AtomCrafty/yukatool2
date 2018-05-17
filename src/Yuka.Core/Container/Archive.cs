using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Yuka.Util;

namespace Yuka.Container {
	public class Archive : IDisposable {
		public readonly string Name;
		internal ArchiveHeader Header = ArchiveHelpers.DummyHeader;
		internal Dictionary<string, ArchiveFile> Files = new Dictionary<string, ArchiveFile>();

		internal Stream Stream;
		internal bool IsDirty { get; private set; }
		internal void MarkDirty() => IsDirty = true;

		internal readonly ArchiveSaveMode SaveMode;
		private readonly object _syncRoot = new object();

		public Archive(string name, Stream stream, ArchiveSaveMode saveMode = ArchiveSaveMode.Explicit) {
			Name = name;
			Stream = stream;
			SaveMode = saveMode;

			if(saveMode == ArchiveSaveMode.Immediate) {
				lock(_syncRoot) ArchiveHelpers.WriteHeader(Header, new BinaryWriter(stream));
			}
		}

		public void Reload() {
			CloseAllFiles();
			Files.Clear();

			var reader = new BinaryReader(Stream);
			Stream.Seek(0);
			lock(_syncRoot) {
				Header = ArchiveHelpers.ReadHeader(reader);
				Files = ArchiveHelpers.ReadIndex(this, reader);
			}
		}

		public string[] GetFiles(string filter = "*.*") {
			var mask = new Regex("^" + Regex.Escape(filter).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
			return Files.Keys.Where(file => mask.IsMatch(file)).ToArray();
		}

		public bool FileExists(string name) {
			return Files.ContainsKey(name.ToLower());
		}

		public long GetFileSize(string name) {
			name = name.ToLower();
			return Files.ContainsKey(name) ? Files[name].DataLength : -1;
		}

		public ArchiveFileStream OpenFile(string name, bool writable = false) {
			if(writable && SaveMode == ArchiveSaveMode.ReadOnly) {
				throw new InvalidOperationException("Archive opened in read only mode");
			}

			if(!Files.ContainsKey(name.ToLower())) throw new FileNotFoundException(name);

			return Files[name.ToLower()].Open(writable);
		}

		public ArchiveFileStream CreateFile(string name, bool deleteExisting = false) {
			string lower = name.ToLower();

			if(Files.ContainsKey(lower)) {
				if(!deleteExisting) return OpenFile(name, true);

				Files[lower].CloseAll();
				Files.Remove(lower);
				return OpenFile(name, true);
			}

			var file = new ArchiveFile(this, name, 0, 0);
			file.MarkDirty();
			Files[lower] = file;
			return file.Open(true);
		}

		public bool DeleteFile(string name) {
			string lower = name.ToLower();
			if(!Files.ContainsKey(lower)) return false;

			Files[lower].CloseAll();
			Files.Remove(lower);
			IsDirty = true;
			return true;
		}

		internal bool SaveFile(ArchiveFile file, MemoryStream data) {
			lock(_syncRoot) {
				var w = new BinaryWriter(Stream);
				switch(SaveMode) {
					case ArchiveSaveMode.Explicit:
						// mark file as dirty, but don't save to disk
						MarkDirty();
						file.MarkDirty();
						file.NewData = data.ToArray();
						file.DataLength = file.NewData.Length;
						return true;
					case ArchiveSaveMode.Flush:
						MarkDirty();

						// if index and the old file data are in the back of the file (this file was last updated), overwrite them.
						// otherwise, the old file data will be kept until Flush() is called and the entire archive is written again.
						long newFileOffset = Stream.Length;
						if(Header.IndexOffset + Header.IndexLength == newFileOffset) {
							// Index is at the end of the file, overwrite it
							newFileOffset = Header.IndexOffset;
						}
						if(file.DataOffset + file.DataLength == newFileOffset) {
							// Old file data is at the end, overwrite it
							newFileOffset = file.DataOffset;
						}
						file.DataOffset = newFileOffset;
						file.DataLength = data.Length;

						// write file data
						Stream.SetLength(newFileOffset);
						Stream.Seek(newFileOffset);
						data.Seek(0).CopyTo(Stream);

						// write index
						Header.IndexOffset = (uint)Stream.Position;
						ArchiveHelpers.WriteIndex(Files, w);

						// update header
						Stream.Seek(0);
						ArchiveHelpers.WriteHeader(Header, w);
						Stream.Flush();

						return true;
					case ArchiveSaveMode.Immediate:
						MarkDirty();
						Stream.Seek(0, SeekOrigin.End);
						// write file name
						file.NameOffset = (uint)Stream.Position;
						w.WriteNullTerminatedString(file.Name);
						// write file data
						file.DataOffset = (uint)Stream.Position;
						data.Seek(0).CopyTo(Stream);
						file.DataLength = (uint)(Stream.Position - file.DataOffset);
						return true;
					default:
						return false;
				}
			}
		}

		public void Flush() {
			lock(_syncRoot) {
				if(!IsDirty) return;

				switch(SaveMode) {
					case ArchiveSaveMode.Explicit:
					case ArchiveSaveMode.Flush:

						string tmpFile = Path.ChangeExtension(Name, ".ykc.tmp") ?? "";
						var tmp = new FileStream(tmpFile, FileMode.Create);
						var w = new BinaryWriter(tmp);

						ArchiveHelpers.WriteHeader(Header, w);

						// write names
						foreach(var file in Files.Values) {
							file.NameOffset = tmp.Position;
							w.WriteNullTerminatedString(file.Name);
						}

						// write data
						foreach(var file in Files.Values) {
							if(file.IsDirty && SaveMode == ArchiveSaveMode.Explicit) {
								file.DataOffset = tmp.Position;
								w.Write(file.NewData);
							}
							else {
								long dataOffset = tmp.Position;
								Stream.CopyRangeTo(tmp, file.DataOffset, file.DataLength);
								file.DataOffset = dataOffset;
							}
						}

						// write index
						Header.IndexOffset = (uint)tmp.Position;
						ArchiveHelpers.WriteIndex(Files, w);
						Header.IndexLength = (uint)(tmp.Position - Header.IndexOffset);

						// update header
						tmp.Seek(0);
						ArchiveHelpers.WriteHeader(Header, w);

						// swap files
						tmp.Close();
						Stream.Close();
						File.Delete(Name ?? "");
						File.Move(tmpFile, Name ?? "");

						// reload
						IsDirty = false;
						Stream = new FileStream(Name ?? "", FileMode.Open);
						Reload();
						break;
					case ArchiveSaveMode.Immediate:
						Stream.Seek(0, SeekOrigin.End);
						w = new BinaryWriter(Stream);

						// write index
						Header.IndexOffset = (uint)Stream.Position;
						ArchiveHelpers.WriteIndex(Files, w);
						Header.IndexLength = (uint)(Stream.Position - Header.IndexOffset);

						// update header
						Stream.Seek(0);
						ArchiveHelpers.WriteHeader(Header, w);
						Stream.Flush();
						break;
					default: return;
				}
			}
		}

		#region Disposal

		public void CloseAllFiles() {
			foreach(var file in Files.Values) {
				file.CloseAll();
			}
		}

		public void Close() {
			Flush();
			CloseAllFiles();
			Stream.Close();
		}

		~Archive() {
			Stream?.Dispose();
		}

		public void Dispose() {
			Close();
			Stream?.Dispose();
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Static methods

		public static Archive Load(string file, ArchiveSaveMode saveMode = ArchiveSaveMode.Explicit) {
			var archive = new Archive(file, new FileStream(file, FileMode.Open), saveMode);
			archive.Reload();
			return archive;
		}

		public static Archive Create(string file, ArchiveSaveMode saveMode = ArchiveSaveMode.Immediate) {
			return new Archive(file, new FileStream(file, FileMode.Create), saveMode);
		}

		#endregion
	}

	public enum ArchiveSaveMode {
		/// <summary>
		/// Disables saving the archive.
		/// </summary>
		ReadOnly,
		/// <summary>
		/// Stores changes in memory and only writes them to disk on explicit Archive.Flush calls.
		/// Use this mode if you don't expect large changes to your input file (in terms of file size).
		/// </summary>
		Explicit,
		/// <summary>
		/// Immediately flushes changes to disk, which may cause fragmentation. The archive is defragmented when Archive.Flush is called.
		/// Even if the application crashes between file changes, the archive file will contain the latest version (but may be fragmented).
		/// If you change the same file twice in a row, the second change won't cause any additional fragmentation.
		/// </summary>
		Flush,
		/// <summary>
		/// Files are immediately written to disk, but no index is written until Archive.Flush is called.
		/// Only use this mode if every file is written exactly once (e.g. if constructing a new archive from scratch).
		/// If a file is written more than once, both versions will be in the final archive, but only the last one will be indexable.
		/// </summary>
		Immediate
	}
}
