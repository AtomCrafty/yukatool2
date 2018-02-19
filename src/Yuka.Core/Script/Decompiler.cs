using System;
using System.IO;
using Yuka.IO;
using Yuka.IO.Formats;
using Yuka.Util;

namespace Yuka.Script {
	public class Decompiler {
		private AssignmentTarget _currentAssignmentTarget;

		public YukaScript Decompile(string name, Stream s) {
			var r = s.NewReader();

			var header = ReadHeader(r);

			// read instructions
			var instr = new uint[header.InstrCount];
			s.Seek(header.InstrOffset);
			for(int i = 0; i < header.InstrCount; i++) {
				instr[i] = r.ReadUInt32();
			}

			// prepare data stream
			Stream dataStream = new ReadOnlySubStream(s, header.DataOffset, header.DataLength);
			if(header.Encryption == 1) dataStream = new XorStream(dataStream, Options.ScriptDataXorKey);
			var dataReader = dataStream.NewReader();

			// read index
			var index = new YksFormat.IndexEntry[header.IndexCount];
			s.Seek(header.IndexOffset);
			for(int i = 0; i < header.IndexCount; i++) {
				index[i] = ReadEntry(r, dataReader);
			}

			foreach(var e in index) Console.WriteLine(e);

			// iterate instructions
			for(int iid = 0; iid < header.InstrCount; iid++) {
				var entry = index[instr[iid]];
				switch(entry) {
					case YksFormat.IndexEntry.Func func:
						break;
					case YksFormat.IndexEntry.Ctrl ctrl:
						break;
					case YksFormat.IndexEntry.CInt cint:
					case YksFormat.IndexEntry.CStr cstr:
						break;
					case YksFormat.IndexEntry.SStr sstr:
					case YksFormat.IndexEntry.VInt vint:
					case YksFormat.IndexEntry.VStr vstr:
					case YksFormat.IndexEntry.VLoc vloc:
						SetAssignmentTarget(entry);
						break;
				}
			}

			return null;
		}

		internal void SetAssignmentTarget(YksFormat.IndexEntry entry) {
			if(_currentAssignmentTarget != null) throw new InvalidOperationException("Assignment target already set");
			switch(entry) {
				case YksFormat.IndexEntry.SStr sstr:
					_currentAssignmentTarget = new AssignmentTarget.SpecialString(sstr.FlagType);
					break;
				case YksFormat.IndexEntry.VInt vint when vint.FlagType == "GlobalFlag":
					_currentAssignmentTarget = new AssignmentTarget.GlobalFlag(vint.FlagId);
					break;
				case YksFormat.IndexEntry.VInt vint when vint.FlagType == "Flag":
					_currentAssignmentTarget = new AssignmentTarget.LocalFlag(vint.FlagId);
					break;
				case YksFormat.IndexEntry.VStr vstr when vstr.FlagType == "GlobalString":
					_currentAssignmentTarget = new AssignmentTarget.GlobalString(vstr.FlagId);
					break;
				case YksFormat.IndexEntry.VStr vstr when vstr.FlagType == "String":
					_currentAssignmentTarget = new AssignmentTarget.LocalString(vstr.FlagId);
					break;
				case YksFormat.IndexEntry.VLoc vloc:
					_currentAssignmentTarget = new AssignmentTarget.Local(vloc.Id);
					break;
				default: throw new ArgumentOutOfRangeException(nameof(entry), "Invalid assignment target: " + entry);
			}
		}

		internal static YksFormat.IndexEntry ReadEntry(BinaryReader index, BinaryReader data) {
			var type = (YksFormat.IndexEntryType)index.ReadUInt32();
			uint field1 = index.ReadUInt32();
			uint field2 = index.ReadUInt32();
			uint field3 = index.ReadUInt32();

			switch(type) {
				case YksFormat.IndexEntryType.Func:
					return new YksFormat.IndexEntry.Func(field1, field2, field3, data);
				case YksFormat.IndexEntryType.Ctrl:
					return new YksFormat.IndexEntry.Ctrl(field1, field2, field3, data);
				case YksFormat.IndexEntryType.CInt:
					return new YksFormat.IndexEntry.CInt(field1, field2, field3, data);
				case YksFormat.IndexEntryType.CStr:
					return new YksFormat.IndexEntry.CStr(field1, field2, field3, data);
				case YksFormat.IndexEntryType.SStr:
					return new YksFormat.IndexEntry.SStr(field1, field2, field3, data);
				case YksFormat.IndexEntryType.VInt:
					return new YksFormat.IndexEntry.VInt(field1, field2, field3, data);
				case YksFormat.IndexEntryType.VStr:
					return new YksFormat.IndexEntry.VStr(field1, field2, field3, data);
				case YksFormat.IndexEntryType.VLoc:
					return new YksFormat.IndexEntry.VLoc(field1, field2, field3, data);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported index entry type");
			}
		}

		internal static YksFormat.Header ReadHeader(BinaryReader r) {
			return new YksFormat.Header {
				Signature = r.ReadBytes(6),
				Encryption = r.ReadInt16(),
				HeaderLength = r.ReadInt32(),
				Unknown1 = r.ReadUInt32(),
				InstrOffset = r.ReadUInt32(),
				InstrCount = r.ReadUInt32(),
				IndexOffset = r.ReadUInt32(),
				IndexCount = r.ReadUInt32(),
				DataOffset = r.ReadUInt32(),
				DataLength = r.ReadUInt32(),
				LocalCount = r.ReadUInt32(),
				Unknown2 = r.ReadUInt32()
			};
		}
	}
}
