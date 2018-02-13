using System;
using System.Collections.Generic;
using System.IO;
using Yuka.Container;

namespace Yuka.IO {
	public abstract class FileSystem : IDisposable {

		public abstract string[] GetFiles(string filter = "*.*");
		public abstract bool FileExists(string name);
		public abstract long GetFileSize(string name);
		public abstract Stream OpenFile(string name, bool writable = false);
		public abstract Stream CreateFile(string name);
		public abstract bool DeleteFile(string name);
		public abstract void Dispose();

		#region Static methods

		public static readonly DummyFileSystem Dummy = new DummyFileSystem();

		public static FolderFileSystem NewFile(string path) {
			return FromFile(path);
		}

		public static FolderFileSystem FromFile(string path) {
			return new SingleFileSystem(path);
		}

		public static FolderFileSystem NewFolder(string path) {
			Directory.CreateDirectory(path);
			return FromFolder(path);
		}

		public static FolderFileSystem FromFolder(string path) {
			return new FolderFileSystem(path);
		}

		public static ArchiveFileSystem NewArchive(string path, ArchiveSaveMode saveMode = ArchiveSaveMode.Immediate) {
			return new ArchiveFileSystem(Archive.Create(path, saveMode));
		}

		public static ArchiveFileSystem FromArchive(string path, ArchiveSaveMode saveMode = ArchiveSaveMode.Explicit) {
			return new ArchiveFileSystem(Archive.Load(path, saveMode));
		}

		public static string NormalizePath(string path) {
			return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path).TrimEnd('/'));
		}

		#endregion
	}

	public sealed class DummyFileSystem : FileSystem {

		internal DummyFileSystem() { }

		public override string[] GetFiles(string filter = "*.*") => new string[0];

		public override bool FileExists(string name) => false;

		public override long GetFileSize(string name) => -1;

		public override Stream OpenFile(string name, bool writable = false) => throw new FileNotFoundException("Using dummy file system", name);

		public override Stream CreateFile(string name) => throw new InvalidOperationException("Cannot create file in dummy file system");

		public override bool DeleteFile(string name) => false;

		public override void Dispose() { }
	}

	/// <summary>
	/// Represents a folder in the system's native file system.
	/// File handles stay valid, even after the file system has been disposed.
	/// </summary>
	public class FolderFileSystem : FileSystem {

		public readonly string BasePath;

		public FolderFileSystem(string basePath) {
			BasePath = NormalizePath(basePath) + '\\';
			if(!Directory.Exists(BasePath)) {
				throw new FileNotFoundException(null, BasePath);
			}
		}

		public override string[] GetFiles(string filter = "*.*") {
			var files = Directory.GetFiles(BasePath, filter, SearchOption.AllDirectories);
			for(int i = 0; i < files.Length; i++) {
				files[i] = files[i].Substring(BasePath.Length).TrimStart('\\', '/');
			}
			return files;
		}

		public override bool FileExists(string name) {
			string path = Path.GetFullPath(Path.Combine(BasePath, name));
			if(!path.StartsWith(BasePath)) return false;
			return File.Exists(path);
		}

		public override long GetFileSize(string name) {
			string path = CheckInPath(name);
			return FileExists(name) ? new FileInfo(path).Length : -1;
		}

		public override Stream OpenFile(string name, bool writable = false) {
			string path = CheckInPath(name);
			return new FileStream(
				path,
				FileMode.Open,
				writable ? FileAccess.ReadWrite : FileAccess.Read,
				writable ? FileShare.None : FileShare.Read
			);
		}

		public override Stream CreateFile(string name) {
			string path = CheckInPath(name);
			Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
			return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		}

		public override bool DeleteFile(string name) {
			string path = CheckInPath(name);
			if(!File.Exists(path)) return false;
			File.Delete(path);
			return true;
		}

		protected string CheckInPath(string name) {
			string path = Path.GetFullPath(Path.Combine(BasePath, name));
			if(!path.StartsWith(BasePath))
				throw new UnauthorizedAccessException($"Path outside of directory: \n  Base: {BasePath}\n  Path: {path}");
			return path;
		}

		public override void Dispose() { }
	}

	/// <summary>
	/// Represents a filtered collection of files in the system's native file system.
	/// Allows creation of new files, which are then added to the collection.
	/// </summary>
	public class SingleFileSystem : FolderFileSystem {

		public List<string> Files = new List<string>();

		public SingleFileSystem(string path) : base(Path.GetDirectoryName(path)) {
			Files.Add(Path.GetFileName(path));
		}

		public override string[] GetFiles(string filter = "*.*") {
			return Files.ToArray();
		}

		public override bool FileExists(string name) {
			return Files.Contains(name.ToLower());
		}

		public override Stream OpenFile(string name, bool writable = false) {
			if(!FileExists(name)) throw new FileNotFoundException("Using single file system", name);
			return base.OpenFile(name, writable);
		}

		public override Stream CreateFile(string name) {
			var stream = base.CreateFile(name);
			if(stream != null) Files.Add(name.ToLower());
			return stream;
		}
	}

	/// <summary>
	/// Represents a yuka archive file.
	/// Disposing the file system will invalidate all file handles.
	/// </summary>
	public class ArchiveFileSystem : FileSystem {

		public Archive Archive;

		public ArchiveFileSystem(Archive archive) {
			Archive = archive;
		}

		public override string[] GetFiles(string filter = "*.*") {
			return Archive.GetFiles(filter);
		}

		public override bool FileExists(string name) {
			return Archive.FileExists(name);
		}

		public override long GetFileSize(string name) {
			return Archive.GetFileSize(name);
		}

		public override Stream OpenFile(string name, bool writable = false) {
			return Archive.OpenFile(name, writable);
		}

		public override Stream CreateFile(string name) {
			return Archive.CreateFile(name);
		}

		public override bool DeleteFile(string name) {
			return Archive.DeleteFile(name);
		}

		public override void Dispose() {
			Archive.Flush();
			Archive.Close();
		}

		public void Flush() {
			Archive.Flush();
		}
	}
}
