﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Yuka.IO;
using Yuka.Util;

namespace Yuka.Graphics {
	public class YukaGraphic : IDisposable {
		public byte[] ColorData, AlphaData;
		public Bitmap ColorBitmap, AlphaBitmap;
		public Animation Animation;

		public FormatPreference ColorExportFormat = FormatPreference.DefaultGraphics;
		public FormatPreference AlphaExportFormat = FormatPreference.DefaultGraphics;
		public FormatPreference AnimationExportFormat = FormatPreference.DefaultAnimation;

		protected int _width;
		public int Width {
			get {
				// decode to determine size
				if(_width == 0) Decode();
				return _width;
			}
		}

		protected int _height;
		public int Height {
			get {
				// decode to determine size
				if(_height == 0) Decode();
				return _height;
			}
		}

		public YukaGraphic() {
		}

		public YukaGraphic(byte[] colorData, byte[] alphaData, Animation animation) {
			ColorData = colorData;
			AlphaData = alphaData;
			Animation = animation;
		}

		public void MergeChannels() {
			// no alpha channel
			if(AlphaData == null && AlphaBitmap == null && ColorData != null) {
				if(ColorData.StartsWith(Format.Png.Signature) || ColorData.StartsWith(Format.Bmp.Signature)) {
					return;
				}
			}

			Decode();
			if(ColorBitmap != null && ColorBitmap.PixelFormat != PixelFormat.Format32bppArgb) {
				// make sure cb actually has an alpha channel to copy to
				var newColor = new Bitmap(ColorBitmap.Width, ColorBitmap.Height, PixelFormat.Format32bppArgb);
				using(var gr = System.Drawing.Graphics.FromImage(newColor)) {
					gr.DrawImage(ColorBitmap, new Rectangle(0, 0, ColorBitmap.Width, ColorBitmap.Height));
				}

				ColorBitmap.Dispose();
				ColorBitmap = newColor;
			}

			BitmapUtils.CopyAlphaChannel(AlphaBitmap, ColorBitmap);
			AlphaBitmap?.Dispose();
			AlphaBitmap = null;
		}

		#region Decode / Encode

		public bool IsDecoded => ColorBitmap != null || AlphaBitmap != null;
		public bool IsEncoded => ColorData != null || AlphaData != null;

		public void EnsureDecoded() => Decode();
		public void EnsureEncoded() => Encode();

		public bool Decode(bool merge = false) {
			if(IsDecoded) return false;

			Debug.Assert(ColorBitmap == null);
			Debug.Assert(AlphaBitmap == null);

			if(!ColorData.IsNullOrEmpty()) ColorBitmap = FileReader.Decode<Bitmap>("?" + nameof(ColorData), ColorData);
			if(!AlphaData.IsNullOrEmpty()) AlphaBitmap = FileReader.Decode<Bitmap>("?" + nameof(AlphaData), AlphaData);

			if(merge) MergeChannels();

			ColorData = null;
			AlphaData = null;

			var bitmap = ColorBitmap ?? AlphaBitmap;
			_width = bitmap?.Width ?? 0;
			_height = bitmap?.Height ?? 0;

			return true;
		}

		public bool Encode(AlphaMode am = AlphaMode.Discard, ColorMode cm = ColorMode.MergePng) {
			if(IsEncoded) return false;

			Debug.Assert(ColorData == null);
			Debug.Assert(AlphaData == null);

			if(cm == ColorMode.MergePng || cm == ColorMode.MergeGnp) {
				MergeChannels();
			}

			// encode color data
			var buffer = new MemoryStream();
			switch(cm) {
				case ColorMode.MergePng:
				case ColorMode.Png:
					ColorBitmap.Save(buffer, ImageFormat.Png);
					break;
				case ColorMode.MergeGnp:
				case ColorMode.Gnp:
					ColorBitmap.Save(buffer, ImageFormat.Png);
					buffer.Seek(0).WriteBytes(Format.Gnp.Signature);
					break;
				case ColorMode.Bmp:
					ColorBitmap.Save(buffer, ImageFormat.Bmp);
					break;
			}

			ColorData = buffer.Length > 0 ? buffer.ToArray() : null;

			// encode alpha data
			buffer.Seek(0).SetLength(0);
			switch(am) {
				case AlphaMode.Png:
					ColorBitmap.Save(buffer, ImageFormat.Png);
					break;
				case AlphaMode.Gnp:
					ColorBitmap.Save(buffer, ImageFormat.Png);
					buffer.Seek(0).WriteBytes(Format.Gnp.Signature);
					break;
				case AlphaMode.Bmp:
					ColorBitmap.Save(buffer, ImageFormat.Bmp);
					break;
			}

			AlphaData = buffer.Length > 0 ? buffer.ToArray() : null;

			ColorBitmap?.Dispose();
			AlphaBitmap?.Dispose();
			ColorBitmap = null;
			AlphaBitmap = null;

			return true;
		}

		#endregion

		// TODO call dispose
		public void Dispose() {
			ColorBitmap?.Dispose();
			AlphaBitmap?.Dispose();
			GC.SuppressFinalize(this);
		}

		~YukaGraphic() {
			Dispose();
		}
	}

	public enum ColorMode {
		/// <summary>Discard color information</summary>
		Discard,

		/// <summary>Merge alpha channel and encode as png</summary>
		MergePng,

		/// <summary>Merge alpha channel and encode as gnp</summary>
		MergeGnp,

		/// <summary>Encode as png</summary>
		Png,

		/// <summary>Encode as gnp</summary>
		Gnp,

		/// <summary>Encode as bmp</summary>
		Bmp
	}

	public enum AlphaMode {
		/// <summary>Discard alpha channel</summary>
		Discard,

		/// <summary>Encode alpha channel as png</summary>
		Png,

		/// <summary>Encode alpha channel as gnp</summary>
		Gnp,

		/// <summary>Encode alpha channel as bmp</summary>
		Bmp
	}
}