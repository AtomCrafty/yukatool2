using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
			Stream.Seek(0);
			var r = Stream.NewReader();

			var header = ReadHeader(r);

			// read instructions
			var code = new uint[header.InstrCount];
			Stream.Seek(header.InstrOffset);
			for(int i = 0; i < header.InstrCount; i++) {
				code[i] = r.ReadUInt32();
			}

			// prepare data buffer
			var dataBuffer = new byte[header.DataLength];
			Stream.Seek(header.DataOffset).Read(dataBuffer, 0, (int)header.DataLength);
			if(header.Encryption == 1) {
				for(int i = 0; i < header.DataLength; i++) {
					dataBuffer[i] ^= Options.YksScriptDataXorKey;
				}
			}

			// read index
			var index = new DataElement[header.IndexCount];
			Stream.Seek(header.IndexOffset);

			using(var dataReader = new MemoryStream(dataBuffer).NewReader()) {
				for(int i = 0; i < header.IndexCount; i++) {
					var type = (DataElementType)r.ReadUInt32();
					uint field1 = r.ReadUInt32();
					uint field2 = r.ReadUInt32();
					uint field3 = r.ReadUInt32();

					index[i] = DataElement.Create(type, field1, field2, field3, dataReader);
				}

				//foreach(var e in index) Console.WriteLine(e);

				// needed to assign a unique id to each label
				int currentLabelId = 0;

				// disassemble instructions
				var instructions = new InstructionList(header.MaxLocals);
				for(uint i = 0; i < code.Length; i++) {
					var dataElement = index[code[i]];
					switch(dataElement) {

						case DataElement.Func func:
							uint argCount = code[++i];
							var arguments = new DataElement[argCount];
							for(int j = 0; j < argCount; j++) {
								var argument = index[code[++i]];

								if(argument is DataElement.Ctrl ctrl) {
									int argLink = dataReader.Seek(ctrl.LinkOffset).ReadInt32();
									if(argLink != -1 && argLink < code.Length) {
										ctrl.LinkedElement = index[code[argLink]] as DataElement.Ctrl;
									}
									else {
										if(argLink == Format.Yks.OperatorLink) Debug.Assert(Format.Yks.Operators.Contains(ctrl.Name));
										//Console.WriteLine("Unlinked control element: " + ctrl);
									}
								}

								arguments[j] = argument;
							}
							instructions.Add(new CallInstruction(func, arguments, instructions));
							break;

						case DataElement.Ctrl ctrl:
							// assign a unique id to this label
							ctrl.Id = currentLabelId++;

							// link the label
							int link = dataReader.Seek(ctrl.LinkOffset).ReadInt32();
							if(link != -1) {
								ctrl.LinkedElement = index[code[link]] as DataElement.Ctrl;
							}
							//else Console.WriteLine("Unlinked control element: " + ctrl);

							instructions.Add(new LabelInstruction(ctrl, instructions));
							break;

						// ReSharper disable UnusedVariable
						case DataElement.SStr sstr:
						case DataElement.VInt vint:
						case DataElement.VLoc vloc:
						case DataElement.VStr vstr:
							// ReSharper enable UnusedVariable
							instructions.Add(new TargetInstruction(dataElement, instructions));
							break;
						default:
							throw new FormatException("Invalid instruction type: " + dataElement.Type);
					}
				}

				return new YukaScript { InstructionList = instructions };
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
				MaxLocals = r.ReadUInt32(),
				Unknown2 = r.ReadUInt32()
			};
		}
	}
}