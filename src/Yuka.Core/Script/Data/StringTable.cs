using System;
using System.Collections.Generic;
using System.Linq;
using Yuka.IO;

namespace Yuka.Script.Data {
	public class StringTable : Dictionary<string, StringTableEntry> {
		public List<string> Stages;

		public IEnumerable<StringTableEntry> Names => Values.Where(e => e.Category == StringCategory.N);
		public IEnumerable<StringTableEntry> NonNames => Values.Where(e => e.Category != StringCategory.N);
	}

	public class StringTableEntry {
		public StringCategory Category;
		public string Key;
		public string Speaker;
		public string Comment;
		public string Fallback;
		public string[] Text;

		public static StringCategory? GetCategoryForKey(string key) {
			if(key.StartsWith("L")) return StringCategory.L;
			if(key.StartsWith("N")) return StringCategory.N;
			if(key.StartsWith("S")) return StringCategory.S;
			return null;
		}

		internal StringTableEntry(StringCategory category, string key) {
			Category = category;
			Key = key;
		}

		public StringTableEntry(StringCategory category, string key, string fallback, string speaker = null) {
			Category = category;
			Key = key;
			Fallback = fallback;
			Speaker = speaker;
			Text = Array.Empty<string>();
		}

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
