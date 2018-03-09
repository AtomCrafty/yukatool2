using System.Diagnostics;
using Yuka.Script.Data;

namespace Yuka.Script.Syntax.Expr {
	public class IntegerLiteral : Expression {
		public int Value;

		public override string ToString() => Value.ToString();

		[DebuggerStepThrough]
		public override T Accept<T>(ISyntaxVisitor<T> visitor) => visitor.Visit(this);
	}
}
