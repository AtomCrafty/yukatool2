using System;
using System.Windows;
using Yuka.Gui.ViewModels;
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
	}
}
