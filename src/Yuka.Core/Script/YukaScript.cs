using Yuka.IO.Formats;
using Yuka.Script.Data;
using Yuka.Script.Instructions;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script {
	public class YukaScript {

		internal YksFormat.Header Header;
		public DataElement[] Index;
		public Instruction[] Instructions;

		public BlockStmt Body;

		public bool IsDecompiled => Body != null;

		public void EnsureDecompiled() {
			if(IsDecompiled) return;
			Decompile();
		}

		public void Decompile() {
			new Decompiler(this).Decompile();
		}
	}
}