using System.Diagnostics;
using Yuka.Script.Data;

namespace Yuka.Script.Syntax {
	public abstract class SyntaxNode {
	}

	public abstract class Statement : SyntaxNode {
		public abstract void Accept(ISyntaxVisitor visitor);
	}

	public abstract class Expression : SyntaxNode {
		public abstract DataElement Accept(ISyntaxVisitor visitor);
	}
}
