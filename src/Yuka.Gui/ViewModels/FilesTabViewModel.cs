using Yuka.Gui.Services;
using Yuka.Gui.Services.Abstract;
using Yuka.IO;

namespace Yuka.Gui.ViewModels {
	public class FilesTabViewModel : ViewModel {

		public FilesTabViewModel() {
			OpenArchiveCommand = new ActionCommand(OpenArchive);
			CloseArchiveCommand = new ActionCommand(CloseArchive, false);
			ExportAllCommand = new ActionCommand(ExportAllFiles, false);
		}

		public FileSystemViewModel LoadedFileSystem { get; protected set; }

		public ActionCommand OpenArchiveCommand { get; protected set; }
		public ActionCommand CloseArchiveCommand { get; protected set; }
		public ActionCommand ExportAllCommand { get; protected set; }

		private void OpenArchive() {
			CloseArchive();

			// select file
			string path = ServiceLocator.GetService<IFileService>().SelectArchiveFile(@"S:\Games\Visual Novels\Lover Able\");
			if(string.IsNullOrWhiteSpace(path)) return;

			LoadedFileSystem = new FileSystemViewModel(FileSystem.FromArchive(path));

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

		public void ExportAllFiles() {

		}
	}
}