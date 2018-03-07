using System.Diagnostics;
using Yuka.Script.Data;

namespace Yuka.Script.Syntax.Expr {
	public class IntPointer : Expression {
		public int PointerId;

		public override string ToString() => '&' + PointerId.ToString();

		[DebuggerStepThrough]
		public override DataElement Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
