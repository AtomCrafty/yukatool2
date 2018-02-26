using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Yuka.Graphics;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class BmpFormat : Format {
		public override string Extension => ".bmp";
		public override string Description => "Windows Bitmap image file format";
		public override FormatType Type => FormatType.Unpacked;
		public readonly byte[] Signature = Encoding.ASCII.GetBytes("BM");
		public readonly string AlphaExtension = ".alpha.bmp";
	}

	public class BmpGraphicReader : FileReader<YukaGraphic> {

		public override Format Format => Bmp;

		public override bool CanRead(string name, BinaryReader r) {
			long pos = r.BaseStream.Position;
			try {
				return r.ReadBytes(Bmp.Signature.Length).Matches(Bmp.Signature);
			}
			finally { r.BaseStream.Position = pos; }
		}

		public override YukaGraphic Read(string name, Stream s) {
			throw new InvalidOperationException("Cannot read graphic from bmp stream");
		}

		public override YukaGraphic Read(string name, FileSystem fs) {
			// TODO handle pure alpha files
			if(name.EndsWith(Bmp.AlphaExtension) && fs.FileExists(name.Substring(0, name.Length - Bmp.AlphaExtension.Length))) return null;

			// load color data
			byte[] colorData;
			using(var r = fs.OpenFile(name).NewReader()) {
				colorData = r.ReadToEnd();
			}

			// check if alpha channel file exists
			string alphaFileName = name.WithExtension(Bmp.AlphaExtension);
			byte[] alphaData = null;
			if(fs.FileExists(alphaFileName)) {
				using(var r = fs.OpenFile(alphaFileName).NewReader()) {
					alphaData = r.ReadToEnd();
				}
			}

			// load animation file if there is one
			string aniFileName = name.WithExtension(Ani.Extension);
			string frmFileName = name.WithExtension(Frm.Extension);
			var animation =
				fs.FileExists(aniFileName)
					? Decode<Animation>(aniFileName, fs)
					: fs.FileExists(frmFileName)
						? Decode<Animation>(frmFileName, fs)
						: null;

			return new YukaGraphic { ColorData = colorData, AlphaData = alphaData, Animation = animation };
		}
	}

	public class BmpGraphicWriter : FileWriter<YukaGraphic> {

		public override Format Format => Bmp;

		public override bool CanWrite(object obj) {
			return obj is YukaGraphic;
		}

		public override void Write(YukaGraphic obj, Stream s) {
			throw new InvalidOperationException("Cannot write graphic to bmp stream");
		}

		public override void Write(YukaGraphic ykg, string baseName, FileSystem fs) {

			// write animaton data
			if(ykg.Animation != null) {
				Encode(ykg.Animation, baseName, fs, ykg.AnimationExportFormat);
			}

			if(!ykg.IsDecoded) {
				if(!Options.YkgMergeAlphaChannelOnExport || ykg.AlphaData == null) {
					if(ykg.ColorData.StartsWith(Bmp.Signature)) {

						// data is already in the correct format, so no re-encoding is needed
						using(var s = fs.CreateFile(baseName.WithExtension(Bmp.Extension))) {
							s.WriteBytes(ykg.ColorData);
						}

						// no alpha channel to save, so we can just return
						if(ykg.AlphaData == null) return;

						if(ykg.AlphaData.StartsWith(Png.Signature)) {
							using(var s = fs.CreateFile(baseName.WithExtension(Bmp.AlphaExtension))) {
								s.WriteBytes(ykg.AlphaData);
							}
						}
						else {
							using(var bitmap = FileReader.Decode<Bitmap>("?" + nameof(ykg.AlphaBitmap), ykg.AlphaData)) {
								Encode(bitmap, baseName.WithExtension(Png.AlphaExtension), fs, new FormatPreference(Bmp));
							}
						}

						return;
					}
				}

				// the current format is not the requested output format, so we need to re-encode everything
				ykg.Decode();
				if(!ykg.IsDecoded) {
					// TODO Warning
					Console.WriteLine("Decode failed for graphic: " + baseName);
					// decode failed -> don't write any file
					return;
				}
			}

			// merge or write alpha channel
			if(ykg.AlphaBitmap != null) {
				if(Options.YkgMergeAlphaChannelOnExport) {
					// merge alpha channel into color bitmap
					ykg.MergeChannels();
				}
				else {
					// save alpha channel on its own
					Encode(ykg.AlphaBitmap, baseName.WithExtension(Bmp.AlphaExtension), fs, new FormatPreference(Bmp));
				}
			}

			// write color data
			if(ykg.ColorBitmap != null) {
				Encode(ykg.ColorBitmap, baseName, fs, new FormatPreference(Bmp));
			}
		}
	}

	public class BmpBitmapReader : FileReader<Bitmap> {

		public override Format Format => Bmp;

		public override bool CanRead(string name, BinaryReader r) {
			long pos = r.BaseStream.Position;
			try {
				var signature = r.ReadBytes(Bmp.Signature.Length);
				return signature.Matches(Bmp.Signature);
			}
			finally { r.BaseStream.Position = pos; }
		}

		public override Bitmap Read(string name, Stream s) {
			return new Bitmap(s);
		}
	}

	public class BmpBitmapWriter : FileWriter<Bitmap> {

		public override Format Format => Bmp;

		public override bool CanWrite(object obj) {
			return obj is Bitmap;
		}

		public override void Write(Bitmap bmp, Stream s) {
			bmp.Save(s, ImageFormat.Bmp);
		}
	}
}