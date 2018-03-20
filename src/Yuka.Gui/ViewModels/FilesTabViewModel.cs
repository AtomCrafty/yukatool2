using System.ComponentModel;
using System.Windows;
using Yuka.IO;

namespace Yuka.Gui.ViewModels {
	public class FilesTabViewModel : ViewModel {

		public FilesTabViewModel() {
			OpenArchiveCommand = new ActionCommand(OpenArchive);
			CloseArchiveCommand = new ActionCommand(CloseArchive, false);
			ExportAllCommand = new ActionCommand(() => { }, false);
		}

		public FileSystemViewModel LoadedFileSystem { get; protected set; }

		public ActionCommand OpenArchiveCommand { get; protected set; }
		public ActionCommand CloseArchiveCommand { get; protected set; }
		public ActionCommand ExportAllCommand { get; protected set; }

		private void OpenArchive() {
			CloseArchive();

			// TODO file choose dialog
			LoadedFileSystem = new FileSystemViewModel(FileSystem.FromArchive(@"S:\Games\Visual Novels\Lover Able\data02.ykc"));

			UpdateCommandAvailability();
		}

		public void CloseArchive() {
			if(LoadedFileSystem == null) return;

			LoadedFileSystem.Close();
			LoadedFileSystem = null;

			UpdateCommandAvailability();
		}

		public void UpdateCommandAvailability() {
			OpenArchiveCommand.Enable();

			if(LoadedFileSystem != null) {
				CloseArchiveCommand.Enable();
				ExportAllCommand.Enable();
			}
			else {
				CloseArchiveCommand.Disable();
				ExportAllCommand.Disable();
			}
		}
	}
}