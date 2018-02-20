namespace Yuka.Script.Syntax.Expr {
	public class StringLiteral : ExpressionSyntaxNode {
		public string Value;

		public override string ToString() => '"' + Value + '"';
	}
}
