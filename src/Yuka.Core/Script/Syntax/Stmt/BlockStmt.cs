using System.Collections.Generic;
using System.Linq;
using Yuka.Script.Instructions;

namespace Yuka.Script.Syntax.Stmt {
	public class BlockStmt : StatementSyntaxNode {
		public List<StatementSyntaxNode> Statements = new List<StatementSyntaxNode>();

		public override string ToString() => $"{{\n  {string.Join("\n", Statements.Select(s => s?.ToString())).Replace("\n", "\n  ")}\n}}";

		public override List<Instruction> Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
