using System.Diagnostics;

namespace Yuka.Script.Syntax.Expr {
	public class Variable : Expression {
		public string VariableType;
		public int VariableId = -1;

		public override string ToString() => VariableId != -1 ? $"{VariableType}:{VariableId}" : VariableType;

		[DebuggerStepThrough]
		public override T Accept<T>(ISyntaxVisitor<T> visitor) => visitor.Visit(this);
	}
}
