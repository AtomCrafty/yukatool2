using System;
using System.Windows;
using Yuka.Gui.ViewModels;
using Yuka.Gui.Views.Log;
using Yuka.IO;
using Yuka.IO.Formats;

namespace Yuka.Gui.Views {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
		public MainWindow() {
			InitializeComponent();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
			// TODO temp
			//new LogWindow { WindowStartupLocation = WindowStartupLocation.Manual, Left = 10, Top = 50, Height = 1000 }.Show();
			Left = 550;
			Top = 300;
			Focus();

			var args = Environment.GetCommandLineArgs();
			if(args.Length <= 1) return;

			switch(Format.GuessFromFileName(args[1])) {
				case AniFormat _:
					break;
				case BmpFormat _:
					break;
				case CsvFormat _:
					break;
				case FrmFormat _:
					break;
				case GnpFormat _:
					break;
				case PngFormat _:
					break;
				case RawFormat _:
					break;
				case TxtFormat _:
					break;
				case YkcFormat _:
					(FindResource("FilesTabViewModel") as FilesTabViewModel)?.LoadArchive(args[1]);
					break;
				case YkdFormat _:
					break;
				case YkgFormat _:
					break;
				case YkiFormat _:
					break;
				case YksFormat _:
					break;
			}
		}

		private void MainWindow_OnClosed(object sender, EventArgs e) {
			Application.Current.Shutdown();
		}
	}
}
