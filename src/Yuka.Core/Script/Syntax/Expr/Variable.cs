using System.Diagnostics;
using Yuka.Script.Data;

namespace Yuka.Script.Syntax.Expr {
	public class Variable : Expression {
		public string FlagType;
		public int FlagId;

		public int FlagPointerId;
		public bool IsPointer;

		public override string ToString() => IsPointer ? $"{FlagType}:&{FlagPointerId}" : FlagId != -1 ? $"{FlagType}:{FlagId}" : FlagType;

		[DebuggerStepThrough]
		public override DataElement Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
