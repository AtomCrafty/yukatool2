using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuka.IO;
using Yuka.Util;

namespace Yuka.Script.Data {
	public class DataSectorReader : IDisposable {
		protected readonly BinaryReader Reader;

		public DataSectorReader(byte[] buffer, bool decrypt) {
			if(decrypt) {
				for(int i = 0; i < buffer.Length; i++) {
					buffer[i] ^= Options.YksScriptDataXorKey;
				}
			}
			Reader = new BinaryReader(new MemoryStream(buffer));
		}

		protected readonly Dictionary<uint, ScriptValue.Int> IntElements = new Dictionary<uint, ScriptValue.Int>();
		protected readonly Dictionary<uint, ScriptValue.Str> StrElements = new Dictionary<uint, ScriptValue.Str>();

		public ScriptValue.Int GetInteger(uint offset) {
			if(IntElements.ContainsKey(offset)) return IntElements[offset];
			return IntElements[offset] = new ScriptValue.Int(Reader.Seek(offset).ReadInt32());
		}

		public ScriptValue.Str GetString(uint offset) {
			if(StrElements.ContainsKey(offset)) return StrElements[offset];
			return StrElements[offset] = new ScriptValue.Str(Reader.Seek(offset).ReadNullTerminatedString());
		}

		public void Dispose() {
			Reader?.Dispose();
		}
	}

	public class DataSectorWriter {
		public bool Encrypt = Options.YksEncryptScriptDataOnExport;
		public byte EncryptionKey = Options.YksScriptDataXorKey;

		protected readonly MemoryStream Stream;
		protected readonly BinaryWriter Writer;
		protected readonly Dictionary<ScriptValue, uint> Offsets = new Dictionary<ScriptValue, uint>();

		public DataSectorWriter() {
			Stream = new MemoryStream();
			Writer = Stream.NewWriter();
		}

		public uint Write(ScriptValue.Int value) {
			if(Offsets.ContainsKey(value)) return Offsets[value];
			uint offset = (uint)Stream.Position;
			Writer.Write(value.IntValue);
			return Offsets[value] = offset;
		}

		public uint Write(ScriptValue.Str value) {
			if(Offsets.ContainsKey(value)) return Offsets[value];
			uint offset = (uint)Stream.Position;
			Writer.WriteNullTerminatedString(value.StringValue);
			return Offsets[value] = offset;
		}

		public uint Write(int value) {
			var scriptValue = new ScriptValue.Int(value);
			if(Offsets.ContainsKey(scriptValue)) return Offsets[scriptValue];
			uint offset = (uint)Stream.Position;
			Writer.Write(value);
			return Offsets[scriptValue] = offset;
		}

		public Stream GetStream() {
			Stream.Seek(0, SeekOrigin.Begin);
			return Encrypt ? (Stream)new XorStream(Stream, EncryptionKey) : Stream;
		}
	}
}
