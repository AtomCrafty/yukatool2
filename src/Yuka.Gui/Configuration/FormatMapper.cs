using System;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using PropertyChanged;
using Yuka.IO;

namespace Yuka.Gui.Configuration {
	public sealed class FormatMapper : ObservableCollection<FormatMapping> {

		public void SetMappedFormat(Format from, Format to) {
			var mapping = this.First(m => m.From == from);
			if(mapping == null) Add(mapping = new FormatMapping(from));
			mapping.To = to;
		}

		public Format GetMappedFormat(Format from) {
			var mapping = this.First(m => m.From == from);
			if(mapping == null) Add(mapping = new FormatMapping(from));
			return mapping.To;
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