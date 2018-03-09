using System.IO;
using System.Text;
using Yuka.Script;
using Yuka.Script.Binary;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class YksFormat : Format {
		public override string Extension => ".yks";
		public override string Description => "Compiled Yuka script";
		public override FormatType Type => FormatType.Packed;

		public readonly byte[] Signature = Encoding.ASCII.GetBytes("YKS001");
		public readonly int HeaderLength = 0x30;
		public readonly int IndexEntryLength = 0x10;

		public readonly string[] BlockLabels = { "{", "}" };
		public readonly string[] Operators = { "+", "-", "*", "/", "%", "=", "<", ">" };
		public readonly int OperatorLink = ushort.MaxValue; // all operator ctrl elements have this link value

		public const string Flag = "Flag";
		public const string GlobalFlag = "GlobalFlag";
		public const string String = "String";
		public const string GlobalString = "GlobalString";
		public const string TempGlobalString = "tempglobalstring";
		public const string 主人公 = "主人公";
		public const string 汎用文字変数 = "汎用文字変数";

		internal Header DummyHeader => new Header { Signature = Signature, HeaderLength = HeaderLength };

		public sealed class Header {
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
			internal uint MaxLocals;
			internal uint Unknown2;
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
			return new Disassembler(name, s).Disassemble();
		}
	}

	public class YksScriptWriter : FileWriter<YukaScript> {

		public override Format Format => Yks;

		public override bool CanWrite(object obj) {
			return obj is YukaScript;
		}

		public override void Write(YukaScript script, Stream s) {
			new Assembler(script, s).Assemble();
		}
	}
}