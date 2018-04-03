using System;
using System.Globalization;
using System.Windows.Data;

namespace Yuka.Gui.Converters {
	[ValueConversion(typeof(long), typeof(string))]
	class FileSizeConverter : IValueConverter {
		private static readonly string[] Sizes = { "B", "KB", "MB", "GB", "TB" };

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if(!(value is long val)) return "?";
			if(val < 0) return "-";

			double len = val;
			int order = 0;
			while(len >= 1024 && order < Sizes.Length - 1) {
				order++;
				len = len / 1024;
			}

			return $"{len:0.##} {Sizes[order]}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
