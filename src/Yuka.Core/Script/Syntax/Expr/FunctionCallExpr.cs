using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script.Syntax.Expr {
	public class FunctionCallExpr : ExpressionSyntaxNode {
		public FunctionCallStmt CallStmt;

		public override string ToString() => CallStmt.ToString().TrimEnd(';');
	}
}
