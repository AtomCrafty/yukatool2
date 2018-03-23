using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Yuka.IO;

namespace Yuka.Gui.ViewModels {
	public class FileSystemViewModel : ViewModel {

		protected readonly FileSystem LoadedFileSystem;
		public FileSystemEntryViewModel Root { get; protected set; }

		public FileSystemViewModel(FileSystem loadedFileSystem) {
			LoadedFileSystem = loadedFileSystem ?? throw new ArgumentNullException(nameof(loadedFileSystem));

			Root = new FileSystemEntryViewModel(LoadedFileSystem, loadedFileSystem.Name, FileSystemEntryType.Root);

			var nodes = new Dictionary<string, FileSystemEntryViewModel>();
			foreach(string file in LoadedFileSystem.GetFiles()) {
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
			var node = new FileSystemEntryViewModel(LoadedFileSystem, path, type);

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
			LoadedFileSystem.Dispose();
		}
	}
}
