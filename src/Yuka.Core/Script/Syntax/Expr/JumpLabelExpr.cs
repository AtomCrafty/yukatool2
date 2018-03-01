using Yuka.Script.Data;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script.Syntax.Expr {
	public class JumpLabelExpr : Expression {
		public JumpLabelStmt LabelStmt;

		public override string ToString() => $":{LabelStmt.Name}";

		public override DataElement Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
