using System.Collections.Generic;
using Yuka.Script.Instructions;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script.Syntax.Expr {
	public class FunctionCallExpr : ExpressionSyntaxNode {
		public FunctionCallStmt CallStmt;

		public override string ToString() => CallStmt.ToString().TrimEnd(';');

		public override List<Instruction> Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
