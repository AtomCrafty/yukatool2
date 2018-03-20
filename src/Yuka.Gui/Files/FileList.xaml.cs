using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Yuka.IO;
using Path = System.IO.Path;

namespace Yuka.Gui.Files {
	/// <summary>
	/// Interaction logic for FileList.xaml
	/// </summary>
	public partial class FileList {

		public FileSystem LoadedFileSystem { get; protected set; }
		protected readonly Dictionary<string, ListItem> Nodes = new Dictionary<string, ListItem>(StringComparer.InvariantCultureIgnoreCase);

		public FileList() {
			InitializeComponent();
			RefreshFileView();
		}

		public void LoadArchive(string path) {
			CloseFileSystem(false);
			LoadedFileSystem = FileSystem.FromArchive(path);
			RefreshFileView();
		}

		public void CloseFileSystem(bool refreshView = true) {
			LoadedFileSystem?.Dispose();
			LoadedFileSystem = null;

			if(refreshView) RefreshFileView();
		}

		public void RefreshFileView() {
			Nodes.Clear();
			Items.Clear();

			if(LoadedFileSystem == null) return;

			foreach(string filePath in LoadedFileSystem.GetFiles()) {
				AddNode(filePath);
			}
		}

		public ListItem AddNode(string path) {
			if(string.IsNullOrWhiteSpace(path)) return null;
			if(Nodes.ContainsKey(path)) return Nodes[path];

			var fileNode = new FileListItem(path, LoadedFileSystem);

			Items.Add(fileNode);
			Nodes[path] = fileNode;

			return fileNode;
		}
	}

	public class FileListItem : ListItem {

		public readonly FileSystem ContainingFileSystem;
		public readonly string FullPath;

		public FileListItem(string path, FileSystem containingFileSystem) {
			ContainingFileSystem = containingFileSystem;
			FullPath = path;
		}
	}
}
