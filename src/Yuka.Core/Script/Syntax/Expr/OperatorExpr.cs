using System.Diagnostics;
using System.Linq;
using Yuka.Script.Data;

namespace Yuka.Script.Syntax.Expr {
	public class OperatorExpr : ExpressionSyntaxNode {
		public ExpressionSyntaxNode[] Operands;
		public string[] Operators;

		public override string ToString() => (Operands[0] is OperatorExpr ? $"({Operands[0]})" : Operands[0].ToString()) + string.Join("", Operators.Zip(Operands.Skip(1), (s, node) => $" {s} {(node is OperatorExpr ? $"({node})" : node.ToString())}"));

		[DebuggerStepThrough]
		public override DataElement Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
