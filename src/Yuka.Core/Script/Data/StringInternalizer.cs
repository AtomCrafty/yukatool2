using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Yuka.IO;
using Yuka.Script.Syntax;
using Yuka.Script.Syntax.Expr;
using Yuka.Script.Syntax.Stmt;
using Yuka.Util;

namespace Yuka.Script.Data {
	public class StringInternalizer : NodeVisitor {

		public YukaScript Script;

		public StringInternalizer(YukaScript script) {
			Script = script;
		}

		public void InternalizeStringLiteral(StringLiteral literal) {
			string key = literal.ExternalKey;

			literal.StringTable = null;
			literal.ExternalKey = null;

			if(Script.Strings.TryGetValue(key, out var entry)) {
				literal.Value = entry.CurrentTextVersion;
				if(entry.CurrentTextVersion.Trim() == ".")
					Console.WriteLine($"WARNING: single dot in {Path.GetFileNameWithoutExtension(Script.Name).ToLower()}, {key}");
				//// TODO remove this again
				literal.Value = key.StartsWith("L")
					? $"@l({Path.GetFileNameWithoutExtension(Script.Name).ToLower()}, {key}){entry.CurrentTextVersion}"
					: entry.CurrentTextVersion;
			}
			else {
				Console.WriteLine($"Warning: Missing translation for {key} in '{Script.Name}'");
			}
		}

		public override object Visit(StringLiteral literal) {
			if(literal.IsExternalized) {
				InternalizeStringLiteral(literal, false, out _);
			}
			return base.Visit(literal);
		}

		private Regex InterpolationPattern = new Regex(@"{(?<type>\w+):(?<id>[0-9]+)}");

		public bool InternalizeStringLiteral(StringLiteral literal, bool allowInterpolation, out Expression expression) {
			string key = literal.ExternalKey;

			if(!Script.Strings.TryGetValue(key, out var entry)) {
				Console.WriteLine($"Warning: Missing translation for {key} in '{Script.Name}'");
				expression = literal;
				return false;
			}

			string text = entry.CurrentTextVersion;
			if(text.Trim() == ".") {
				Console.WriteLine($"WARNING: single dot in {Path.GetFileNameWithoutExtension(Script.Name).ToLower()}, {key}");
				expression = literal;
				return false;
			}

			const bool prependLineId = true;
			if(prependLineId && key.StartsWith("L")) {
				text = $"@l({Path.GetFileNameWithoutExtension(Script.Name).ToLower()}, {key}){text}";
			}

			var matches = InterpolationPattern.Matches(text);

			if(matches.Count == 0) {
				literal.StringTable = null;
				literal.ExternalKey = null;

				literal.Value = text;

				expression = literal;
				return false;
			}

			var operands = new List<Expression>();
			int lastOperandEnd = 0;
			foreach(Match match in matches) {
				if(match.Index > lastOperandEnd) {
					operands.Add(new StringLiteral { Value = text.Substring(lastOperandEnd, match.Index - lastOperandEnd) });
				}

				operands.Add(new Variable {
					VariableType = match.Groups["type"].Value,
					VariableId = int.Parse(match.Groups["id"].Value)
				});

				lastOperandEnd = match.Index + match.Length;
			}

			if(text.Length > lastOperandEnd) {
				operands.Add(new StringLiteral { Value = text.Substring(lastOperandEnd, text.Length - lastOperandEnd) });
			}

			expression = new OperatorExpr {
				Operands = operands.ToArray(),
				Operators = Enumerable.Repeat("+", operands.Count - 1).ToArray()
			};
			return true;
		}

		public override void Visit(FunctionCallStmt stmt) {
			if(stmt.MethodName.IsOneOf(Options.YkdLineMethods) || stmt.MethodName.IsOneOf(Options.YkdNameMethods)) {
				// externalize line of text
				for(int i = 0; i < stmt.Arguments.Length; i++) {
					var argument = stmt.Arguments[i];
					if(argument is StringLiteral { IsExternalized: true } literal) {
						if(InternalizeStringLiteral(literal, true, out var expression)) {
							stmt.Arguments[i] = expression;
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
				base.Visit(stmt);
			}
		}
	}
}
