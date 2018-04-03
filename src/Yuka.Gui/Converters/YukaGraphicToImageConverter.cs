using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Yuka.Graphics;

namespace Yuka.Gui.Converters {
	[ValueConversion(typeof(YukaGraphic), typeof(BitmapImage))]
	class YukaGraphicToImageConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if(!(value is YukaGraphic graphic)) return null;

			graphic.MergeChannels();
			graphic.EnsureEncoded();

			if(graphic.ColorData == null) return null;

			using(var stream = new MemoryStream(graphic.ColorData)) {
				var bitmap = new BitmapImage();
				bitmap.BeginInit();
				bitmap.StreamSource = stream;
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.EndInit();
				bitmap.Freeze();
				return bitmap;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
