using System.Collections.Generic;
using Yuka.Script.Instructions;

namespace Yuka.Script.Syntax {
	public abstract class SyntaxNode {
		public abstract List<Instruction> Accept(ISyntaxVisitor visitor);
	}

	public abstract class StatementSyntaxNode : SyntaxNode { }

	public abstract class ExpressionSyntaxNode : SyntaxNode { }
}
