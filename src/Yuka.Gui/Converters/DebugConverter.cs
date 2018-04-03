using System;
using System.Globalization;
using System.Windows.Data;

namespace Yuka.Gui.Converters {
	public class DebugConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return value == null ? "null" : value.GetType().Name + ": " + value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return value;
		}
	}
}