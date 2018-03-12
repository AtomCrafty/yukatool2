using System.IO;
using Yuka.Graphics;
using Yuka.Util;

namespace Yuka.IO.Formats {

	public class FrmFormat : Format {
		public override string Extension => ".frm";
		public override string Description => "Binary frame animation data";
		public readonly int FrameSize = 0x20;
		public override FormatType Type => FormatType.Packed;

		public override FileCategory GetFileType(FileSystem fs, string fileName) {
			// when a png or bmp with the same name exists, this ani belongs to it
			return fs.FileExists(fileName.WithExtension(Png.Extension))
				   || fs.FileExists(fileName.WithExtension(Bmp.Extension)) ? FileCategory.Secondary : FileCategory.Primary;
		}
	}

	public class FrmAnimationReader : FileReader<Animation> {

		public override Format Format => Format.Frm;

		public override bool CanRead(string name, BinaryReader r) {
			return ('.' + name.ToLower()).EndsWith(".frm");
		}

		public override Animation Read(string name, Stream s) {
			return Animation.FromFrameData(s.NewReader().ReadToEnd());
		}
	}

	public class FrmAnimationWriter : FileWriter<Animation> {

		public override Format Format => Format.Frm;

		public override bool CanWrite(object obj) {
			return obj is Animation;
		}

		public override void Write(Animation ani, Stream s) {
			ani.EnsureEncoded();
			s.WriteBytes(ani.FrameData);
		}
	}
}