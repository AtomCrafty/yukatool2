using System.Collections.Generic;
using Yuka.Script.Instructions;

namespace Yuka.Script.Syntax.Expr {
	public class IntLiteral : ExpressionSyntaxNode {
		public int Value;

		public override string ToString() => Value.ToString();

		public override List<Instruction> Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
