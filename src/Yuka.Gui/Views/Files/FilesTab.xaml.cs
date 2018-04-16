using System.Windows;
using Yuka.Gui.Services;

namespace Yuka.Gui.Views.Files {
	/// <summary>
	/// Interaction logic for ArchiveTab.xaml
	/// </summary>
	public partial class FilesTab {

		public FilesTab() {
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			Service.Get<ConfirmationService>().ConfirmAndRemember("SampleConfirmation", "Main message", "Operation details", "Window title");
		}
	}
}