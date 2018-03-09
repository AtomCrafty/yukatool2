using Yuka.Script.Data;

namespace Yuka.Script.Syntax {
	public abstract class SyntaxNode {
	}

	public abstract class Statement : SyntaxNode {
		public abstract void Accept<T>(ISyntaxVisitor<T> visitor);
	}

	public abstract class Expression : SyntaxNode {
		public abstract T Accept<T>(ISyntaxVisitor<T> visitor);
	}
}
