using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Yuka.Graphics;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class PngFormat : Format {
		public override string Extension => ".png";
		public override string Description => "PNG image file format";
		public override FormatType Type => FormatType.Unpacked;
		public readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
		public readonly string AlphaExtension = ".alpha.png";
	}

	public class PngGraphicReader : FileReader<YukaGraphic> {

		public override Format Format => Png;

		public override bool CanRead(string name, BinaryReader r) {
			long pos = r.BaseStream.Position;
			try {
				return r.ReadBytes(Png.Signature.Length).Matches(Png.Signature);
			}
			finally { r.BaseStream.Position = pos; }
		}

		public override YukaGraphic Read(string name, Stream s) {
			throw new InvalidOperationException("Cannot read graphic from png stream");
		}

		public override YukaGraphic Read(string name, FileSystem fs) {
			// TODO handle pure alpha files
			if(name.EndsWith(Png.AlphaExtension) && fs.FileExists(name.Substring(0, name.Length - Png.AlphaExtension.Length))) return null;

			// load color data
			byte[] colorData;
			using(var r = fs.OpenFile(name).NewReader()) {
				colorData = r.ReadToEnd();
			}

			// check if alpha channel file exists
			string alphaFileName = name.WithExtension(Png.AlphaExtension);
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

	public class PngGraphicWriter : FileWriter<YukaGraphic> {

		public override Format Format => Png;

		public override bool CanWrite(object obj) {
			return obj is YukaGraphic;
		}

		public override void Write(YukaGraphic obj, Stream s) {
			throw new InvalidOperationException("Cannot write graphic to png stream");
		}

		public override void Write(YukaGraphic ykg, string baseName, FileSystem fs) {

			if(ykg.Animation != null) {
				Encode(ykg.Animation, baseName, fs, ykg.AnimationExportFormat);
			}

			if(!ykg.IsDecoded) {
				if(!Options.MergeAlphaChannelOnExport || ykg.AlphaData == null) {
					if(ykg.ColorData.StartsWith(Gnp.Signature)) {
						// changing the signature is faster than re-encoding the image
						for(int i = 0; i < Png.Signature.Length; i++) {
							ykg.ColorData[i] = Png.Signature[i];
						}
					}
					if(ykg.ColorData.StartsWith(Png.Signature)) {

						// data is already in the correct format, so no re-encoding is needed
						using(var s = fs.CreateFile(baseName.WithExtension(Png.Extension))) {
							s.WriteBytes(ykg.ColorData);
						}

						// no alpha channel to save, so we can just return
						if(ykg.AlphaData == null) return;

						if(ykg.AlphaData.StartsWith(Gnp.Signature)) {
							// changing the signature is faster than re-encoding the image
							for(int i = 0; i < Png.Signature.Length; i++) {
								ykg.AlphaData[i] = Png.Signature[i];
							}
						}
						if(ykg.AlphaData.StartsWith(Png.Signature)) {
							using(var s = fs.CreateFile(baseName.WithExtension(Png.AlphaExtension))) {
								s.WriteBytes(ykg.AlphaData);
							}
						}
						else {
							using(var bitmap = FileReader.Decode<Bitmap>("?" + nameof(ykg.AlphaBitmap), ykg.AlphaData)) {
								Encode(bitmap, baseName.WithExtension(Png.AlphaExtension), fs, new FormatPreference(Png));
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

			Debug.Assert(ykg.IsDecoded);

			if(ykg.AlphaBitmap != null) {
				if(Options.MergeAlphaChannelOnExport) {
					// merge alpha channel into color bitmap
					ykg.MergeChannels();
				}
				else {
					// save alpha channel on its own
					Encode(ykg.AlphaBitmap, baseName.WithExtension(Png.AlphaExtension), fs, new FormatPreference(Png));
				}
			}

			if(ykg.ColorBitmap != null) {
				Encode(ykg.ColorBitmap, baseName, fs, new FormatPreference(Png));
			}
		}
	}

	public class PngBitmapReader : FileReader<Bitmap> {

		public override Format Format => Png;

		public override bool CanRead(string name, BinaryReader r) {
			long pos = r.BaseStream.Position;
			try {
				var signature = r.ReadBytes(Png.Signature.Length);
				return signature.Matches(Png.Signature);
			}
			finally { r.BaseStream.Position = pos; }
		}

		public override Bitmap Read(string name, Stream s) {
			return new Bitmap(s);
		}
	}

	public class PngBitmapWriter : FileWriter<Bitmap> {

		public override Format Format => Png;

		public override bool CanWrite(object obj) {
			return obj is Bitmap;
		}

		public override void Write(Bitmap bmp, Stream s) {
			bmp.Save(s, ImageFormat.Png);
		}
	}
}