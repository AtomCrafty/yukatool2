using System.Diagnostics;

namespace Yuka.Script.Syntax.Stmt {
	public class JumpLabelStmt : Statement {
		public string Name;

		public override string ToString() => $"{Name}:";

		[DebuggerStepThrough]
		public override void Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
