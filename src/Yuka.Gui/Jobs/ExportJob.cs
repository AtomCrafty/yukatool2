using System;
using System.IO;
using Yuka.Gui.Configuration;
using Yuka.Gui.Properties;
using Yuka.Gui.Services;
using Yuka.Gui.Services.Abstract;
using Yuka.IO;

namespace Yuka.Gui.Jobs {
	public class ExportJob : Job {
		public FileSystem SourceFileSystem;
		public string TargetFolder;
		public string LocalBasePath;
		public string[] Files;
		public bool AutoConvert = Config.Current.ConvertOnExport;

		public override void Execute() {
			if(AutoConvert) {
				// TODO convert-copy
				throw new NotImplementedException("ExportJob convert-copy mode not implemented");
			}
			else {
				if(TargetFolder == null) {
					TargetFolder = Service.Get<IFileService>().SelectDirectory(null, Resources.IO_ExportSelectFolderDescription);
					if(TargetFolder == null) {
						Log.Note(Resources.IO_ExportTargetNull, Resources.Tag_IO);
						return;
					}
				}

				using(var dstFs = FileSystem.NewFolder(TargetFolder)) {

					Description = Resources.IO_ExportWaitingForConfirmation;
					if(dstFs.GetFiles().Length > 0 && !Service.Get<ConfirmationService>().ConfirmAndRemember("ExportIntoNonEmptyFolder")) return;

					for(int i = 0; i < Files.Length; i++) {
						string file = Files[i];
						string relativePath = file.Substring(LocalBasePath.Length);

						// TODO status bar
						Description = string.Format(Resources.IO_ExportProgressUpdate, i + 1, Files.Length, file);
						Log.Note(Description, Resources.Tag_IO);
						Progress = (double)(i + 1) / Files.Length;

						using(var srcFile = SourceFileSystem.OpenFile(file))
						using(var dstFile = dstFs.CreateFile(relativePath)) {
							srcFile.CopyTo(dstFile);
						}
					}

					// TODO status bar
					Description = string.Format(Resources.IO_ExportFinished, Files.Length);
					Log.Note(Description, Resources.Tag_IO);
					Progress = 1;
				}
			}
		}
	}
}
