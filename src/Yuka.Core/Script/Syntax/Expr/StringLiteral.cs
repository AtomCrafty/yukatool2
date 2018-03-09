using System;
using System.Diagnostics;
using Yuka.Script.Data;
using Yuka.Util;

namespace Yuka.Script.Syntax.Expr {
	public class StringLiteral : Expression {
		protected string _value;

		public StringTable StringTable { get; set; }
		public string ExternalKey { get; set; }

		public bool IsExternalized => ExternalKey != null;

		public string Value {
			get => IsExternalized ? StringTable[ExternalKey].CurrentTextVersion : _value;
			set {
				if(IsExternalized) throw new InvalidOperationException("Unable to change value of externalized string constant");
				_value = value;
			}
		}

		public override string ToString() => IsExternalized ? '@' + ExternalKey : '"' + Value.Escape() + '"';

		[DebuggerStepThrough]
		public override T Accept<T>(ISyntaxVisitor<T> visitor) => visitor.Visit(this);
	}
}
