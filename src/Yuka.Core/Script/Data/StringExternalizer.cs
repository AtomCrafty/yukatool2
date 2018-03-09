using System.Collections.Generic;
using System.Text.RegularExpressions;
using Yuka.IO;
using Yuka.Script.Syntax;
using Yuka.Script.Syntax.Expr;
using Yuka.Script.Syntax.Stmt;
using Yuka.Util;

namespace Yuka.Script.Data {
	public class StringExternalizer : NodeVisitor {

		public StringTable StringTable;
		protected readonly Dictionary<string, int> IdCounter = new Dictionary<string, int>();
		protected string _currentSpeaker;

		// TODO make this configurable
		public static readonly Regex InternalStringRegex = new Regex(@".*\\.*|.*\.(?:png|bmp|ogg|yk.)$|^\s*$");

		public static bool IsExternalizableString(string value) {
			return !InternalStringRegex.IsMatch(value);
		}

		public string GetUniqueId(string category) {
			if(!IdCounter.ContainsKey(category)) IdCounter[category] = 1;
			return category + IdCounter[category]++;
		}

		public void ExternalizeStringLiteral(StringLiteral literal, string category, bool includeSpeaker = false) {
			string key = GetUniqueId(category);

			StringTable[key] = new StringTableEntry(key, literal.Value, includeSpeaker ? _currentSpeaker : null);

			literal.StringTable = StringTable;
			literal.ExternalKey = key;
		}

		public override object Visit(StringLiteral literal) {
			if(IsExternalizableString(literal.Value)) {
				ExternalizeStringLiteral(literal, "S");
			}
			return base.Visit(literal);
		}

		public override void Visit(FunctionCallStmt stmt) {
			if(stmt.MethodName.IsOneOf(Options.YkdLineMethods)) {

				// externalize line of text
				foreach(var argument in stmt.Arguments) {
					if(argument is StringLiteral literal) {
						ExternalizeStringLiteral(literal, "L", true);
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

						ExternalizeStringLiteral(literal, "N");
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
}
