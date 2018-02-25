using System.Collections.Generic;
using Yuka.Script.Instructions;

namespace Yuka.Script.Syntax.Expr {
	public class VariableExpr : ExpressionSyntaxNode {
		public string FlagType;
		public int FlagId;

		public override string ToString() => $"[{FlagType}:{FlagId}]";

		public override List<Instruction> Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
