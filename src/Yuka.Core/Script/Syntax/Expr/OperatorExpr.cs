﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script.Syntax.Expr {
	public class OperatorExpr : ExpressionSyntaxNode {
		public ExpressionSyntaxNode[] Operands;
		public string[] Operators;

		public override string ToString() => (Operands[0] is OperatorExpr ? $"({Operands[0]})" : Operands[0].ToString()) + string.Join("", Operators.Zip(Operands.Skip(1), (s, node) => $" {s} {(node is OperatorExpr ? $"({node})" : node.ToString())}"));
	}
}
