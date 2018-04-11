using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Yuka.Gui.Converters {
	[ValueConversion(typeof(LogSeverity), typeof(Brush))]
	public class SeverityToBrushConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if(!(value is LogSeverity severity)) throw new ArgumentOutOfRangeException(nameof(value), value, null);

			switch(severity) {
				case LogSeverity.Debug: return Brushes.LimeGreen;
				case LogSeverity.Note: return Brushes.Gray;
				case LogSeverity.Info: return Brushes.DodgerBlue;
				case LogSeverity.Warn: return Brushes.Orange;
				case LogSeverity.Error: return Brushes.Red;
				default: throw new ArgumentOutOfRangeException(nameof(value), value, null);
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
