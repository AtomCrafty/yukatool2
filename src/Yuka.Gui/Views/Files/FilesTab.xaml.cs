using System.Drawing;
using Yuka.Gui.Services;
using Yuka.Gui.Services.Abstract;

namespace Yuka.Gui.Views.Files {
	/// <summary>
	/// Interaction logic for ArchiveTab.xaml
	/// </summary>
	public partial class FilesTab {

		public FilesTab() {
			InitializeComponent();
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e) {
			Service.Get<ConfirmationService>().ConfirmRemember("SampleConfirmation", "Main message", "Operation details", "Window title");
		}
	}
}