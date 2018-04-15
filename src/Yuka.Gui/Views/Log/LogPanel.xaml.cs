using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Yuka.Gui.Views.Log {
	/// <summary>
	/// Interaktionslogik für LogPanel.xaml
	/// </summary>
	public partial class LogPanel {
		public LogPanel() {
			InitializeComponent();
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e) {
			Gui.Log.Clear();
			Gui.Log.Note(Properties.Resources.System_LogCleared);
		}
	}
}
