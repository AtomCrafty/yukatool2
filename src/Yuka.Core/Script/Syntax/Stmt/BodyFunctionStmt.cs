using System.Collections.Generic;
using Yuka.Script.Instructions;

namespace Yuka.Script.Syntax.Stmt {
	public class BodyFunctionStmt : StatementSyntaxNode {
		public FunctionCallStmt Function;
		public BlockStmt Body;

		public override string ToString() => $"{Function.ToString().TrimEnd(';')} {Body}";

		public override List<Instruction> Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
