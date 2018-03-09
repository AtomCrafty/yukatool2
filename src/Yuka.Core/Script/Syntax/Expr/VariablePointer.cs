using System.Diagnostics;
using Yuka.Script.Data;

namespace Yuka.Script.Syntax.Expr {
	public class VariablePointer : Expression {
		public string VariableType;
		public int PointerId;

		public override string ToString() => $"{VariableType}:&{PointerId}";

		[DebuggerStepThrough]
		public override DataElement Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
