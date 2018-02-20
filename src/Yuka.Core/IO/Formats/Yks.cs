using System.IO;
using System.Text;
using Yuka.Script;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class YksFormat : Format {
		public override string Extension => ".yks";
		public override FormatType Type => FormatType.Packed;

		public readonly byte[] Signature = Encoding.ASCII.GetBytes("YKS001");
		public readonly int HeaderLength = 0x30;

		internal Header DummyHeader => new Header { Signature = Signature, HeaderLength = HeaderLength };

		internal sealed class Header {
			internal byte[] Signature;
			internal short Encryption;
			internal int HeaderLength;
			internal uint Unknown1;
			internal uint InstrOffset;
			internal uint InstrCount;
			internal uint IndexOffset;
			internal uint IndexCount;
			internal uint DataOffset;
			internal uint DataLength;
			internal uint LocalCount;
			internal uint Unknown2;
		}

		internal abstract class IndexEntry {
			internal IndexEntryType Type;
			internal uint Field1, Field2, Field3;

			protected IndexEntry(IndexEntryType type, uint field1, uint field2, uint field3) {
				Type = type;
				Field1 = field1;
				Field2 = field2;
				Field3 = field3;
			}

			public override string ToString() => $"{Field1:X8} {Field2:X8} {Field3:X8} {Type}";

			#region Entry classes

			internal sealed class Func : IndexEntry {
				internal uint NameOffset { get => Field1; set => Field1 = value; }
				internal uint LastUsedAt { get => Field2; set => Field2 = value; }

				internal readonly string Name;

				public Func(uint field1, uint field2, uint field3, BinaryReader data)
					: base(IndexEntryType.Func, field1, field2, field3) {
					Name = data.Seek(NameOffset).ReadNullTerminatedString();
				}

				public override string ToString() => $"{base.ToString()} [{Name}]";
			}

			internal sealed class Ctrl : IndexEntry {
				internal uint NameOffset { get => Field1; set => Field1 = value; }
				internal uint Link { get => Field2; set => Field2 = value; }

				internal readonly string Name;

				public Ctrl(uint field1, uint field2, uint field3, BinaryReader data)
					: base(IndexEntryType.Ctrl, field1, field2, field3) {
					Name = data.Seek(NameOffset).ReadNullTerminatedString();
				}

				public override string ToString() => $"{base.ToString()} [{Name}]";
			}

			internal sealed class CInt : IndexEntry {
				internal uint ValueOffset { get => Field2; set => Field2 = value; }

				internal int Value;

				public CInt(uint field1, uint field2, uint field3, BinaryReader data)
					: base(IndexEntryType.CInt, field1, field2, field3) {
					Value = data.Seek(ValueOffset).ReadInt32();
				}

				public override string ToString() => $"{base.ToString()} [{Value}]";
			}

			internal sealed class CStr : IndexEntry {
				internal uint ValueOffset { get => Field2; set => Field2 = value; }

				internal string Value;

				public CStr(uint field1, uint field2, uint field3, BinaryReader data)
					: base(IndexEntryType.CStr, field1, field2, field3) {
					Value = data.Seek(ValueOffset).ReadNullTerminatedString();
				}

				public override string ToString() => $"{base.ToString()} [{Value}]";
			}

			internal sealed class SStr : IndexEntry {
				internal uint FlagTypeOffset { get => Field1; set => Field1 = value; }

				internal string FlagType;

				public SStr(uint field1, uint field2, uint field3, BinaryReader data)
					: base(IndexEntryType.SStr, field1, field2, field3) {
					FlagType = data.Seek(FlagTypeOffset).ReadNullTerminatedString();
				}

				public override string ToString() => $"{base.ToString()} [{FlagType}]";
			}

			internal sealed class VInt : IndexEntry {
				internal uint FlagTypeOffset { get => Field1; set => Field1 = value; }
				internal uint FlagIdOffset { get => Field3; set => Field3 = value; }

				internal string FlagType;
				internal int FlagId;

				public VInt(uint field1, uint field2, uint field3, BinaryReader data)
					: base(IndexEntryType.VInt, field1, field2, field3) {
					FlagType = data.Seek(FlagTypeOffset).ReadNullTerminatedString();
					FlagId = data.Seek(FlagIdOffset).ReadInt32();
				}

				public override string ToString() => $"{base.ToString()} [{FlagType} {FlagId}]";
			}

			internal sealed class VStr : IndexEntry {
				internal uint FlagTypeOffset { get => Field1; set => Field1 = value; }
				internal uint FlagIdOffset { get => Field3; set => Field3 = value; }

				internal string FlagType;
				internal int FlagId;

				public VStr(uint field1, uint field2, uint field3, BinaryReader data)
					: base(IndexEntryType.VStr, field1, field2, field3) {
					FlagType = data.Seek(FlagTypeOffset).ReadNullTerminatedString();
					FlagId = data.Seek(FlagIdOffset).ReadInt32();
				}

				public override string ToString() => $"{base.ToString()} [{FlagType} {FlagId}]";
			}

			internal sealed class VLoc : IndexEntry {
				internal uint Id { get => Field2; set => Field2 = value; }

				// ReSharper disable once UnusedParameter.Local
				public VLoc(uint field1, uint field2, uint field3, BinaryReader data)
					: base(IndexEntryType.VLoc, field1, field2, field3) { }

				public override string ToString() => $"{base.ToString()} [{Id}]";
			}

			#endregion
		}

		internal enum IndexEntryType : uint {
			Func = 0x00,
			Ctrl = 0x01,
			CInt = 0x04,
			CStr = 0x05,
			SStr = 0x07,
			VInt = 0x08,
			VStr = 0x09,
			VLoc = 0x0A,
			RInt = 0x0B,
			RStr = 0x0C
		}
	}

	public class YksScriptReader : FileReader<YukaScript> {

		public override Format Format => Yks;

		public override bool CanRead(string name, BinaryReader r) {
			long pos = r.BaseStream.Position;
			try {
				var signature = r.ReadBytes(Yks.Signature.Length);
				return signature.Matches(Yks.Signature);
			}
			finally { r.BaseStream.Position = pos; }
		}

		public override YukaScript Read(string name, Stream s) {
			return new Decompiler().Disassemble(name, s);
		}
	}
}