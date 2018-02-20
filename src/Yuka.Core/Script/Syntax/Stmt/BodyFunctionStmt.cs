namespace Yuka.Script.Syntax.Stmt {
	public class BodyFunctionStmt : StatementSyntaxNode {
		public FunctionCallStmt Function;
		public BlockStmt Body;

		public override string ToString() => $"{Function.ToString().TrimEnd(';')} {Body}";
	}
}
