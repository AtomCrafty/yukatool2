using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Yuka.Gui.Properties;
using Yuka.Gui.Services.Abstract;
using Yuka.IO;

namespace Yuka.Gui.Services {
	public class FileService : IFileService {
		protected readonly Window ParentWindow;

		public FileService(Window parentWindow) {
			ParentWindow = parentWindow;
		}

		public string SelectDirectory(string initialDirectory, [Localizable(true)] string description = null) {
			var dialog = new VistaFolderBrowserDialog {
				SelectedPath = initialDirectory,
				Description = description
			};

			return dialog.ShowDialog(ParentWindow) != true ? null : dialog.SelectedPath;
		}

		public string OpenFile(string filter, string ext, string title, string initialDirectory) {
			var dialog = new OpenFileDialog {
				InitialDirectory = initialDirectory,
				Filter = filter,
				FilterIndex = 0,
				DefaultExt = ext,
				Title = title,
				DereferenceLinks = true,
				CheckFileExists = true
			};

			Log.Note(Resources.IO_FileSelectionStarted, Resources.Tag_IO);
			if(dialog.ShowDialog(ParentWindow) == true) {
				string path = dialog.FileName;
				Log.Note(string.Format(Resources.IO_FileSelectionEnded, path), Resources.Tag_IO);
				return path;
			}

			Log.Note(Resources.IO_FolderSelectionAbortedByUser, Resources.Tag_IO);
			return null;
		}

		public string SaveFile(string filter, string ext, string title, string initialDirectory) {
			var dialog = new SaveFileDialog {
				InitialDirectory = initialDirectory,
				Filter = filter,
				FilterIndex = 0,
				DefaultExt = ext,
				Title = title,
				DereferenceLinks = true
			};

			Log.Note(Resources.IO_FileSelectionStarted, Resources.Tag_IO);
			if(dialog.ShowDialog(ParentWindow) == true) {
				string path = dialog.FileName;
				Log.Note(string.Format(Resources.IO_FileSelectionEnded, path), Resources.Tag_IO);
				return path;
			}

			Log.Note(Resources.IO_FolderSelectionAbortedByUser, Resources.Tag_IO);
			return null;
		}

		public string SelectArchiveFile(string initialDirectory) {
			var dialog = new OpenFileDialog {
				CheckFileExists = true,
				DefaultExt = Format.Ykc.Extension,
				InitialDirectory = initialDirectory,
				Title = Resources.UI_OpenArchiveDialogTitle,
				DereferenceLinks = true,
				Filter = Resources.UI_ArchiveFilter,
				FilterIndex = 0
			};

			return dialog.ShowDialog(ParentWindow) != true ? null : dialog.FileName;
		}
	}
}
