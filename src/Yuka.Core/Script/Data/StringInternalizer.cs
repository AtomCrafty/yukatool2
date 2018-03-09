using System;
using Yuka.Script.Syntax;
using Yuka.Script.Syntax.Expr;

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

			if(Script.Strings.ContainsKey(key)) {
				literal.Value = Script.Strings[key].CurrentTextVersion;
			}
			else {
				Console.WriteLine($"Warning: Missing translation for {key} in '{Script.Name}'");
			}
		}

		public override object Visit(StringLiteral literal) {
			if(literal.IsExternalized) {
				InternalizeStringLiteral(literal);
			}
			return base.Visit(literal);
		}
	}
}
