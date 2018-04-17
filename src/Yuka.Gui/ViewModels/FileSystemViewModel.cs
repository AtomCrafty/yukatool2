using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yuka.Gui.Jobs;
using Yuka.Gui.Properties;
using Yuka.Gui.Services;
using Yuka.Gui.Services.Abstract;
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

			foreach(string file in FileSystem.GetFiles()) AddFile(file);
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

		public void AddFile(string path) => CreateNode(path, ShellItemType.File, Nodes);
		public void AddDirectory(string path) => CreateNode(path, ShellItemType.Directory, Nodes);

		#endregion

		#region IO Methods

		#region Import

		[Obsolete]
		public void CopyFile(string localFilePath, Stream srcStream, bool rawCopy) {
			if(rawCopy) {
				using(var dstStream = FileSystem.CreateFile(localFilePath)) {
					srcStream.CopyTo(dstStream);
					dstStream.Flush();
					CreateNode(localFilePath, ShellItemType.File, Nodes);
				}
			}
			else {
				// TODO convert-copy
				Log.Warn("Convert-copy not implemented", Resources.Tag_IO);
			}
		}

		[Obsolete]
		public void ImportFolder(string folderPath, string localBasePath, bool rawCopy) {
			try {
				string localPath = Path.Combine(localBasePath, Path.GetFileName(folderPath) ?? "");
				int fileCount = 0;

				using(var srcFs = FileSystem.FromFolder(folderPath)) {
					foreach(string file in srcFs.GetFiles()) {
						string localFilePath = Path.Combine(localPath, file);
						using(var srcStream = srcFs.OpenFile(file)) {
							CopyFile(localFilePath, srcStream, rawCopy);
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

		[Obsolete]
		public void ImportFile(string filePath, string localBasePath, bool rawCopy) {
			try {
				string localFilePath = Path.Combine(localBasePath, Path.GetFileName(filePath) ?? "");
				using(var srcStream = File.Open(filePath, FileMode.Open)) {
					CopyFile(localFilePath, srcStream, rawCopy);
				}

				Log.Note(string.Format(Resources.IO_ImportFileFinished, filePath, localFilePath), Resources.Tag_IO);
			}
			catch(Exception e) {
				Log.Fail(string.Format(Resources.IO_ImportFileFailed, e.GetType().Name, e.Message), Resources.Tag_IO);
				Log.Fail(e.StackTrace, Resources.Tag_IO);
			}
		}

		[Obsolete]
		public void ImportFileOrFolder(string path, string localBasePath, bool rawCopy) {
			if(Directory.Exists(path)) ImportFolder(path, localBasePath, rawCopy);
			else if(File.Exists(path)) ImportFile(path, localBasePath, rawCopy);
			else Log.Warn(string.Format(Resources.IO_ImportFileNotFound, path), Resources.Tag_IO);
		}

		public void ImportFiles(FileSystem sourceFs, string[] files, string localBasePath, bool convert) {
			Service.Get<IJobService>().QueueJob(new ImportJob {
				ViewModel = this,
				SourceFileSystem = sourceFs,
				DestinationFileSystem = FileSystem,
				LocalBasePath = localBasePath,
				Files = files,
				AutoConvert = convert
			});
		}

		public void ImportPaths(string[] paths, string localBasePath, bool convert) {
			var absolutePaths = new List<string>();
			string basePath = null;

			// gather files
			foreach(string path in paths) {
				string parentDirectory = Path.GetDirectoryName(path) ?? "";

				if(basePath == null) {
					basePath = parentDirectory;
				}
				else if(parentDirectory != basePath) {
					Log.Warn(Resources.IO_ImportBaseDirectoryMismatch, Resources.Tag_IO);
					continue;
				}

				if(File.Exists(path)) {
					absolutePaths.Add(path);
				}
				else if(Directory.Exists(path)) {
					absolutePaths.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
				}
				else {
					Log.Warn(string.Format(Resources.IO_ImportFileNotFound, path), Resources.Tag_IO);
				}
			}

			// convert to relative paths
			int basePathLength = FileSystem.NormalizePath(basePath).Length + 1; // +1 for the slash
			var relativePaths = absolutePaths.Select(f => f.Substring(basePathLength)).ToArray();

			// import files
			ImportFiles(FileSystem.FromFolder(basePath), relativePaths, localBasePath, convert);
		}

		#endregion

		#region Delete

		public void DeleteFileOrFolder(string path) {
			if(Nodes.TryGetValue(path, out var node) && node != null) DeleteNode(node);
		}

		public void DeleteNode(ShellItemViewModel item) {
			switch(item.Type) {
				case ShellItemType.Directory:
					Log.Info(string.Format(Resources.IO_DeletingDirectoryFromArchive, item.FullPath), Resources.Tag_IO);
					// create shallow copy to iterate over, because the collection is modified in the loop
					foreach(var child in item.Children.ToList()) {
						DeleteNode(child);
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
		public FileSystemPendingViewModel() : base(FileSystem.Dummy) {
		}
	}

	internal sealed class DesignModeFileSystemViewModel : FileSystemViewModel {
		public DesignModeFileSystemViewModel() : base(FileSystem.Dummy) {
		}
	}
}