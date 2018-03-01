using System.Diagnostics;
using Yuka.Script.Data;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script.Syntax.Expr {
	public class FunctionCallExpr : Expression {
		public FunctionCallStmt CallStmt;

		public override string ToString() => CallStmt.ToString().TrimEnd(';');

		[DebuggerStepThrough]
		public override DataElement Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
