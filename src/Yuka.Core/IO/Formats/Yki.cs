using System.IO;
using Yuka.Script;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class YkiFormat : Format {
		public override string Extension => ".yki";
		public override string Description => "Intermediate Yuka script instruction list";
		public override FormatType Type => FormatType.Unpacked;
	}
	
	public class YkiScriptWriter : FileWriter<YukaScript> {
		public override Format Format => Yki;

		public override bool CanWrite(object obj) {
			return obj is YukaScript;
		}

		public override void Write(YukaScript script, Stream s) {
			var w = new StreamWriter(s);

			script.EnsureCompiled();

			foreach(var instruction in script.InstructionList) {
				w.WriteLine(instruction.ToString());
			}
		}
	}
}