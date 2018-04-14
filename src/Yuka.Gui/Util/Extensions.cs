using System.Windows;
using System.Windows.Media;

namespace Yuka.Gui.Util {
	public static class Extensions {
		public static T FindAnchestor<T>(this DependencyObject current) where T : DependencyObject {
			do {
				if(current is T target) {
					return target;
				}
				current = VisualTreeHelper.GetParent(current);
			}
			while(current != null);
			return null;
		}
	}
}
