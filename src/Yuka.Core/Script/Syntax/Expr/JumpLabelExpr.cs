using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script.Syntax.Expr {
	public class JumpLabelExpr : ExpressionSyntaxNode {
		public JumpLabelStmt LabelStmt;

		public override string ToString() => $":{LabelStmt.Name}";
	}
}
