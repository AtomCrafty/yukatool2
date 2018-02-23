using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Yuka.Util;

namespace Yuka.Container {
	internal class ArchiveFile : IDisposable {
		internal readonly Archive Archive;
		internal string Name;
		internal long DataLength;

		internal long DataOffset;
		internal long NameOffset;

		internal readonly string OriginalName;
		internal readonly long OriginalOffset;
		internal readonly long OriginalSize;

		internal byte[] NewData;
		internal bool IsDirty { get; private set; }
		internal void MarkDirty() => IsDirty = true;

		internal bool IsOpenedExclusively { get; private set; }
		internal readonly List<Stream> Streams = new List<Stream>();

		internal ArchiveFile(Archive archive, string name, long offset, long size) {
			Archive = archive;
			OriginalName = Name = name;
			OriginalOffset = DataOffset = offset;
			OriginalSize = DataLength = size;
		}

		internal ArchiveFileStream Open(bool exclusive) {
			if(IsOpenedExclusively) return null; // file was already opened exclusively

			Stream stream;
			if(exclusive) {
				if(Streams.Count > 0) return null; // unable to obtain exclusive access
				IsOpenedExclusively = true; // prevent others to access this file
				stream = new MemoryStream();
				if(IsDirty) {
					if(NewData != null) stream.Write(NewData, 0, NewData.Length);
				}
				else {
					Archive.Stream.CopyRangeTo(stream, DataOffset, DataLength);
				}
			}
			else {
				if(IsDirty) {
					stream = new MemoryStream(NewData, false);
				}
				else {
					stream = new ReadOnlySubStream(Archive.Stream, DataOffset, DataLength);
				}
			}

			stream.Seek(0);
			Streams.Add(stream);
			return new ArchiveFileStream(this, stream);
		}

		internal bool Close(Stream stream, bool flush = false) {
			if(flush && IsOpenedExclusively && Streams.Contains(stream)) Flush();

			IsOpenedExclusively = false;
			return Streams.Remove(stream);
		}

		internal void CloseAll() {
			Streams.ForEach(stream => stream.Close());
			Streams.Clear();
			IsOpenedExclusively = false;
		}

		internal bool Flush() {
			if(!IsOpenedExclusively) return false; // no need to flush read only files

			Debug.Assert(Streams.Count == 1); // there should be exactly one (writable) stream

			if(!(Streams.FirstOrDefault() is MemoryStream ms)) return false;

			return Archive.SaveFile(this, ms);
		}

		internal void ChangeName(string name) {
			Name = name;
			NameOffset = -1;
			Archive.MarkDirty();
		}

		public void Dispose() {
			CloseAll();
		}
	}

	public class ArchiveFileStream : Stream {
		internal readonly ArchiveFile File;
		public readonly Stream BaseStream;

		internal ArchiveFileStream(ArchiveFile file, Stream baseStream) {
			File = file;
			BaseStream = baseStream;
		}

		public override void Flush() => File.Flush();

		public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);
		public override void SetLength(long value) => BaseStream.SetLength(value);
		public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);
		public override void Write(byte[] buffer, int offset, int count) => BaseStream.Write(buffer, offset, count);

		public override bool CanRead => BaseStream.CanRead;
		public override bool CanSeek => BaseStream.CanSeek;
		public override bool CanWrite => BaseStream.CanWrite;
		public override long Length => BaseStream.Length;
		public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

		public override void Close() {
			File.Close(BaseStream, true);
			base.Close();
		}
	}
}