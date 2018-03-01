using System.Diagnostics;
using Yuka.Script.Data;

namespace Yuka.Script.Syntax {
	public abstract class SyntaxNode {
	}

	public abstract class StatementSyntaxNode : SyntaxNode {
		public abstract void Accept(ISyntaxVisitor visitor);
	}

	public abstract class ExpressionSyntaxNode : SyntaxNode {
		public abstract DataElement Accept(ISyntaxVisitor visitor);
	}
}
