using System.Windows;
using Yuka.Gui.Services;

namespace Yuka.Gui {
	/// <summary>
	/// Interaction logic for "App.xaml"
	/// </summary>
	public partial class App {
		private void App_OnStartup(object sender, StartupEventArgs e) {
			Service.Register(new FileService(MainWindow));
			Service.Register(new ConfirmationService(MainWindow));
			Service.Register(new JobService(MainWindow));
		}
	}
}
