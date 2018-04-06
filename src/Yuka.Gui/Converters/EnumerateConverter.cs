using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Yuka.Gui.Converters {
	[ValueConversion(typeof(object), typeof(IEnumerable<object>))]
	class EnumerateConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return new[] { value };
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return (value as IEnumerable<object>)?.First();
		}
	}
}
