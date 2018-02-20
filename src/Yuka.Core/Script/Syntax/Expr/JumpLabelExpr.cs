using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script.Syntax.Expr {
	public class JumpLabelExpr : ExpressionSyntaxNode {
		public JumpLabelStmt LabelStmt;

		public override string ToString() => $":{LabelStmt.Name}";
	}
}
