using System.IO;
using System.Text;
using Yuka.Script;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class YksFormat : Format {
		public override string Extension => ".yks";
		public override string Description => "Compiled Yuka script";
		public override FormatType Type => FormatType.Packed;

		public readonly byte[] Signature = Encoding.ASCII.GetBytes("YKS001");
		public readonly int HeaderLength = 0x30;

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
			using(var disasm = new Disassembler(s)) {
				return disasm.Disassemble();
			}
		}
	}
}