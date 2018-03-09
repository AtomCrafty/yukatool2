using System;
using System.IO;
using System.Linq;
using Yuka.Script;
using Yuka.Script.Data;
using Yuka.Script.Source;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class YkdFormat : Format {
		public override string Extension => ".ykd";
		public override string Description => "Decompiled Yuka script";
		public override FormatType Type => FormatType.Unpacked;
	}

	public class YkdScriptReader : FileReader<YukaScript> {

		public override Format Format => Ykd;

		public override bool CanRead(string name, BinaryReader r) {
			return name.EndsWith(Ykd.Extension);
		}

		public override YukaScript Read(string name, Stream s) {
			throw new InvalidOperationException("Reading from stream is not supported by " + nameof(YkdScriptReader));
		}

		public override YukaScript Read(string baseName, FileSystem fs) {
			StringTable stringTable = null;

			string csvFileName = baseName.WithExtension(Csv.Extension);
			if(fs.FileExists(csvFileName)) {
				stringTable = Decode<StringTable>(csvFileName, fs);
			}

			using(var stream = fs.OpenFile(baseName)) {
				var lexer = new Lexer(new StreamReader(stream), baseName);
				return new Parser(baseName, stringTable, lexer).ParseScript();
			}
		}
	}

	public class YkdScriptWriter : FileWriter<YukaScript> {

		public override Format Format => Ykd;

		public override bool CanWrite(object obj) {
			return obj is YukaScript;
		}

		public override void Write(YukaScript script, Stream s) {
			throw new InvalidOperationException("Writing to stream is not supported by " + nameof(YkdScriptWriter));
		}

		public override void Write(YukaScript script, string baseName, FileSystem fs) {
			using(var stream = fs.CreateFile(baseName.WithExtension(Ykd.Extension))) {
				var writer = new StreamWriter(stream);

				script.EnsureDecompiled();

				foreach(var statement in script.Body.Statements) {
					writer.WriteLine(statement.ToString());
				}

				writer.Flush();
			}

			if(script.Strings != null && script.Strings.Any()) {
				Encode(script.Strings, baseName, fs, new FormatPreference(Csv));
			}
		}
	}
}