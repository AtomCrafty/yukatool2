using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yuka.IO;
using Yuka.IO.Formats;
using Yuka.Script.Data;
using Yuka.Script.Instructions;
using Yuka.Util;

namespace Yuka.Script {

	/// <summary>
	/// Creates a binary script file from an instruction list
	/// </summary>
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
			var dataStream = WriteDataSector();

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
				Encryption = (short)(Options.YksEncryptScriptDataOnExport ? 1 : 0),
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
			var writer = new DataSectorWriter();

			foreach(var dataElement in Index) {

				switch(dataElement) {
					case DataElement.CInt cint:
						cint.ValueOffset = writer.Write(cint.Value);
						break;
					case DataElement.CStr cstr:
						cstr.ValueOffset = writer.Write(cstr.Value);
						break;
					case DataElement.Ctrl ctrl:
						ctrl.NameOffset = writer.Write(ctrl.Name);
						if(ctrl.LinkedElement != null) {
							ctrl.LinkOffset = writer.Write(ctrl.LinkedElement.LabelOffset);
						}
						else if(Format.Yks.Operators.Contains(ctrl.Name.StringValue)) {
							ctrl.LinkOffset = writer.Write(Format.Yks.OperatorLink);
						}
						else {
							ctrl.LinkOffset = writer.Write(-1);
						}
						break;
					case DataElement.Func func:
						func.NameOffset = writer.Write(func.Name);
						break;
					case DataElement.SStr sstr:
						sstr.FlagTypeOffset = writer.Write(sstr.FlagType);
						break;
					case DataElement.VInt vint:
						vint.FlagTypeOffset = writer.Write(vint.FlagType);
						vint.FlagIdOffset = writer.Write(vint.FlagId);
						break;
					case DataElement.VStr vstr:
						vstr.FlagTypeOffset = writer.Write(vstr.FlagType);
						vstr.FlagIdOffset = writer.Write(vstr.FlagId);
						break;
				}
			}
			return writer.GetStream();
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