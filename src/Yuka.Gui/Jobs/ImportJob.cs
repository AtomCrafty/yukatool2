﻿using System;
using System.IO;
using Yuka.Gui.Configuration;
using Yuka.Gui.Properties;
using Yuka.Gui.ViewModels;
using Yuka.IO;

namespace Yuka.Gui.Jobs {
	public class ImportJob : Job {

		public FileSystemViewModel ViewModel;
		public FileSystem SourceFileSystem, DestinationFileSystem;
		public string LocalBasePath;
		public string[] Files;
		public bool AutoConvert = Config.Current.ConvertOnImport;
		public bool CloseSourceFileSystem;

		public override void Execute() {
			if(AutoConvert) {
				// TODO convert-copy
				throw new NotImplementedException("ImportJob convert-copy mode not implemented");
			}
			else {
				for(int i = 0; i < Files.Length; i++) {
					string file = Files[i];
					string localFilePath = Path.Combine(LocalBasePath, file);

					// TODO status bar
					Description = string.Format(Resources.IO_ImportProgressUpdate, i + 1, Files.Length, localFilePath);
					Log.Note(Description, Resources.Tag_IO);
					Progress = (double)(i + 1) / Files.Length;

					using(var srcStream = SourceFileSystem.OpenFile(file))
					using(var dstStream = DestinationFileSystem.CreateFile(localFilePath)) {
						srcStream.CopyTo(dstStream);
					}

					ViewModel.AddFile(localFilePath);
				}

				// TODO status bar
				Description = string.Format(Resources.IO_ImportFinished, Files.Length);
				Log.Note(Description, Resources.Tag_IO);
				Progress = 1;
			}

			if(CloseSourceFileSystem) SourceFileSystem.Dispose();
		}
	}
}