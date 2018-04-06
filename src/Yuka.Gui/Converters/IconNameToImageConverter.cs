using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Yuka.Gui.Converters {
	[ValueConversion(typeof(string), typeof(BitmapImage))]
	public class IconNameToImageConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var uri = new Uri($@"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/res/images/{value}.png");
			return new BitmapImage(uri);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
