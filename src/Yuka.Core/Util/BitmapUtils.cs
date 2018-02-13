using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Yuka.Util {
	public static class BitmapUtils {

		/// <summary>
		/// Copies the last byte of each pixel in src to the last byte of each pixel in dst.
		/// </summary>
		/// <param name="src">Source bitmap</param>
		/// <param name="dst">Destination bitmap</param>
		public static void CopyAlphaChannel(Bitmap src, Bitmap dst) {
			if(src == null || dst == null) return;
			if(src.Size != dst.Size) throw new ArgumentException("Bitmap sizes do not match");

			var rect = new Rectangle(Point.Empty, src.Size);

			var srcFormat = src.PixelFormat;
			var dstFormat = dst.PixelFormat;
			int srcStep = Image.GetPixelFormatSize(srcFormat) / 8;
			int dstStep = Image.GetPixelFormatSize(dstFormat) / 8;

			var srcData = src.LockBits(rect, ImageLockMode.ReadOnly, srcFormat);
			var dstData = dst.LockBits(rect, ImageLockMode.WriteOnly, dstFormat);
			var srcBits = new byte[Math.Abs(srcData.Stride) * srcData.Height];
			var dstBits = new byte[Math.Abs(dstData.Stride) * dstData.Height];

			Marshal.Copy(srcData.Scan0, srcBits, 0, srcBits.Length);
			Marshal.Copy(dstData.Scan0, dstBits, 0, dstBits.Length);

			// alpha byte is the last one
			int srcIndex = srcStep - 1;
			int dstIndex = dstStep - 1;
			while(srcIndex < srcBits.Length && dstIndex < dstBits.Length) {
				dstBits[dstIndex] = srcBits[srcIndex];

				srcIndex += srcStep;
				dstIndex += dstStep;
			}

			Marshal.Copy(dstBits, 0, dstData.Scan0, dstBits.Length);

			src.UnlockBits(srcData);
			dst.UnlockBits(dstData);
		}
	}
}
