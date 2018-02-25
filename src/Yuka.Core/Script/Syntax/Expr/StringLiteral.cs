using System.Collections.Generic;
using Yuka.Script.Instructions;

namespace Yuka.Script.Syntax.Expr {
	public class StringLiteral : ExpressionSyntaxNode {
		public string Value;

		public override string ToString() => '"' + Value + '"';

		public override List<Instruction> Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
