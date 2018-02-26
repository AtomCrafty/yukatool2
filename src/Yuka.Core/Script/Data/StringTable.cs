using System.Collections.Generic;
using System.Linq;
using Yuka.IO;

namespace Yuka.Script.Data {
	public class StringTable : Dictionary<string, StringTableEntry> {
		public List<string> Stages;

		public IEnumerable<StringTableEntry> Names => Values.Where(e => e.Key.StartsWith(Options.CsvNamePrefix));
		public IEnumerable<StringTableEntry> NonNames => Values.Where(e => !e.Key.StartsWith(Options.CsvNamePrefix));
	}

	public class StringTableEntry {
		public string Key;
		public string Speaker;
		public string Comment;
		public string Fallback;
		public string[] Text;

		public string CurrentTextVersion {
			get {
				for(int i = Text.Length - 1; i >= 0; i--) {
					string str = Text[i];
					if(str.Length > 0 && str != Options.CsvSkipTextField) {
						return str;
					}
				}
				return Fallback;
			}
		}

		public override string ToString() => CurrentTextVersion;
	}
}
