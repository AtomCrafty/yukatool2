using System.Collections.ObjectModel;
using System.IO;
using Yuka.Gui.Converters;
using Yuka.IO;

namespace Yuka.Gui.ViewModels {
	public class FileSystemEntryViewModel : ViewModel {

		protected readonly FileSystem FileSystem;

		public FileSystemEntryType Type { get; set; }

		public string FullPath { get; set; }
		public string Name { get; set; }
		public long Size { get; set; }
		public ObservableCollection<FileSystemEntryViewModel> Children { get; } = new ObservableCollection<FileSystemEntryViewModel>();

		public bool IsExpanded { get; set; } = true;
		public string Icon => FileSystemEntryToImageConverter.GetIconNameForFileSystemEntry(Type, Name, IsExpanded);
		public Format Format => Format.GuessFromFileName(Name);

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