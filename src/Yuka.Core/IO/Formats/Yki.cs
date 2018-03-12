using System;
using System.IO;
using Yuka.Script;
using Yuka.Script.Binary;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class YkiFormat : Format {
		public override string Extension => ".yki";
		public override string Description => "Intermediate Yuka script instruction list";
		public override FormatType Type => FormatType.None;

		public override FileCategory GetFileCategory(FileSystem fs, string fileName) {
			// if both a ykd and yki file exist, the ykd should take precedence
			return fs.FileExists(fileName.WithExtension(Ykd.Extension)) ? FileCategory.Secondary : FileCategory.Primary;
		}
	}

	public class YkiScriptReader : FileReader<YukaScript> {
		public override Format Format => Yki;

		public override bool CanRead(string name, BinaryReader r) {
			return name.EndsWith(Yki.Extension, StringComparison.CurrentCultureIgnoreCase);
		}

		public override YukaScript Read(string name, Stream s) {
			return new YukaScript(name, new InstructionParser(s).Parse());
		}
	}

	public class YkiScriptWriter : FileWriter<YukaScript> {
		public override Format Format => Yki;

		public override bool CanWrite(object obj) {
			return obj is YukaScript;
		}

		public override void Write(YukaScript script, Stream s) {
			var writer = new StreamWriter(s);

			script.EnsureCompiled();

			foreach(var instruction in script.InstructionList) {
				writer.WriteLine(instruction.ToString());
			}

			writer.Flush();
		}
	}
}