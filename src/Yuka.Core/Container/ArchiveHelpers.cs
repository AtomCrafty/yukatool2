using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Yuka.Util;

namespace Yuka.Container {
	internal static class ArchiveHelpers {
		public static readonly byte[] Signature = Encoding.ASCII.GetBytes("YKC001");
		public const uint HeaderSize = 24;
		public const uint EntrySize = 20;

		public static ArchiveHeader DummyHeader => new ArchiveHeader { Signature = Signature, HeaderSize = HeaderSize };

		internal static ArchiveHeader ReadHeader(BinaryReader r) {
			var header = new ArchiveHeader {
				Signature = r.ReadBytes(6),
				Version = r.ReadUInt16(),
				HeaderSize = r.ReadUInt32(),
				Unknown = r.ReadUInt32(),
				IndexOffset = r.ReadUInt32(),
				IndexLength = r.ReadUInt32()
			};

			Debug.Assert(header.Signature.Matches(Signature), $"Unexpected archive signature: {Encoding.ASCII.GetString(header.Signature)}");
			Debug.Assert(header.Version == 0, $"Unsupported archive version: {header.Version}");
			Debug.Assert(header.HeaderSize == HeaderSize, $"Unexpected header size field: {header.HeaderSize}");

			return header;
		}

		internal static Dictionary<string, ArchiveFile> ReadIndex(Archive archive, BinaryReader r) {
			int count = (int)(archive.Header.IndexLength / EntrySize);
			var index = new List<ArchiveEntry>();

			r.Seek(archive.Header.IndexOffset);
			for(int i = 0; i < count; i++) {
				index.Add(ReadIndexEntry(r));
			}

			var dict = new Dictionary<string, ArchiveFile>(count);
			foreach(var entry in index) {
				var name = r.Seek(entry.NameOffset).ReadString(entry.NameLength);
				var file = new ArchiveFile(archive, name, entry.DataOffset, entry.DataLength);
				dict[name.ToLower()] = file;
			}

			return dict;
		}

		internal static ArchiveEntry ReadIndexEntry(BinaryReader r) {
			return new ArchiveEntry {
				NameOffset = r.ReadUInt32(),
				NameLength = r.ReadUInt32(),
				DataOffset = r.ReadUInt32(),
				DataLength = r.ReadUInt32(),
				Unknown = r.ReadUInt32()
			};
		}

		internal static void WriteHeader(ArchiveHeader header, BinaryWriter w) {
			w.Write(header.Signature);
			w.Write(header.Version);
			w.Write(header.HeaderSize);
			w.Write(header.Unknown);
			w.Write(header.IndexOffset);
			w.Write(header.IndexLength);
		}

		internal static void WriteIndex(Dictionary<string, ArchiveFile> files, BinaryWriter w) {
			foreach(var file in files.Values) {
				WriteIndexEntry(
					(uint)file.NameOffset,
					(uint)file.Name.Length + 1,
					(uint)file.DataOffset,
					(uint)file.DataLength, w
				);
			}
		}

		internal static void WriteIndexEntry(uint nameOffset, uint nameLength, uint dataOffset, uint dataLength, BinaryWriter w) {
			w.Write(nameOffset);
			w.Write(nameLength);
			w.Write(dataOffset);
			w.Write(dataLength);
			w.Write((uint)0);
		}
	}

	internal struct ArchiveHeader {
		internal byte[] Signature;
		internal ushort Version;
		internal uint HeaderSize;
		internal uint Unknown;
		internal uint IndexOffset;
		internal uint IndexLength;
	}

	internal struct ArchiveEntry {
		internal uint NameOffset;
		internal uint NameLength;
		internal uint DataOffset;
		internal uint DataLength;
		internal uint Unknown;
	}
}