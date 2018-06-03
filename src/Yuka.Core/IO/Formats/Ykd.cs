using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yuka.Script;
using Yuka.Script.Data;
using Yuka.Script.Source;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class YkdFormat : Format {
		public override string Id => "ykd";
		public override string Extension => ".ykd";
		public override string Description => "Decompiled Yuka script";
		public override FormatType Type => FormatType.Unpacked;

		public override IEnumerable<string> GetSecondaryFiles(FileSystem fs, string fileName) {
			string ykiFileName = fileName.WithExtension(Yki.Extension);
			string csvFileName = fileName.WithExtension(Csv.Extension);

			// if both a ykd and yki file exist, the ykd should take precedence

			var list = new List<string>();
			if(fs.FileExists(csvFileName)) list.Add(csvFileName);
			if(fs.FileExists(ykiFileName)) list.Add(ykiFileName);
			return list;
		}
	}

	public class YkdScriptReader : FileReader<YukaScript> {

		public override Format Format => Ykd;

		public override bool CanRead(string name, BinaryReader r) {
			return name.EndsWith(Ykd.Extension);
		}

		public override YukaScript Read(string name, Stream s) {
			throw new InvalidOperationException("Reading from stream is not supported by " + nameof(YkdScriptReader));
		}

		public override YukaScript Read(string baseName, FileSystem fs, FileList files) {
			StringTable stringTable = null;

			string csvFileName = baseName.WithExtension(Csv.Extension);
			if(fs.FileExists(csvFileName)) {
				stringTable = Decode<StringTable>(csvFileName, fs, files);
			}

			using(var stream = fs.OpenFile(baseName)) {
				files?.Add(baseName, Ykd);
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

		public override void Write(YukaScript script, string baseName, FileSystem fs, FileList files) {
			string scriptName = baseName.WithExtension(Ykd.Extension);
			using(var stream = fs.CreateFile(scriptName)) {
				files?.Add(scriptName, Ykd);
				var writer = new StreamWriter(stream);

				script.EnsureDecompiled();

				foreach(var statement in script.Body.Statements) {
					writer.WriteLine(statement.ToString());
				}

				writer.Flush();
			}

			if(script.Strings != null && script.Strings.Any()) {
				Encode(script.Strings, baseName, fs, new FormatPreference(Csv), files);
			}
		}
	}
}