using System;
using System.Diagnostics;
using Yuka.Script.Data;
using Yuka.Util;

namespace Yuka.Script.Syntax.Expr {
	public class StringLiteral : Expression {
		protected string _value;

		public StringTable StringTable { get; set; }
		public string ExternalId { get; set; }

		public bool IsExternalized => ExternalId != null;

		public string Value {
			get => IsExternalized ? StringTable[ExternalId].CurrentTextVersion : _value;
			set {
				if(IsExternalized) throw new InvalidOperationException("Unable to change value of externalized string constant");
				_value = value;
			}
		}

		public override string ToString() => IsExternalized ? '@' + ExternalId : '"' + Value.Escape() + '"';

		[DebuggerStepThrough]
		public override DataElement Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
	}
}
