using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
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

		public bool IsFileSystemLoading { get; protected set; }
		public bool IsFileSystemValid => !IsFileSystemLoading && LoadedFileSystem != null;

		private void OpenArchive() {
			// select archive file TODO remove default path
			string path = ServiceLocator.GetService<IFileService>().SelectArchiveFile(@"S:\Games\Visual Novels\Lover Able\");
			if(string.IsNullOrWhiteSpace(path)) return;

			IsFileSystemLoading = true;
			CloseArchive();
			LoadedFileSystem = FileSystemViewModel.Pending;

			Task.Run(() => {
				var fileSystem = new FileSystemViewModel(FileSystem.FromArchive(path));
				Application.Current.Dispatcher.Invoke(() => {
					LoadedFileSystem = fileSystem;
					IsFileSystemLoading = false;
					UpdateCommandAvailability();
				});
			});
		}

		public void CloseArchive() {
			if(LoadedFileSystem == null) return;

			LoadedFileSystem.Close();
			LoadedFileSystem = null;

			UpdateCommandAvailability();
		}

		public void UpdateCommandAvailability() {
			OpenArchiveCommand.IsEnabled = !IsFileSystemLoading;

			if(IsFileSystemValid) {
				CloseArchiveCommand.Enable();
				ExportAllCommand.Enable();
			}
			else {
				CloseArchiveCommand.Disable();
				ExportAllCommand.Disable();
			}
		}

		public void ExportAllFiles() {
			// select archive file TODO default path
			string path = ServiceLocator.GetService<IFileService>().SelectDirectory("");
			try {
				var srcFs = LoadedFileSystem.FileSystem;
				using(var dstFs = FileSystem.NewFolder(path)) {
					foreach(string file in srcFs.GetFiles()) {
						using(var srcFile = srcFs.OpenFile(file))
						using(var dstFile = dstFs.CreateFile(file)) {
							srcFile.CopyTo(dstFile);
						}
					}
				}
			}
			catch(IOException e) {
				Console.WriteLine(e);
			}
		}

		public override string ToString() => GetType().Name + ": " + LoadedFileSystem;
	}
}