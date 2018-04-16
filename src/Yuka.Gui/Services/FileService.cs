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

		public string SelectFile(string initialDirectory, string[] filters = null) {
			throw new NotImplementedException();
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
