using System.Diagnostics;
using Yuka.Script.Data;
using Yuka.Util;

namespace Yuka.Script.Syntax.Expr {
	public class StringLiteral : Expression {
		public string Value;

		public override string ToString() => '"' + Value.Escape() + '"';

		[DebuggerStepThrough]
		public override DataElement Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
