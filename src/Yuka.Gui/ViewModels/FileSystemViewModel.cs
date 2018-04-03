using System;
using System.Collections.Generic;
using System.IO;
using Yuka.IO;

namespace Yuka.Gui.ViewModels {
	public class FileSystemViewModel : ViewModel {

		public static readonly FileSystemViewModel Pending = new FileSystemPendingViewModel();

		internal readonly FileSystem FileSystem;
		public FileSystemEntryViewModel Root { get; protected set; }

		public FileSystemViewModel(FileSystem fileSystem) {
			FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

			Root = new FileSystemEntryViewModel(FileSystem, fileSystem.Name, FileSystemEntryType.Root);

			var nodes = new Dictionary<string, FileSystemEntryViewModel>();
			foreach(string file in FileSystem.GetFiles()) {
				CreateNode(file, FileSystemEntryType.File, nodes);
			}
		}

		/// <summary>
		/// Adds a file/directory node and all its parents to the directory tree if it doesn't exist yet.
		/// </summary>
		/// <param name="path">Full path of the added element within the file system</param>
		/// <param name="type">Type of the element to be added (file/directory)</param>
		/// <param name="nodes">A directory containing all previously created nodes</param>
		/// <returns>The node corresponding to the specified path</returns>
		protected FileSystemEntryViewModel CreateNode(string path, FileSystemEntryType type, Dictionary<string, FileSystemEntryViewModel> nodes) {
			if(nodes.ContainsKey(path)) return nodes[path];

			string parentPath = Path.GetDirectoryName(path);
			var node = new FileSystemEntryViewModel(FileSystem, path, type);

			if(!string.IsNullOrWhiteSpace(parentPath)) {
				var parent = CreateNode(parentPath, FileSystemEntryType.Directory, nodes);
				parent.Children.Add(node);
			}
			else {
				Root.Children.Add(node);
			}

			nodes[path] = node;
			return node;
		}

		public void Close() {
			FileSystem.Dispose();
		}

		public override string ToString() => FileSystem?.ToString() ?? "null";
	}

	internal sealed class FileSystemPendingViewModel : FileSystemViewModel {
		public FileSystemPendingViewModel() : base(FileSystem.Dummy) { }
	}
}
