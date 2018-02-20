namespace Yuka.Script.Syntax.Stmt {
	public class JumpLabelStmt : StatementSyntaxNode {
		public string Name;

		public override string ToString() => $"{Name}:";
	}
}
