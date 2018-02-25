using System.Collections.Generic;
using Yuka.Script.Instructions;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script.Syntax.Expr {
	public class JumpLabelExpr : ExpressionSyntaxNode {
		public JumpLabelStmt LabelStmt;

		public override string ToString() => $":{LabelStmt.Name}";

		public override List<Instruction> Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
