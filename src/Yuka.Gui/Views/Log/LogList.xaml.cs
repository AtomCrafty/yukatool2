using System;
using System.Windows;
using System.Windows.Controls;
using Yuka.Gui.Util;

namespace Yuka.Gui.Views.Log {
	/// <summary>
	/// Interaktionslogik für LogPanel.xaml
	/// </summary>
	public partial class LogList {

		public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.Register("AutoScroll", typeof(bool), typeof(LogList), new UIPropertyMetadata(true));

		public bool AutoScroll {
			get => (bool)GetValue(AutoScrollProperty);
			set => SetValue(AutoScrollProperty, (value as bool?).Value);
		}

		public LogList() {
			InitializeComponent();
		}

		private void LogList_OnLoaded(object sender, RoutedEventArgs e) {
			var scrollViewer = this.FindDescendant<ScrollViewer>();
			if(scrollViewer == null) return;

			scrollViewer.ScrollChanged += (s, args) => {
				if(AutoScroll) scrollViewer.ScrollToBottom();
				else if(Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 1) AutoScroll = true;
			};
			scrollViewer.PreviewMouseWheel += (s, args) => {
				if(args.Delta > 0 || Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) >= 1) AutoScroll = false;
			};
		}
	}
}
