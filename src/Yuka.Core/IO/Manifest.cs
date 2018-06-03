using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yuka.IO {

	[JsonConverter(typeof(ManifestConverter))]
	public class Manifest : List<(FileList SourceFiles, FileList TargetFiles)> {
		public void Add(FileList sourceFiles, FileList targetFiles) => Add((sourceFiles, targetFiles));
	}

	[JsonConverter(typeof(FileListConverter))]
	public class FileList : List<(string Name, Format Format)> {
		public void Add(string name, Format format) => Add((name, format));
		public override string ToString() => $"[{string.Join(", ", this.Select(pair => $"{pair.Format.Name} {pair.Name}"))}]";
	}

	public class ManifestConverter : JsonConverter<Manifest> {

		public override void WriteJson(JsonWriter writer, Manifest manifest, JsonSerializer serializer) {
			var arr = new JArray();
			foreach(var (source, target) in manifest) {
				arr.Add(new JObject {
					{ "source", JToken.FromObject(source, serializer) },
					{ "target", JToken.FromObject(target, serializer) }
				});
			}
			arr.WriteTo(writer);
		}

		public override Manifest ReadJson(JsonReader reader, Type objectType, Manifest existingValue, bool hasExistingValue, JsonSerializer serializer) {
			var manifest = new Manifest();

			var arr = JToken.ReadFrom(reader) as JArray;
			Debug.Assert(arr != null);

			foreach(var token in arr) {
				var obj = token as JObject;

				Debug.Assert(obj != null);

				var s = obj.GetValue("source");
				var t = obj.GetValue("target");

				Debug.Assert(s != null);
				Debug.Assert(t != null);

				var source = s.ToObject<FileList>();
				var target = t.ToObject<FileList>();

				manifest.Add(source, target);
			}

			return manifest;
		}
	}

	public class FileListConverter : JsonConverter<FileList> {

		public override void WriteJson(JsonWriter writer, FileList manifest, JsonSerializer serializer) {
			var arr = new JArray();
			foreach(var (name, format) in manifest) {
				arr.Add(new JObject {
					{ "name", JToken.FromObject(name, serializer) },
					{ "format", JToken.FromObject(format, serializer) }
				});
			}
			arr.WriteTo(writer);
		}

		public override FileList ReadJson(JsonReader reader, Type objectType, FileList existingValue, bool hasExistingValue, JsonSerializer serializer) {
			var list = new FileList();

			var arr = JToken.ReadFrom(reader) as JArray;
			Debug.Assert(arr != null);

			foreach(var token in arr) {
				var obj = token as JObject;

				Debug.Assert(obj != null);

				var n = obj.GetValue("name");
				var f = obj.GetValue("format");

				Debug.Assert(n != null);
				Debug.Assert(f != null);

				string name = n.ToObject<string>(serializer);
				var format = f.ToObject<Format>(serializer);

				list.Add(name, format);
			}

			return list;
		}
	}
}
