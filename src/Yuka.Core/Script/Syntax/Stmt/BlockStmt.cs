using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Yuka.Script.Syntax.Stmt {
	public class BlockStmt : Statement {
		public List<Statement> Statements = new List<Statement>();

		public override string ToString() => $"{{{Environment.NewLine}  {string.Join(Environment.NewLine, Statements.Select(s => s?.ToString())).Replace("\n", "\n  ")}{Environment.NewLine}}}";

		[DebuggerStepThrough]
		public override void Accept<T>(ISyntaxVisitor<T> visitor) => visitor.Visit(this);
	}
}
