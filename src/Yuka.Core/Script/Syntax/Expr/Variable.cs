using System.Diagnostics;
using Yuka.Script.Data;

namespace Yuka.Script.Syntax.Expr {
	public class Variable : Expression {
		public string VariableType;
		public int VariableId = -1;

		public override string ToString() => VariableId != -1 ? $"{VariableType}:{VariableId}" : VariableType;

		[DebuggerStepThrough]
		public override DataElement Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
