using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Yuka.Gui.Util;
using Yuka.Gui.ViewModels;
using Yuka.Util;

namespace Yuka.Gui.Views.Files {
	/// <summary>
	/// Interaction logic for FileTree.xaml
	/// </summary>
	public partial class FileTree {
		public FileTree() {
			InitializeComponent();
		}

		private void TreeItem_OnDragEnter(object sender, DragEventArgs e) {
			if(!(sender is TreeViewItem elem) || !(elem.DataContext is ShellItemViewModel item)) return;
			if(!e.Data.GetFormats().Contains("FileDrop")) return;

			e.Effects = DragDropEffects.Copy;
		}

		private void TreeItem_OnDrop(object sender, DragEventArgs e) {
			if(!e.Data.GetFormats().Contains("FileDrop")) return;

			try {
				var paths = e.Data.GetData("FileDrop") as string[];
				if(paths.IsNullOrEmpty()) return;

				if(!((sender as FrameworkElement)?.FindAnchestor<TreeViewItem>()?.DataContext is ShellItemViewModel item)) return;
				item.ImportFiles(paths);
			}
			catch(Exception ex) {
				Gui.Log.Fail(string.Format(Properties.Resources.IO_DragNDropReceiveFailed, ex.GetType().Name, ex.Message), Properties.Resources.Tag_IO);
				Gui.Log.Fail(ex.StackTrace, Properties.Resources.Tag_IO);
			}
		}
	}
}