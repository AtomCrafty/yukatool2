using System;
using System.Globalization;
using System.Windows.Data;

namespace Yuka.Gui.Converters {
	[ValueConversion(typeof(DateTime), typeof(string))]
	public class DateTimeToStringConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (value as DateTime?)?.ToString(CultureInfo.CurrentCulture);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}