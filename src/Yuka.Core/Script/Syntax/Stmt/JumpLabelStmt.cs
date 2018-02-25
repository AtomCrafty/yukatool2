using System.Collections.Generic;
using Yuka.Script.Instructions;

namespace Yuka.Script.Syntax.Stmt {
	public class JumpLabelStmt : StatementSyntaxNode {
		public string Name;

		public override string ToString() => $"{Name}:";

		public override List<Instruction> Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
