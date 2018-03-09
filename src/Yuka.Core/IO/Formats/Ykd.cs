using System.IO;
using Yuka.Script;
using Yuka.Script.Data;
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
			var lexer = new Lexer(new StreamReader(s), name);
			// TODO read string table from csv file
			return new Parser(lexer, new StringTable()).ParseScript();
		}
	}

	public class YkdScriptWriter : FileWriter<YukaScript> {

		public override Format Format => Ykd;

		public override bool CanWrite(object obj) {
			return obj is YukaScript;
		}

		public override void Write(YukaScript script, Stream s) {
			var writer = new StreamWriter(s);

			script.EnsureDecompiled();

			foreach(var statement in script.Body.Statements) {
				writer.WriteLine(statement.ToString());
			}

			writer.Flush();
		}
	}
}