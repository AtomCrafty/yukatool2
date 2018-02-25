using System.Collections.Generic;
using Yuka.Script.Instructions;

namespace Yuka.Script.Syntax.Stmt {
	public class AssignmentStmt : StatementSyntaxNode {
		public AssignmentTarget Target;
		public ExpressionSyntaxNode Expression;

		public override string ToString() => $"{Target} = {Expression};";

		public override List<Instruction> Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
