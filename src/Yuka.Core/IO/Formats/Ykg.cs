using System.Diagnostics;
using System.IO;
using System.Text;
using Yuka.Graphics;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class YkgFormat : Format {
		public override string Extension => ".ykg";
		public override FormatType Type => FormatType.Packed;

		public readonly byte[] Signature = Encoding.ASCII.GetBytes("YKG000");
		public readonly int HeaderLength = 0x40;

		internal Header DummyHeader => new Header { Signature = Signature, HeaderLength = HeaderLength };

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

	public class YkgGraphicReader : FileReader<YukaGraphic> {

		public override Format Format => Ykg;

		public override bool CanRead(string name, BinaryReader r) {
			long pos = r.BaseStream.Position;
			try {
				var signature = r.ReadBytes(Ykg.Signature.Length);
				return signature.Matches(Ykg.Signature);
			}
			finally { r.BaseStream.Position = pos; }
		}

		public override YukaGraphic Read(string name, Stream s) {

			var r = s.NewReader();
			var header = ReadHeader(r);

			Debug.Assert(header.Signature.Matches(Ykg.Signature));
			Debug.Assert(header.HeaderLength == Ykg.HeaderLength);
			Debug.Assert(header.Encryption == 0);

			var colorData = r.Seek(header.ColorOffset).ReadBytes((int)header.ColorLength).NullIfEmpty();
			var alphaData = r.Seek(header.AlphaOffset).ReadBytes((int)header.AlphaLength).NullIfEmpty();
			var frameData = r.Seek(header.FrameOffset).ReadBytes((int)header.FrameLength).NullIfEmpty();

			return new YukaGraphic(colorData, alphaData, Animation.FromFrameData(frameData));
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
				FrameLength = r.ReadUInt32()
			};
		}
	}

	public class YkgGraphicWriter : FileWriter<YukaGraphic> {

		public override Format Format => Ykg;

		public override bool CanWrite(object obj) {
			return obj is YukaGraphic;
		}

		public override void Write(YukaGraphic ykg, Stream s) {
			ykg.EnsureEncoded();
			var w = s.NewWriter();

			WriteHeader(Ykg.DummyHeader, w);

			long colorOffset = 0, colorLength = 0;
			if(ykg.ColorData != null) {
				colorOffset = s.Position;
				colorLength = ykg.ColorData.LongLength;
				w.Write(ykg.ColorData);
			}

			long alphaOffset = 0, alphaLength = 0;
			if(ykg.AlphaData != null) {
				alphaOffset = s.Position;
				alphaLength = ykg.AlphaData.LongLength;
				w.Write(ykg.AlphaData);
			}

			long frameOffset = 0, frameLength = 0;
			if(ykg.Animation != null) {
				frameOffset = s.Position;
				Encode(ykg.Animation, s, new FormatPreference(Frm));
				frameLength = s.Position - frameOffset;
			}

			long end = s.Position;
			s.Seek(0);
			WriteHeader(new YkgFormat.Header {
				Signature = Ykg.Signature,
				Encryption = 0,
				HeaderLength = Ykg.HeaderLength,

				ColorOffset = (uint)colorOffset,
				ColorLength = (uint)colorLength,
				AlphaOffset = (uint)alphaOffset,
				AlphaLength = (uint)alphaLength,
				FrameOffset = (uint)frameOffset,
				FrameLength = (uint)frameLength
			}, w);
			s.Seek(end);
		}

		internal static void WriteHeader(YkgFormat.Header header, BinaryWriter w) {
			w.Write(header.Signature);
			w.Write(header.Encryption);
			w.Write(header.HeaderLength);
			w.Write(new byte[28]);
			w.Write(header.ColorOffset);
			w.Write(header.ColorLength);
			w.Write(header.AlphaOffset);
			w.Write(header.AlphaLength);
			w.Write(header.FrameOffset);
			w.Write(header.FrameLength);
		}
	}
}