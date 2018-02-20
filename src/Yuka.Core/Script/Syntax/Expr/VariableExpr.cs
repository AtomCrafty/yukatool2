using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuka.IO.Formats;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script.Syntax.Expr {
	public class VariableExpr : ExpressionSyntaxNode {
		public string FlagType;
		public int FlagId;

		public override string ToString() => $"[{FlagType}:{FlagId}]";
	}
}
