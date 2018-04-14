using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Yuka.Gui.Jobs;
using Yuka.Gui.Properties;
using Yuka.Gui.Services;
using Yuka.Gui.Services.Abstract;
using Yuka.IO;

namespace Yuka.Gui.ViewModels {
	public class FilesTabViewModel : ViewModel {

		public FilesTabViewModel() {
			OpenArchiveCommand = new ActionCommand(OpenArchive);
			CloseArchiveCommand = new ActionCommand(CloseArchive, false);
			SaveArchiveCommand = new ActionCommand(SaveArchive, false);
			ExportAllCommand = new ActionCommand(ExportAllFiles, false);
		}

		public FileSystemViewModel LoadedFileSystem { get; protected set; }

		public ActionCommand OpenArchiveCommand { get; protected set; }
		public ActionCommand CloseArchiveCommand { get; protected set; }
		public ActionCommand SaveArchiveCommand { get; protected set; }
		public ActionCommand ExportAllCommand { get; protected set; }

		public bool IsFileSystemLoading { get; protected set; }
		public bool IsFileSystemValid => !IsFileSystemLoading && LoadedFileSystem != null;

		private void OpenArchive() {
			// select archive file
			// TODO default path
			Log.Note(Resources.IO_ArchiveSelectionStarted, Resources.Tag_IO);
			string path = Service.Get<IFileService>().SelectArchiveFile(@"S:\Games\Visual Novels\Lover Able\");
			if(string.IsNullOrWhiteSpace(path)) {
				Log.Warn(Resources.IO_ArchiveSelectionAbortedByUser, Resources.Tag_IO);
				return;
			}
			Log.Note(string.Format(Resources.IO_ArchiveSelectionEnded, path), Resources.Tag_IO);

			LoadArchive(path);
		}

		public void LoadArchive(string path) {
			IsFileSystemLoading = true;
			CloseArchive();
			LoadedFileSystem = FileSystemViewModel.Pending;

			Log.Info(string.Format(Resources.IO_ArchiveLoading, path), Resources.Tag_IO);

			Task.Run(() => {
				try {
					var fileSystem = new FileSystemViewModel(FileSystem.FromArchive(path));
					Application.Current.Dispatcher.Invoke(() => {
						LoadedFileSystem = fileSystem;
						IsFileSystemLoading = false;
						UpdateCommandAvailability();
					});
				}
				catch(Exception e) {
					Log.Fail(string.Format(Resources.IO_ArchiveLoadFailed, e.GetType().Name, e.Message), Resources.Tag_IO);
					Log.Fail(e.StackTrace, Resources.Tag_IO);
				}
			});
		}

		public void CloseArchive() {
			if(LoadedFileSystem == null) return;

			Log.Info(string.Format(Resources.IO_ArchiveClosing, LoadedFileSystem.FileSystem.Name), Resources.Tag_IO);

			LoadedFileSystem.Close();
			LoadedFileSystem = null;

			UpdateCommandAvailability();
		}

		public void SaveArchive() {
			(LoadedFileSystem?.FileSystem as ArchiveFileSystem)?.Flush();
		}

		public void UpdateCommandAvailability() {
			OpenArchiveCommand.IsEnabled = !IsFileSystemLoading;

			if(IsFileSystemValid) {
				CloseArchiveCommand.Enable();
				SaveArchiveCommand.Enable();
				ExportAllCommand.Enable();
			}
			else {
				CloseArchiveCommand.Disable();
				SaveArchiveCommand.Disable();
				ExportAllCommand.Disable();
			}
		}

		public void ExportAllFiles() {
			// select archive file 
			// TODO default path
			Log.Note(Resources.IO_ExportFolderSelectionStarted, Resources.Tag_IO);
			string path = Service.Get<IFileService>().SelectDirectory("");
			if(string.IsNullOrWhiteSpace(path)) {
				Log.Warn(Resources.IO_FolderSelectionAbortedByUser, Resources.Tag_IO);
				return;
			}
			Log.Note(string.Format(Resources.IO_ExportFolderSelectionEnded, path), Resources.Tag_IO);

			// TODO Temp
			//new ExportAllJob(LoadedFileSystem.FileSystem, path).Execute();
			//return;

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
			catch(Exception e) {
				Log.Fail(string.Format(Resources.IO_ExportAllFailed, e.GetType().Name, e.Message), Resources.Tag_IO);
				Log.Fail(e.StackTrace, Resources.Tag_IO);
			}
		}

		public override string ToString() => GetType().Name + ": " + LoadedFileSystem;
	}
}