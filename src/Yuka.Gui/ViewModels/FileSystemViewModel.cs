using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yuka.Gui.Properties;
using Yuka.IO;

namespace Yuka.Gui.ViewModels {
	public class FileSystemViewModel : ViewModel {

		public static readonly FileSystemViewModel Pending = new FileSystemPendingViewModel();
		public static readonly FileSystemViewModel Design = new DesignModeFileSystemViewModel();

		internal readonly FileSystem FileSystem;
		public ShellItemViewModel Root { get; protected set; }

		protected readonly Dictionary<string, ShellItemViewModel> Nodes = new Dictionary<string, ShellItemViewModel>();

		#region Initialization

		public FileSystemViewModel(FileSystem fileSystem) {
			FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

			Root = new ShellItemViewModel(this, null, fileSystem.Name, ShellItemType.Root);

			foreach(string file in FileSystem.GetFiles()) {
				CreateNode(file, ShellItemType.File, Nodes);
			}
		}

		/// <summary>
		/// Adds a file/directory node and all its parents to the directory tree if it doesn't exist yet.
		/// </summary>
		/// <param name="path">Full path of the added element within the file system</param>
		/// <param name="type">Type of the element to be added (file/directory)</param>
		/// <param name="nodes">A directory containing all previously created nodes</param>
		/// <returns>The node corresponding to the specified path</returns>
		protected ShellItemViewModel CreateNode(string path, ShellItemType type, Dictionary<string, ShellItemViewModel> nodes) {
			if(nodes.ContainsKey(path)) return nodes[path];

			string parentPath = Path.GetDirectoryName(path);
			var parent = !string.IsNullOrWhiteSpace(parentPath) ? CreateNode(parentPath, ShellItemType.Directory, nodes) : Root;

			var node = new ShellItemViewModel(this, parent, path, type);
			parent.Children.Add(node);

			nodes[path] = node;
			return node;
		}

		#endregion

		#region IO Methods

		#region Import

		public void AddFile(string localFilePath, Stream srcStream) {
			using(var dstStream = FileSystem.CreateFile(localFilePath)) {
				srcStream.CopyTo(dstStream);
				dstStream.Flush();
				CreateNode(localFilePath, ShellItemType.File, Nodes);
			}
		}

		public void ImportFolder(string folderPath, string localBasePath) {
			try {
				string localPath = Path.Combine(localBasePath, Path.GetFileName(folderPath) ?? "");
				int fileCount = 0;

				using(var srcFs = FileSystem.FromFolder(folderPath)) {
					foreach(string file in srcFs.GetFiles()) {
						string localFilePath = Path.Combine(localPath, file);
						using(var srcStream = srcFs.OpenFile(file)) {
							AddFile(localFilePath, srcStream);
							fileCount++;
						}
					}
				}
				Log.Note(string.Format(Resources.IO_ImportFolderFinished, fileCount, folderPath, localPath), Resources.Tag_IO);
			}
			catch(Exception e) {
				Log.Fail(string.Format(Resources.IO_ImportFolderFailed, e.GetType().Name, e.Message), Resources.Tag_IO);
				Log.Fail(e.StackTrace, Resources.Tag_IO);
			}
		}

		public void ImportFile(string filePath, string localBasePath) {
			try {
				string localFilePath = Path.Combine(localBasePath, Path.GetFileName(filePath) ?? "");
				using(var srcStream = File.Open(filePath, FileMode.Open)) {
					AddFile(localFilePath, srcStream);
				}
				Log.Note(string.Format(Resources.IO_ImportFileFinished, filePath, localFilePath), Resources.Tag_IO);
			}
			catch(Exception e) {
				Log.Fail(string.Format(Resources.IO_ImportFileFailed, e.GetType().Name, e.Message), Resources.Tag_IO);
				Log.Fail(e.StackTrace, Resources.Tag_IO);
			}
		}

		public void ImportFileOrFolder(string path, string localBasePath) {
			if(Directory.Exists(path)) ImportFolder(path, localBasePath);
			else if(File.Exists(path)) ImportFile(path, localBasePath);
			else Log.Warn(string.Format(Resources.IO_ImportFileNotFound, path), Resources.Tag_IO);
		}

		#endregion

		#region Delete

		public void DeleteFileOrFolder(ShellItemViewModel item) {
			switch(item.Type) {

				case ShellItemType.Directory:
					Log.Info(string.Format(Resources.IO_DeletingDirectoryFromArchive, item.FullPath), Resources.Tag_IO);
					// create shallow copy to iterate over (because collection is modified in loop)
					foreach(var child in item.Children.ToList()) {
						DeleteFileOrFolder(child);
					}
					item.Parent.Children.Remove(item);
					Nodes.Remove(item.FullPath);
					break;

				case ShellItemType.File:
					Log.Info(string.Format(Resources.IO_DeletingFileFromArchive, item.FullPath), Resources.Tag_IO);
					// delete file from archive
					FileSystem.DeleteFile(item.FullPath);
					// remove node from tree view
					item.Parent.Children.Remove(item);
					Nodes.Remove(item.FullPath);
					break;

				case ShellItemType.Root:
					Log.Fail(Resources.IO_CannotDeleteRootNode, Resources.Tag_IO);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Export

		public void ExportFileOrFolder(ShellItemViewModel shellItemViewModel) {
			Log.Fail("FileSystemViewModel.ExportFileOrFolder is not implemented yet", Resources.Tag_System);
		}

		#endregion

		#endregion

		public void Close() => FileSystem.Dispose();
		public override string ToString() => FileSystem?.ToString() ?? "null";
	}

	internal sealed class FileSystemPendingViewModel : FileSystemViewModel {
		public FileSystemPendingViewModel() : base(FileSystem.Dummy) { }
	}

	internal sealed class DesignModeFileSystemViewModel : FileSystemViewModel {
		public DesignModeFileSystemViewModel() : base(FileSystem.Dummy) { }
	}
}

