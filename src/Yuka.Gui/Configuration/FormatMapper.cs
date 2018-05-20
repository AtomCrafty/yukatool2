using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using Yuka.IO;

namespace Yuka.Gui.Configuration {
	[Serializable]
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

	[Serializable]
	[AddINotifyPropertyChangedInterface]
	public sealed class FormatMapping {
		public FormatMapping(Format from, Format to = null, string path = null) {
			From = from;
			To = to ?? from;
			Path = path;
		}

		public string Path { get; set; }
		[JsonConverter(typeof(FormatConverter))] public Format From { get; set; }
		[JsonConverter(typeof(FormatConverter))] public Format To { get; set; }

		public override string ToString() => (Path != null ? $"({Path}) " : "") + $"{From.Name} => {To.Name}";
	}

	internal sealed class FormatConverter : JsonConverter<Format> {

		public override void WriteJson(JsonWriter writer, Format value, JsonSerializer serializer) {
			var token = JToken.FromObject(value.Id);
			Debug.Assert(token.Type == JTokenType.String);
			token.WriteTo(writer);
		}

		public override Format ReadJson(JsonReader reader, Type objectType, Format existingValue, bool hasExistingValue, JsonSerializer serializer) {
			return Format.ById(reader.Value as string);
		}
	}
}