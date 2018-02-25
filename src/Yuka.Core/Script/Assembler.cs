using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yuka.IO;
using Yuka.IO.Formats;
using Yuka.Script.Data;
using Yuka.Script.Instructions;
using Yuka.Util;

namespace Yuka.Script {
	public class Assembler {

		protected readonly YukaScript Script;
		protected readonly Stream Stream;
		protected readonly List<DataElement> Index = new List<DataElement>();

		public Assembler(YukaScript script, Stream stream) {
			Script = script;
			Stream = stream;
		}

		public void Assemble() {
			Script.EnsureCompiled();
			var w = Stream.NewWriter();
			long startOffset = Stream.Position;

			// skip header for now
			Stream.Seek(Format.Yks.HeaderLength, SeekOrigin.Current);

			// generate code
			uint instrOffset = (uint)(Stream.Position - startOffset);
			foreach(var instruction in Script.InstructionList) {
				switch(instruction) {

					case CallInstruction call:
						call.Function.LastUsedAt = (uint)(Stream.Position - instrOffset - startOffset) / sizeof(int);
						w.Write(IndexOf(call.Function));
						w.Write(call.Arguments.Length);
						foreach(var argument in call.Arguments) {
							w.Write(IndexOf(argument));
						}
						break;

					case LabelInstruction label:
						label.Label.LabelOffset = (int)(Stream.Position - instrOffset - startOffset) / sizeof(int);
						w.Write(IndexOf(label.Label));
						break;

					case TargetInstruction target:
						w.Write(IndexOf(target.Target));
						break;
				}
			}
			uint instrEndOffset = (uint)(Stream.Position - startOffset);

			// create data sector and calculate offsets
			var dataStream = Options.OptimizeScriptDataOnExport ? WriteDataSectorOptimized() : WriteDataSector();

			// write index
			uint indexOffset = (uint)(Stream.Position - startOffset);
			foreach(var dataElement in Index) {
				w.Write((uint)dataElement.Type);
				w.Write(dataElement.Field1);
				w.Write(dataElement.Field2);
				w.Write(dataElement.Field3);
			}
			uint indexEndOffset = (uint)(Stream.Position - startOffset);

			// write data sector
			uint dataOffset = (uint)(Stream.Position - startOffset);
			dataStream.CopyTo(Stream);
			uint dataEndOffset = (uint)(Stream.Position - startOffset);

			// write header
			Stream.Seek(startOffset);
			WriteHeader(new YksFormat.Header {
				Signature = Format.Yks.Signature,
				Encryption = (short)(Options.EncryptScriptDataOnExport ? 1 : 0),
				HeaderLength = Format.Yks.HeaderLength,
				InstrOffset = instrOffset,
				InstrCount = (instrEndOffset - instrOffset) / sizeof(int),
				IndexOffset = indexOffset,
				IndexCount = (uint)((indexEndOffset - indexOffset) / Format.Yks.IndexEntryLength),
				DataOffset = dataOffset,
				DataLength = dataEndOffset - dataOffset,
				MaxLocals = Script.InstructionList.MaxLocals
			}, w);
			Stream.Seek(dataEndOffset);
		}

		protected int IndexOf(DataElement element) {
			int index = Index.IndexOf(element);
			if(index == -1) {
				index = Index.Count;
				Index.Add(element);
			}
			return index;
		}

