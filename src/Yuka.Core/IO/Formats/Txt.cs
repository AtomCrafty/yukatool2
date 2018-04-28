using System.IO;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {
	public class TxtFormat : Format {
		public override string Extension => null;
		public override string Description => "Text data";
		public override FormatType Type => FormatType.None;

		public string[] TextExtensions = { ".txt", ".log", ".ini", ".htm", ".html", ".xml", ".json" };
	}

	public class TxtStringReader : FileReader<string> {

		public override Format Format => Txt;

		public override bool CanRead(string name, BinaryReader r) {
			return Path.GetExtension(name)?.ToLower().IsOneOf(Txt.TextExtensions) ?? false;
		}

		public override string Read(string name, Stream s) {
			using(var reader = new StreamReader(s, Options.TextEncoding)) {
				return reader.ReadToEnd();
			}
		}
	}

	public class TxtStringWriter : FileWriter<string> {

		public override Format Format => Txt;

		public override bool CanWrite(object obj) {
			return obj is string;
		}

		public override void Write(string str, Stream s) {
			using(var writer = new StreamWriter(s, Options.TextEncoding)) {
				writer.Write(str);
			}
		}
	}
}
