using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Yuka.Graphics;

namespace Yuka.Gui.Converters {
	[ValueConversion(typeof(YukaGraphic), typeof(BitmapSource))]
	class YukaGraphicToImageConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if(!(value is YukaGraphic graphic)) return null;

			graphic.Decode(true);

			if(graphic.ColorBitmap == null) return null;
			BitmapSource bitSrc;

			var hBitmap = graphic.ColorBitmap.GetHbitmap();

			try {
				bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
					hBitmap,
					IntPtr.Zero,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());
			}
			catch(Win32Exception) {
				bitSrc = null;
			}
			finally {
				DeleteObject(hBitmap);
			}

			return bitSrc;
		}

		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DeleteObject(IntPtr hObject);

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
