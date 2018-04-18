using System.Collections.ObjectModel;
using System.Linq;
using PropertyChanged;
using Yuka.IO;

namespace Yuka.Gui.Configuration {
	public sealed class FormatMapper : ObservableCollection<FormatMapping> {
		public FormatMapper() {
			foreach(var format in Format.RegisteredFormats) {
				Add(new FormatMapping(format, format));
			}
		}

		public void SetMappedFormat(Format from, Format to) {
			this.First(m => m.From == from).To = to;
		}

		public Format GetMappedFormat(Format from) {
			return this.First(m => m.From == from).To;
		}
	}

	[AddINotifyPropertyChangedInterface]
	public sealed class FormatMapping {
		public FormatMapping(Format from, Format to = null, string path = null) {
			From = from;
			To = to ?? from;
			Path = path;
		}

		public string Path { get; set; }
		public Format From { get; set; }
		public Format To { get; set; }

		public override string ToString() => (Path != null ? $"({Path}) " : "") + $"{From.Name} => {To.Name}";
	}
}