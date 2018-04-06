using System.Windows;
using Yuka.Gui.Services;

namespace Yuka.Gui {
	/// <summary>
	/// Interaction logic for "App.xaml"
	/// </summary>
	public partial class App : Application {
		private void App_OnStartup(object sender, StartupEventArgs e) {
			ServiceLocator.Register(new FileService(MainWindow));
		}
	}
}
