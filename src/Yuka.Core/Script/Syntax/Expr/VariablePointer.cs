using System.Diagnostics;
using Yuka.Script.Data;

namespace Yuka.Script.Syntax.Expr {
	public class VariablePointer : Expression {
		public string FlagType;
		public int FlagPointerId;

		public override string ToString() => $"{FlagType}:&{FlagPointerId}";

		[DebuggerStepThrough]
		public override DataElement Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
