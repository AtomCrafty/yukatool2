using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Yuka.Script.Syntax.Stmt {
	public class BlockStmt : Statement {
		public List<Statement> Statements = new List<Statement>();

		public override string ToString() => $"{{\n  {string.Join("\n", Statements.Select(s => s?.ToString())).Replace("\n", "\n  ")}\n}}";

		[DebuggerStepThrough]
		public override void Accept<T>(ISyntaxVisitor<T> visitor) => visitor.Visit(this);
	}
}