		protected Stream WriteDataSector() {
			var dataStream = new MemoryStream();
			var dataWriter = dataStream.NewWriter();
			foreach(var dataElement in Index) {
				switch(dataElement) {
					case DataElement.CInt cint:
						cint.ValueOffset = (uint)dataStream.Position;
						dataWriter.Write(cint.Value);
						break;
					case DataElement.CStr cstr:
						cstr.ValueOffset = (uint)dataStream.Position;
						dataWriter.WriteNullTerminatedString(cstr.Value);
						break;
					case DataElement.Ctrl ctrl:
						ctrl.NameOffset = (uint)dataStream.Position;
						dataWriter.WriteNullTerminatedString(ctrl.Name);
						ctrl.LinkOffset = (uint)dataStream.Position;
						if(ctrl.LinkedElement != null) {
							dataWriter.Write(ctrl.LinkedElement.LabelOffset);
						}
						else if(Format.Yks.Operators.Contains(ctrl.Name)) {
							dataWriter.Write(Format.Yks.OperatorLink);
						}
						else {
							dataWriter.Write(-1);
						}
						break;
					case DataElement.Func func:
						func.NameOffset = (uint)dataStream.Position;
						dataWriter.WriteNullTerminatedString(func.Name);
						break;
					case DataElement.SStr sstr:
						sstr.FlagTypeOffset = (uint)dataStream.Position;
						dataWriter.WriteNullTerminatedString(sstr.FlagType);
						break;
					case DataElement.VInt vint:
						vint.FlagTypeOffset = (uint)dataStream.Position;
						dataWriter.WriteNullTerminatedString(vint.FlagType);
						vint.FlagIdOffset = (uint)dataStream.Position;
						dataWriter.Write(vint.FlagId);
						break;
					case DataElement.VStr vstr:
						vstr.FlagTypeOffset = (uint)dataStream.Position;
						dataWriter.WriteNullTerminatedString(vstr.FlagType);
						vstr.FlagIdOffset = (uint)dataStream.Position;
						dataWriter.Write(vstr.FlagId);
						break;
				}
			}
			dataStream.Seek(0);
			return Options.EncryptScriptDataOnExport ? (Stream)new XorStream(dataStream, Options.ScriptDataXorKey) : dataStream;
		}

		protected Stream WriteDataSectorOptimized() {
			var dataStream = new MemoryStream();
			var dataWriter = dataStream.NewWriter();

			var writtenInts = new Dictionary<int, uint>();
			var writtenStrings = new Dictionary<string, uint>();

			uint WriteInt(int val) {
				if(!writtenInts.ContainsKey(val)) {
					writtenInts[val] = (uint)dataStream.Position;
					dataWriter.Write(val);
				}
				return writtenInts[val];
			}
			uint WriteString(string val) {
				if(!writtenStrings.ContainsKey(val)) {
					writtenStrings[val] = (uint)dataStream.Position;
					dataWriter.WriteNullTerminatedString(val);
				}
				return writtenStrings[val];
			}

			foreach(var dataElement in Index) {
				switch(dataElement) {
					case DataElement.CInt cint:
						cint.ValueOffset = WriteInt(cint.Value);
						break;
					case DataElement.CStr cstr:
						cstr.ValueOffset = WriteString(cstr.Value);
						break;
					case DataElement.Ctrl ctrl:
						ctrl.NameOffset = WriteString(ctrl.Name);
						if(ctrl.LinkedElement != null) {
							ctrl.LinkOffset = WriteInt(ctrl.LinkedElement.LabelOffset);
						}
						else if(Format.Yks.Operators.Contains(ctrl.Name)) {
							ctrl.LinkOffset = WriteInt(Format.Yks.OperatorLink);
						}
						else {
							ctrl.LinkOffset = WriteInt(-1);
						}
						break;
					case DataElement.Func func:
						func.NameOffset = WriteString(func.Name);
						break;
					case DataElement.SStr sstr:
						sstr.FlagTypeOffset = WriteString(sstr.FlagType);
						break;
					case DataElement.VInt vint:
						vint.FlagTypeOffset = WriteString(vint.FlagType);
						vint.FlagIdOffset = WriteInt(vint.FlagId);
						break;
					case DataElement.VStr vstr:
						vstr.FlagTypeOffset = WriteString(vstr.FlagType);
						vstr.FlagIdOffset = WriteInt(vstr.FlagId);
						break;
				}
			}
			dataStream.Seek(0);
			return Options.EncryptScriptDataOnExport ? (Stream)new XorStream(dataStream, Options.ScriptDataXorKey) : dataStream;
		}

		internal static void WriteHeader(YksFormat.Header header, BinaryWriter w) {
			w.Write(header.Signature);
			w.Write(header.Encryption);
			w.Write(header.HeaderLength);
			w.Write(header.Unknown1);
			w.Write(header.InstrOffset);
			w.Write(header.InstrCount);
			w.Write(header.IndexOffset);
			w.Write(header.IndexCount);
			w.Write(header.DataOffset);
			w.Write(header.DataLength);
			w.Write(header.MaxLocals);
			w.Write(header.Unknown2);
		}
	}
}