using System.Diagnostics;
using Yuka.Script.Data;

namespace Yuka.Script.Syntax.Expr {
	public class StringLiteral : ExpressionSyntaxNode {
		public string Value;

		public override string ToString() => '"' + Value + '"';

		[DebuggerStepThrough]
		public override DataElement Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
