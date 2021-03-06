﻿using System.Diagnostics;

namespace Yuka.Script.Syntax.Expr {
	public class VariablePointer : Expression {
		public string VariableType;
		public int PointerId;

		public override string ToString() => $"{VariableType}:&{PointerId}";

		[DebuggerStepThrough]
		public override T Accept<T>(ISyntaxVisitor<T> visitor) => visitor.Visit(this);
	}
}
