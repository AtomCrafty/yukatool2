using Yuka.Gui.Configuration;
using Yuka.Gui.Properties;
using Yuka.Gui.Services;
using Yuka.Gui.Services.Abstract;

namespace Yuka.Gui.ViewModels {
	public class ConfigTabViewModel : ViewModel {
		public ConfigTabViewModel() {
			ExportConfigCommand = new ActionCommand(ExportConfig);
			ImportConfigCommand = new ActionCommand(ImportConfig);
			ResetConfigCommand = new ActionCommand(ResetConfig);
		}

		public ActionCommand ExportConfigCommand { get; protected set; }
		public ActionCommand ImportConfigCommand { get; protected set; }
		public ActionCommand ResetConfigCommand { get; protected set; }

		public Config Config => Config.Current;





		private static void ExportConfig() {
			string path = Service.Get<IFileService>().SaveFile(Resources.UI_ConfigFilter, ".cfg", Resources.UI_ConfigExportDialogTitle);
			if(path != null) Config.Save(path);
		}

		private static void ImportConfig() {
			if(!Service.Get<ConfirmationService>().ConfirmAndRemember("ImportConfiguration")) return;

			string path = Service.Get<IFileService>().OpenFile(Resources.UI_ConfigFilter, ".cfg", Resources.UI_ConfigImportDialogTitle);
			if(path != null) Config.Load(path);
		}

		private static void ResetConfig() {
			if(!Service.Get<ConfirmationService>().ConfirmAndRemember("ResetConfiguration")) return;

			Config.Reset();
		}
	}
}
