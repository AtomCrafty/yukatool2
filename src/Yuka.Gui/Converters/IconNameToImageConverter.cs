using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Yuka.Gui.Properties;

namespace Yuka.Gui.Converters {
	[ValueConversion(typeof(string), typeof(BitmapImage))]
	public class IconNameToImageConverter : IValueConverter {

		protected readonly Dictionary<string, WeakReference<BitmapImage>> ImageCache = new Dictionary<string, WeakReference<BitmapImage>>();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

			if(!(value is string icon)) return null;
			if(ImageCache.ContainsKey(icon) && ImageCache[icon].TryGetTarget(out var image)) return image;

			Log.Debug(string.Format(Resources.System_IconLoadCacheMiss, icon), Resources.Tag_UI);
			var uri = new Uri($@"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/res/images/{icon}.png");

			image = new BitmapImage(uri);
			ImageCache[icon] = new WeakReference<BitmapImage>(image);
			return image;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
