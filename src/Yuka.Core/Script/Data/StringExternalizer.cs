using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		public static readonly Regex InternalStringRegex = new Regex(@".*\\.*|.*\.(?:png|bmp|ogg|yk.)$|^[\s\d]*$|^[A-Z0-9]{2}$", RegexOptions.IgnoreCase);

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

			StringTable[key] = new StringTableEntry(category, key, literal.Value, includeSpeaker ? _currentSpeaker : null);

			literal.StringTable = StringTable;
			literal.ExternalKey = key;
		}

		public bool ExternalizeInterpolatedString(OperatorExpr concatenation, StringCategory category, bool includeSpeaker, out StringLiteral placeholder) {
			if(concatenation.Operators.Any(op => op != "+")) {
				placeholder = null;
				return false;
			}

			var sb = new StringBuilder();

			foreach(var op in concatenation.Operands) {
				switch(op) {
					case StringLiteral literal:
						sb.Append(literal.Value);
						break;

					case Variable variable:
						sb.AppendFormat("{{{0}:{1}}}", variable.VariableType, variable.VariableId);
						break;

					default:
						placeholder = null;
						return false;
				}
			}

			string interpolated = sb.ToString();
			string key = GetUniqueId(category, interpolated);

			StringTable[key] = new StringTableEntry(category, key, interpolated, includeSpeaker ? _currentSpeaker : null);

			placeholder = new StringLiteral { ExternalKey = key, StringTable = StringTable };
			return true;
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
				for(int i = 0; i < stmt.Arguments.Length; i++) {
					var argument = stmt.Arguments[i];
					if(argument is StringLiteral literal) {
						ExternalizeStringLiteral(literal, StringCategory.L, true);
					}
					else if(argument is OperatorExpr concatenation) {
						if(ExternalizeInterpolatedString(concatenation, StringCategory.S, true, out var placeholder)) {
							stmt.Arguments[i] = placeholder;
						}
						else {
							argument.Accept(this);
						}
					}
					else {
						argument.Accept(this);
					}
				}
			}
			else if(stmt.MethodName.IsOneOf(Options.YkdNameMethods)) {
				// externalize name
				for(int i = 0; i < stmt.Arguments.Length; i++) {
					var argument = stmt.Arguments[i];
					if(argument is StringLiteral literal) {
						// set current speaker
						_currentSpeaker = literal.Value;

						ExternalizeStringLiteral(literal, StringCategory.N);
					}
					else if(argument is Variable) {
						// variable name probably refers to the protagonist
						_currentSpeaker = "me";
					}
					else if(argument is OperatorExpr concatenation) {
						if(ExternalizeInterpolatedString(concatenation, StringCategory.N, true, out var placeholder)) {
							stmt.Arguments[i] = placeholder;
						}
						else {
							argument.Accept(this);
						}
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
