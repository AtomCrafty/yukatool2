using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Yuka.Gui.Util {
	public static class Extensions {

		public static T FindAnchestor<T>(this DependencyObject current) where T : DependencyObject {
			do {
				if(current is T target) return target;

				current = VisualTreeHelper.GetParent(current);
			}
			while(current != null);
			return null;
		}

		public static T FindDescendant<T>(this DependencyObject node) where T : DependencyObject {
			var queue = new Queue<DependencyObject>();
			queue.Enqueue(node);

			while(queue.Any()) {
				var current = queue.Dequeue();
				switch(current) {

					case null:
						continue;

					case T target:
						return target;

					default:
						int count = VisualTreeHelper.GetChildrenCount(current);
						for(int i = 0; i < count; i++) queue.Enqueue(VisualTreeHelper.GetChild(current, i));
						break;
				}
			}
			return null;
		}
	}
}
