using System.Diagnostics;
using Yuka.Script.Data;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script.Syntax.Expr {
	public class FunctionCallExpr : Expression {
		public FunctionCallStmt CallStmt;

		public override string ToString() => CallStmt.ToString().TrimEnd(';');

		[DebuggerStepThrough]
		public override T Accept<T>(ISyntaxVisitor<T> visitor) => visitor.Visit(this);
	}
}
