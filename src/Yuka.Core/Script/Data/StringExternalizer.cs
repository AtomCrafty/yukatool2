using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Yuka.IO;
using Yuka.Script.Syntax;
using Yuka.Script.Syntax.Expr;
using Yuka.Script.Syntax.Stmt;
using Yuka.Util;

namespace Yuka.Script.Data {
	public class StringExternalizer : NodeVisitor {

		public StringTable StringTable;
		protected readonly Dictionary<StringCategory, int> IdCounter = new Dictionary<StringCategory, int>();
		protected string _currentSpeaker;

		public StringExternalizer(StringTable stringTable) {
			StringTable = stringTable;
		}

		// TODO make this configurable
		public static readonly Regex InternalStringRegex = new Regex(@".*\\.*|.*\.(?:png|bmp|ogg|yk.)$|^\s*$");

		public static bool IsExternalizableString(string value) {
			return !InternalStringRegex.IsMatch(value);
		}

		public string GetUniqueId(StringCategory category, string value) {
			if(category == StringCategory.N) {
				string key = StringTable.Names.FirstOrDefault(entry => entry.Fallback == value)?.Key;
				if(key != null) return key;
			}

			if(!IdCounter.ContainsKey(category)) IdCounter[category] = 1;
			return category.ToString() + IdCounter[category]++;
		}

		public void ExternalizeStringLiteral(StringLiteral literal, StringCategory category, bool includeSpeaker = false) {
			string key = GetUniqueId(category, literal.Value);

			StringTable[key] = new StringTableEntry(key, literal.Value, includeSpeaker ? _currentSpeaker : null);

			literal.StringTable = StringTable;
			literal.ExternalKey = key;
		}

		public override object Visit(StringLiteral literal) {
			if(IsExternalizableString(literal.Value)) {
				ExternalizeStringLiteral(literal, StringCategory.S);
			}
			return base.Visit(literal);
		}

		public override void Visit(FunctionCallStmt stmt) {
			if(stmt.MethodName.IsOneOf(Options.YkdLineMethods)) {

				// externalize line of text
				foreach(var argument in stmt.Arguments) {
					if(argument is StringLiteral literal) {
						ExternalizeStringLiteral(literal, StringCategory.L, true);
					}
					else {
						argument.Accept(this);
					}
				}
			}
			else if(stmt.MethodName.IsOneOf(Options.YkdNameMethods)) {

				// externalize name
				foreach(var argument in stmt.Arguments) {
					if(argument is StringLiteral literal) {

						// set current speaker
						_currentSpeaker = literal.Value;

						ExternalizeStringLiteral(literal, StringCategory.N);
					}
					else if(argument is Variable) {

						// variable name probably refers to the protagonist
						_currentSpeaker = "me";
					}
					else {
						argument.Accept(this);
					}
				}
			}
			else {
				if(stmt.MethodName.IsOneOf(Options.YkdResetSpeakerMethods)) {
					_currentSpeaker = null;
				}
				base.Visit(stmt);
			}
		}
	}

	public enum StringCategory {
		L, N, S
	}
}
