using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yuka.IO.Formats;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script.Syntax.Expr {
	public class StringLiteral : ExpressionSyntaxNode {
		public string Value;

		public override string ToString() => '"' + Value + '"';
	}
}
