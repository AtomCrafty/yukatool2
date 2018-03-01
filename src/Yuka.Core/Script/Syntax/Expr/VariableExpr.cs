using System.Diagnostics;
using Yuka.Script.Data;

namespace Yuka.Script.Syntax.Expr {
	public class VariableExpr : ExpressionSyntaxNode {
		public string FlagType;
		public int FlagId;

		public override string ToString() => $"[{FlagType}:{FlagId}]";

		[DebuggerStepThrough]
		public override DataElement Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
