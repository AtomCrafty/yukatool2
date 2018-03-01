using System.IO;
using Yuka.Script;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class YkdFormat : Format {
		public override string Extension => ".ykd";
		public override string Description => "Decompiled Yuka script";
		public override FormatType Type => FormatType.Unpacked;
	}

	public class YkdScriptWriter : FileWriter<YukaScript> {

		public override Format Format => Ykd;

		public override bool CanWrite(object obj) {
			return obj is YukaScript;
		}

		public override void Write(YukaScript script, Stream s) {
			var writer = new StreamWriter(s);

			foreach(var statement in script.Body.Statements) {
				writer.WriteLine(statement.ToString());
			}
		}
	}
}