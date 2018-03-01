using System.Diagnostics;

namespace Yuka.Script.Syntax.Stmt {
	public class AssignmentStmt : Statement {
		public AssignmentTarget Target;
		public Expression Expression;

		public override string ToString() => $"{Target} = {Expression};";

		[DebuggerStepThrough]
		public override void Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
