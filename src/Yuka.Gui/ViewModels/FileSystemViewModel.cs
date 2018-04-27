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

		public void ImportFiles(FileSystem sourceFs, string[] files, string localBasePath, bool convert, bool closeSourceFs = true) {
			Service.Get<IJobService>().QueueJob(new ImportJob {
				ViewModel = this,
				SourceFileSystem = sourceFs,
				DestinationFileSystem = FileSystem,
				LocalBasePath = localBasePath,
				Files = files,
				AutoConvert = convert,
				CloseSourceFileSystem = closeSourceFs
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

		public void ExportFiles(string[] files, string localBasePath, bool convert, string targetFolder = null) {
			Service.Get<IJobService>().QueueJob(new ExportJob {
				SourceFileSystem = FileSystem,
				LocalBasePath = localBasePath,
				Files = files,
				TargetFolder = targetFolder,
				AutoConvert = convert
			});
		}

		public void ExportPaths(string[] paths, string localBasePath, bool convert, string targetFolder = null) {
			var files = new List<string>();

			// gather files
			foreach(string path in paths) {
				files.AddRange(FileSystem.GetFiles().Where(f => f.StartsWith(path)));
			}

			// export files
			ExportFiles(files.ToArray(), localBasePath, convert, targetFolder);
		}

		public void ExportFileOrFolder(ShellItemViewModel item, bool convert, string targetFolder = null) {
			ExportPaths(new[] { item.FullPath }, (item.DropTargetPath + '\\').TrimStart('\\'), convert, targetFolder);
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