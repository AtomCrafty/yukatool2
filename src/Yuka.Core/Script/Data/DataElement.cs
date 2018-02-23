using System;
using System.IO;
using Yuka.Util;

namespace Yuka.Script.Data {

	public abstract class DataElement {
		public DataElementType Type;
		public uint Field1, Field2, Field3;

		protected DataElement(DataElementType type, uint field1, uint field2, uint field3) {
			Type = type;
			Field1 = field1;
			Field2 = field2;
			Field3 = field3;
		}

		public virtual string DisplayInfo => $"{Field1:X8} {Field2:X8} {Field3:X8} {Type}";

		#region Entry classes

		public sealed class Func : DataElement {
			public uint NameOffset { get => Field1; set => Field1 = value; }
			public uint LastUsedAt { get => Field2; set => Field2 = value; }

			public readonly string Name;

			public Func(uint field1, uint field2, uint field3, BinaryReader data)
				: base(DataElementType.Func, field1, field2, field3) {
				Name = data.Seek(NameOffset).ReadNullTerminatedString();
			}

			public override string DisplayInfo => $"{base.DisplayInfo} [{Name}]";
			public override string ToString() => Name;
		}

		public sealed class Ctrl : DataElement {
			public uint NameOffset { get => Field1; set => Field1 = value; }
			public uint Link { get => Field2; set => Field2 = value; }

			public readonly string Name;

			public Ctrl(uint field1, uint field2, uint field3, BinaryReader data)
				: base(DataElementType.Ctrl, field1, field2, field3) {
				Name = data.Seek(NameOffset).ReadNullTerminatedString();
			}

			public override string DisplayInfo => $"{base.DisplayInfo} [{Name}]";
			public override string ToString() => ':' + Name;
		}

		public sealed class CInt : DataElement {
			public uint ValueOffset { get => Field2; set => Field2 = value; }

			public int Value;

			public CInt(uint field1, uint field2, uint field3, BinaryReader data)
				: base(DataElementType.CInt, field1, field2, field3) {
				Value = data.Seek(ValueOffset).ReadInt32();
			}

			public override string DisplayInfo => $"{base.DisplayInfo} [{Value}]";
			public override string ToString() => Value.ToString();
		}

		public sealed class CStr : DataElement {
			public uint ValueOffset { get => Field2; set => Field2 = value; }

			public string Value;

			public CStr(uint field1, uint field2, uint field3, BinaryReader data)
				: base(DataElementType.CStr, field1, field2, field3) {
				Value = data.Seek(ValueOffset).ReadNullTerminatedString();
			}

			public override string DisplayInfo => $"{base.DisplayInfo} [{Value}]";

			// replacing just the most important escape sequences for easy display
			public override string ToString() => '"' + Value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\\n").Replace("\r", "\\\r").Replace("\t", "\\\t") + '"';
		}

		public sealed class SStr : DataElement {
			public uint FlagTypeOffset { get => Field1; set => Field1 = value; }

			public string FlagType;

			public SStr(uint field1, uint field2, uint field3, BinaryReader data)
				: base(DataElementType.SStr, field1, field2, field3) {
				FlagType = data.Seek(FlagTypeOffset).ReadNullTerminatedString();
			}

			public override string DisplayInfo => $"{base.DisplayInfo} [{FlagType}]";
			public override string ToString() => FlagType;
		}

		public sealed class VInt : DataElement {
			public uint FlagTypeOffset { get => Field1; set => Field1 = value; }
			public uint FlagIdOffset { get => Field3; set => Field3 = value; }

			public string FlagType;
			public int FlagId;

			public VInt(uint field1, uint field2, uint field3, BinaryReader data)
				: base(DataElementType.VInt, field1, field2, field3) {
				FlagType = data.Seek(FlagTypeOffset).ReadNullTerminatedString();
				FlagId = data.Seek(FlagIdOffset).ReadInt32();
			}

			public override string DisplayInfo => $"{base.DisplayInfo} [{FlagType} {FlagId}]";
			public override string ToString() => FlagType + ':' + FlagId;
		}

		public sealed class VStr : DataElement {
			public uint FlagTypeOffset { get => Field1; set => Field1 = value; }
			public uint FlagIdOffset { get => Field3; set => Field3 = value; }

			public string FlagType;
			public int FlagId;

			public VStr(uint field1, uint field2, uint field3, BinaryReader data)
				: base(DataElementType.VStr, field1, field2, field3) {
				FlagType = data.Seek(FlagTypeOffset).ReadNullTerminatedString();
				FlagId = data.Seek(FlagIdOffset).ReadInt32();
			}

			public override string DisplayInfo => $"{base.DisplayInfo} [{FlagType} {FlagId}]";
			public override string ToString() => FlagType + ':' + FlagId;
		}

		public sealed class VLoc : DataElement {
			public uint Id { get => Field2; set => Field2 = value; }

			// ReSharper disable once UnusedParameter.Local
			public VLoc(uint field1, uint field2, uint field3, BinaryReader data)
				: base(DataElementType.VLoc, field1, field2, field3) { }

			public override string DisplayInfo => $"{base.DisplayInfo} [{Id}]";
			public override string ToString() => "$" + Id;
		}

		#endregion

		public static DataElement Create(DataElementType type, uint field1, uint field2, uint field3, BinaryReader data) {
			switch(type) {
				case DataElementType.Func:
					return new Func(field1, field2, field3, data);
				case DataElementType.Ctrl:
					return new Ctrl(field1, field2, field3, data);
				case DataElementType.CInt:
					return new CInt(field1, field2, field3, data);
				case DataElementType.CStr:
					return new CStr(field1, field2, field3, data);
				case DataElementType.SStr:
					return new SStr(field1, field2, field3, data);
				case DataElementType.VInt:
					return new VInt(field1, field2, field3, data);
				case DataElementType.VStr:
					return new VStr(field1, field2, field3, data);
				case DataElementType.VLoc:
					return new VLoc(field1, field2, field3, data);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported index entry type");
			}
		}
	}

	public enum DataElementType : uint {
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