using System.Collections.Generic;
using System.Linq;
using Yuka.Script.Instructions;

namespace Yuka.Script.Syntax.Stmt {
	public class FunctionCallStmt : StatementSyntaxNode {
		public string MethodName;
		public ExpressionSyntaxNode[] Arguments;

		public override string ToString() => $"{MethodName}({string.Join(", ", Arguments.Select(a => a.ToString()))});";

		public override List<Instruction> Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
