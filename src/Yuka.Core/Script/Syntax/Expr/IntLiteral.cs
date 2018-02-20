namespace Yuka.Script.Syntax.Expr {
	public class IntLiteral : ExpressionSyntaxNode {
		public int Value;

		public override string ToString() => Value.ToString();
	}
}
