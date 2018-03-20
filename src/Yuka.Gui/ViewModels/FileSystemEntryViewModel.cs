using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Yuka.Graphics;
using Yuka.Gui.Converters;
using Yuka.IO;
using Yuka.Script;

namespace Yuka.Gui.ViewModels {
	public class FileSystemEntryViewModel : ViewModel {

		protected readonly FileSystem FileSystem;

		public FileSystemEntryType Type { get; set; }

		public string FullPath { get; set; }
		public string Name { get; set; }
		public long Size { get; set; }
		public ObservableCollection<FileSystemEntryViewModel> Children { get; } = new ObservableCollection<FileSystemEntryViewModel>();

		public bool IsExpanded { get; set; } = true;

		private bool _isSelected;
		public bool IsSelected {
			get => _isSelected;
			set {
				_isSelected = value;
				(_fileContent as IDisposable)?.Dispose();
				_fileContent = null;
			}
		}

		public string Icon => FileSystemEntryToImageConverter.GetIconNameForFileSystemEntry(Type, Name, IsExpanded);
		public Format Format => Type == FileSystemEntryType.Directory ? null : Format.GuessFromFileName(Name);

		protected object _fileContent;
		public ViewModel FileContent {
			get {
				if(!FileSystem.FileExists(FullPath)) return null;
				_fileContent = _fileContent ?? FileReader.DecodeObject(FullPath, FileSystem).Item1;
				switch(_fileContent) {
					case YukaScript script:
						return new YukaScriptViewModel(script);
					case YukaGraphic graphic:
						return new YukaGraphicViewModel(graphic);
				}
				return null;
			}
		}

		public FileSystemEntryViewModel(FileSystem fs, string fullPath, FileSystemEntryType type) {
			Type = type;
			FileSystem = fs;
			FullPath = fullPath;
			Name = Path.GetFileName(fullPath);
			Size = FileSystem.GetFileSize(fullPath);
		}
	}

	public enum FileSystemEntryType {
		Directory, File, Root
	}
}