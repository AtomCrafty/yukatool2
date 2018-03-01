using System.Diagnostics;
using System.Linq;

namespace Yuka.Script.Syntax.Stmt {
	public class FunctionCallStmt : StatementSyntaxNode {
		public string MethodName;
		public ExpressionSyntaxNode[] Arguments;

		public override string ToString() => $"{MethodName}({string.Join(", ", Arguments.Select(a => a.ToString()))});";

		[DebuggerStepThrough]
		public override void Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
