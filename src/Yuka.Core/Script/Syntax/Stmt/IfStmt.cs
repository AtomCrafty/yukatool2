using System.Collections.Generic;
using System.Linq;

namespace Yuka.Script.Syntax.Stmt {
	public class IfStmt : BodyFunctionStmt {
		public BlockStmt ElseBody;

		public override string ToString() => ElseBody != null ? base.ToString() : base.ToString() + "\nelse " + ElseBody;
	}
}
