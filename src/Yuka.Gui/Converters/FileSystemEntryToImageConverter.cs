using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Yuka.Gui.ViewModels;

namespace Yuka.Gui.Converters {
	[ValueConversion(typeof(string), typeof(BitmapImage))]
	public class FileSystemEntryToImageConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return new BitmapImage(new Uri($@"D:\Projects\GitHub\yukatool2gui\src\Yuka.Gui\res\images\{value}.png"));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}

		public static string GetIconNameForFileSystemEntry(FileSystemEntryType type, string name, bool isExpanded) {
			switch(type) {
				case FileSystemEntryType.Directory:
					return isExpanded ? "folder-open" : "folder";
				case FileSystemEntryType.File:
					switch(Path.GetExtension(name)?.ToLower()) {

						case ".ykc":
							return "folder-archive";

						case ".ykg":
						case ".png":
						case ".gnp":
						case ".bmp":
							return "file-image";

						case ".mp3":
						case ".ogg":
							return "file-audio";

						case ".yks":
							return "file-script";

						case ".csv":
						case ".ini":
						case ".txt":
						case ".ani":
							return "file-text";

						case ".frm":
							return "file-binary";

						default:
							return "file";
					}
				case FileSystemEntryType.Root:
					return "folder-archive";
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
	}
}
