using System.Diagnostics;

namespace Yuka.Script.Syntax.Stmt {
	public class IfStmt : BodyFunctionStmt {
		public BlockStmt ElseBody;

		public override string ToString() => ElseBody != null ? base.ToString() + "\nelse " + ElseBody : base.ToString();

		[DebuggerStepThrough]
		public override void Accept<T>(ISyntaxVisitor<T> visitor) => visitor.Visit(this);
	}
}
