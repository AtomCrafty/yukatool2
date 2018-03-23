using System;
using System.Security.RightsManagement;
using System.Windows;
using Microsoft.Win32;
using Yuka.Gui.Services.Abstract;
using Yuka.IO;

namespace Yuka.Gui.Services {
	public class FileService : IFileService {
		protected readonly Window ParentWindow;

		public FileService(Window parentWindow) {
			ParentWindow = parentWindow;
		}

		public string SelectDirectory(string initialDirectory) {
			throw new NotImplementedException();
		}

		public string SelectFile(string initialDirectory, string[] filters = null) {
			throw new NotImplementedException();
		}

		public string SelectArchiveFile(string initialDirectory) {
			var dialog = new OpenFileDialog {
				CheckFileExists = true,
				DefaultExt = Format.Ykc.Extension,
				InitialDirectory = initialDirectory,
				Title = "Open archive",
				DereferenceLinks = true,
				Filter = "Yuka container (*.ykc)|*.ykc|All files (*.*)|*.*",
				FilterIndex = 0
			};

			return dialog.ShowDialog(ParentWindow) != true ? null : dialog.FileName;
		}
	}
}
