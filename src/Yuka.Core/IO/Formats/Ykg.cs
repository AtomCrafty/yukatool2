using System.Diagnostics;
using System.IO;
using System.Text;
using Yuka.Graphics;
using Yuka.Util;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace Yuka.IO.Formats {

	public class YkgFormat : Format {
		public override string Extension => ".ykg";
		public override FormatType Type => FormatType.Packed;

		public static readonly byte[] Signature = Encoding.ASCII.GetBytes("YKG000");
		public static readonly int HeaderLength = 0x40;

		internal static Header DummyHeader => new Header { Signature = Signature, HeaderLength = HeaderLength };

		internal struct Header {
			internal byte[] Signature;
			internal short Encryption;
			internal int HeaderLength;
			internal uint ColorOffset;
			internal uint ColorLength;
			internal uint AlphaOffset;
			internal uint AlphaLength;
			internal uint FrameOffset;
			internal uint FrameLength;
		}
	}

	public class YkgGraphicReader : FileReader<Graphic> {

		public override Format Format => Format.Ykg;

		public override bool CanRead(string name, BinaryReader r) {
			long pos = r.BaseStream.Position;
			try {
				var signature = r.ReadBytes(YkgFormat.Signature.Length);
				return signature.Matches(YkgFormat.Signature);
			}
			finally { r.BaseStream.Position = pos; }
		}

		public override Graphic Read(string name, Stream s) {

			var r = s.NewReader();
			var header = ReadHeader(r);

			Debug.Assert(header.Signature.Matches(YkgFormat.Signature));
			Debug.Assert(header.HeaderLength == YkgFormat.HeaderLength);
			Debug.Assert(header.Encryption == 0);

			var colorData = r.Seek(header.ColorOffset).ReadBytes((int)header.ColorLength).NullIfEmpty();
			var alphaData = r.Seek(header.AlphaOffset).ReadBytes((int)header.AlphaLength).NullIfEmpty();
			var frameData = r.Seek(header.FrameOffset).ReadBytes((int)header.FrameLength).NullIfEmpty();

			return new Graphic(colorData, alphaData, Animation.FromFrameData(frameData));
		}

		internal static YkgFormat.Header ReadHeader(BinaryReader r) {
			return new YkgFormat.Header {
				Signature = r.ReadBytes(6),
				Encryption = r.ReadInt16(),
				HeaderLength = r.ReadInt32(),
				ColorOffset = r.Skip(28).ReadUInt32(),
				ColorLength = r.ReadUInt32(),
				AlphaOffset = r.ReadUInt32(),
				AlphaLength = r.ReadUInt32(),
				FrameOffset = r.ReadUInt32(),
				FrameLength = r.ReadUInt32(),
			};
		}
	}
}