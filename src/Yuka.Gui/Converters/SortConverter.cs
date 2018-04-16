using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace Yuka.Gui.Converters {
	[ValueConversion(typeof(IList), typeof(ListCollectionView))]
	public class SortConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if(!(value is IList list)) return null;
			var view = new ListCollectionView(list);
			view.SortDescriptions.Add(new SortDescription(parameter?.ToString() ?? throw new ArgumentNullException(nameof(parameter)), ListSortDirection.Ascending));
			return view;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return null;
		}
	}
}
