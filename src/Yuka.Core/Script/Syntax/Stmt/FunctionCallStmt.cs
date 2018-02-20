using System.Collections.Generic;
using System.Linq;

namespace Yuka.Script.Syntax.Stmt {
	public class FunctionCallStmt : StatementSyntaxNode {
		public string MethodName;
		public ExpressionSyntaxNode[] Arguments;

		public override string ToString() => $"{MethodName}({string.Join(", ", Arguments.Select(a => a.ToString()))});";
	}
}
