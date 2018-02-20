namespace Yuka.Script.Syntax.Stmt {
	public class AssignmentStmt : StatementSyntaxNode {
		public AssignmentTarget Target;
		public ExpressionSyntaxNode Expression;

		public override string ToString() => $"{Target} = {Expression};";
	}
}
