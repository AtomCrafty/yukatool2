namespace Yuka.Script.Syntax.Expr {
	public class VariableExpr : ExpressionSyntaxNode {
		public string FlagType;
		public int FlagId;

		public override string ToString() => $"[{FlagType}:{FlagId}]";
	}
}
