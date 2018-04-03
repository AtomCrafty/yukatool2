using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Yuka.Gui.Services;

namespace Yuka.Gui {
	/// <summary>
	/// Interaktionslogik für "App.xaml"
	/// </summary>
	public partial class App : Application {
		private void App_OnStartup(object sender, StartupEventArgs e) {
			ServiceLocator.Register(new FileService(MainWindow));
		}
	}
}
