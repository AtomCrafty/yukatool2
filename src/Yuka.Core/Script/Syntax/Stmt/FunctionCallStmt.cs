using System.Diagnostics;
using System.Linq;

namespace Yuka.Script.Syntax.Stmt {
	public class FunctionCallStmt : Statement {
		public string MethodName;
		public Expression[] Arguments;

		public override string ToString() => $"{MethodName}({string.Join(", ", Arguments.Select(a => a.ToString()))});";

		[DebuggerStepThrough]
		public override void Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
