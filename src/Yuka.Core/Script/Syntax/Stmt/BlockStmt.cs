using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Yuka.Script.Syntax.Stmt {
	public class BlockStmt : StatementSyntaxNode {
		public List<StatementSyntaxNode> Statements = new List<StatementSyntaxNode>();

		public override string ToString() => $"{{\n  {string.Join("\n", Statements.Select(s => s?.ToString())).Replace("\n", "\n  ")}\n}}";

		[DebuggerStepThrough]
		public override void Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
