using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Yuka.Graphics;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class GnpFormat : Format {
		public override string Id => "gnp";
		public override string Extension => ".gnp";
		public override string Description => "PNG image file format with modified signature";
		public override FormatType Type => FormatType.Unpacked;
		public readonly byte[] Signature = { 137, 71, 78, 80, 13, 10, 26, 10 };
		public readonly string AlphaExtension = ".alpha.gnp";
	}

	// useless, since we usually don't read gnp files from disk
	public class GnpGraphicReader : FileReader<YukaGraphic> {

		public override Format Format => Gnp;

		public override bool CanRead(string name, BinaryReader r) {
			long pos = r.BaseStream.Position;
			try {
				return r.ReadBytes(Gnp.Signature.Length).Matches(Gnp.Signature);
			}
			finally { r.BaseStream.Position = pos; }
		}

		public override YukaGraphic Read(string name, Stream s) {
			throw new InvalidOperationException("Cannot read graphic from gnp stream");
		}

		public override YukaGraphic Read(string name, FileSystem fs) {
			// TODO handle pure alpha files
			if(name.EndsWith(Gnp.AlphaExtension) && fs.FileExists(name.Substring(0, name.Length - Gnp.AlphaExtension.Length))) return null;

			// load color data
			byte[] colorData;
			using(var r = fs.OpenFile(name).NewReader()) {
				colorData = r.ReadToEnd();
			}

			Debug.Assert(colorData.Length >= Gnp.Signature.Length);

			// change file header
			for(int i = 0; i < Png.Signature.Length; i++) {
				colorData[i] = Png.Signature[i];
			}

			// check if alpha channel file exists
			string alphaFileName = name.WithExtension(Gnp.AlphaExtension);
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

	// useless, since we usually don't write gnp files to disk
	public class GnpGraphicWriter : FileWriter<YukaGraphic> {

		public override Format Format => Gnp;

		public override bool CanWrite(object obj) {
			return obj is YukaGraphic;
		}

		public override void Write(YukaGraphic obj, Stream s) {
			throw new InvalidOperationException("Cannot write graphic to gnp stream");
		}

		public override void Write(YukaGraphic ykg, string baseName, FileSystem fs) {

			if(ykg.Animation != null) {
				Encode(ykg.Animation, baseName, fs, ykg.AnimationExportFormat);
			}

			if(!ykg.IsDecoded) {
				if(!Options.YkgMergeAlphaChannelOnExport || ykg.AlphaData == null) {
					if(ykg.ColorData.StartsWith(Png.Signature)) {
						// changing the signature is faster than re-encoding the image
						for(int i = 0; i < Gnp.Signature.Length; i++) {
							ykg.ColorData[i] = Gnp.Signature[i];
						}
					}
					if(ykg.ColorData.StartsWith(Gnp.Signature)) {

						// data is already in the correct format, so no re-encoding is needed
						using(var s = fs.CreateFile(baseName.WithExtension(Gnp.Extension))) {
							s.WriteBytes(ykg.ColorData);
						}

						// no alpha channel to save, so we can just return
						if(ykg.AlphaData == null) return;

						if(ykg.AlphaData.StartsWith(Png.Signature)) {
							// changing the signature is faster than re-encoding the image
							for(int i = 0; i < Gnp.Signature.Length; i++) {
								ykg.AlphaData[i] = Gnp.Signature[i];
							}
						}
						if(ykg.AlphaData.StartsWith(Gnp.Signature)) {
							using(var s = fs.CreateFile(baseName.WithExtension(Gnp.AlphaExtension))) {
								s.WriteBytes(ykg.AlphaData);
							}
						}
						else {
							using(var bitmap = FileReader.Decode<Bitmap>("?" + nameof(ykg.AlphaBitmap), ykg.AlphaData)) {
								Encode(bitmap, baseName.WithExtension(Gnp.AlphaExtension), fs, new FormatPreference(Gnp));
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
				if(Options.YkgMergeAlphaChannelOnExport) {
					// merge alpha channel into color bitmap
					ykg.MergeChannels();
				}
				else {
					// save alpha channel on its own
					Encode(ykg.AlphaBitmap, baseName.WithExtension(Gnp.AlphaExtension), fs, new FormatPreference(Gnp));
				}
			}

			if(ykg.ColorBitmap != null) {
				Encode(ykg.ColorBitmap, baseName, fs, new FormatPreference(Gnp));
			}
		}
	}

	public class GnpBitmapReader : FileReader<Bitmap> {

		public override Format Format => Gnp;

		public override bool CanRead(string name, BinaryReader r) {
			long pos = r.BaseStream.Position;
			try {
				return r.ReadBytes(Gnp.Signature.Length).Matches(Gnp.Signature);
			}
			finally { r.BaseStream.Position = pos; }
		}

		public override Bitmap Read(string name, Stream s) {

			// read gnp data
			var colorData = s.NewReader().ReadToEnd();

			Debug.Assert(colorData.Length >= Gnp.Signature.Length);

			// change file header
			for(int i = 0; i < Png.Signature.Length; i++) {
				colorData[i] = Png.Signature[i];
			}

			using(var ms = new MemoryStream(colorData)) {
				return new Bitmap(ms);
			}
		}
	}

	public class GnpBitmapWriter : FileWriter<Bitmap> {

		public override Format Format => Gnp;

		public override bool CanWrite(object obj) {
			return obj is Bitmap;
		}

		public override void Write(Bitmap bmp, Stream s) {
			// save png data
			long start = s.Position;
			bmp.Save(s, ImageFormat.Png);
			long end = s.Position;

			// change signature
			s.Seek(start).WriteBytes(Gnp.Signature);
			s.Seek(end);
		}
	}
}