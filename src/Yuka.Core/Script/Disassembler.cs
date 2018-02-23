using System;
using System.Collections.Generic;
using System.IO;
using Yuka.IO;
using Yuka.IO.Formats;
using Yuka.Script.Data;
using Yuka.Script.Instructions;
using Yuka.Util;

namespace Yuka.Script {
	public class Disassembler {
		protected readonly Stream Stream;

		public Disassembler(Stream stream) {
			Stream = stream;
		}

		public YukaScript Disassemble() {
			var r = Stream.NewReader();

			var header = ReadHeader(r);

			// read instructions
			var code = new uint[header.InstrCount];
			Stream.Seek(header.InstrOffset);
			for(int i = 0; i < header.InstrCount; i++) {
				code[i] = r.ReadUInt32();
			}

			// prepare data stream
			Stream dataStream = new ReadOnlySubStream(Stream, header.DataOffset, header.DataLength);
			if(header.Encryption == 1) dataStream = new XorStream(dataStream, Options.ScriptDataXorKey);
			var dataReader = dataStream.NewReader();

			// read index
			var index = new DataElement[header.IndexCount];
			Stream.Seek(header.IndexOffset);
			for(int i = 0; i < header.IndexCount; i++) {
				var type = (DataElementType)r.ReadUInt32();
				uint field1 = r.ReadUInt32();
				uint field2 = r.ReadUInt32();
				uint field3 = r.ReadUInt32();

				index[i] = DataElement.Create(type, field1, field2, field3, dataReader);
			}

			//foreach(var e in index) Console.WriteLine(e);

			// disassemble instructions
			var instructions = new List<Instruction>();
			for(int i = 0; i < code.Length; i++) {
				var dataElement = index[code[i]];
				switch(dataElement) {
					case DataElement.Func func:
						uint argCount = code[++i];
						var arguments = new DataElement[argCount];
						for(int j = 0; j < argCount; j++) {
							arguments[j] = index[code[++i]];
						}
						instructions.Add(new CallInstruction(func, arguments));
						break;
					case DataElement.Ctrl ctrl:
						instructions.Add(new LabelInstruction(ctrl));
						break;
					case DataElement.SStr sstr:
					case DataElement.VInt vint:
					case DataElement.VLoc vloc:
					case DataElement.VStr vstr:
						instructions.Add(new TargetInstruction(dataElement));
						break;
					default:
						throw new FormatException("Invalid instruction type: " + dataElement.Type);
				}
			}

			return new YukaScript { Header = header, Index = index, Instructions = instructions.ToArray() };
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