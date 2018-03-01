using System.Diagnostics;
using Yuka.Script.Instructions;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script {
	public class YukaScript {

		// only set if compiled
		public InstructionList InstructionList;
		// only set if decompiled
		public BlockStmt Body;

		public bool IsDecompiled => Body != null;

		public void EnsureDecompiled() {
			if(IsDecompiled) return;
			Debug.Assert(InstructionList == null, "Script is compiled and decompiled at the same time");
			Decompile();
		}

		public void Decompile() {
			new Decompiler(this).Decompile();
		}

		public void EnsureCompiled() {
			if(!IsDecompiled) return;
			Debug.Assert(InstructionList != null, "Script is neither compiled nor decompiled");
			Compile();
		}

		public void Compile() {
			new Compiler(this).Compile();
		}
	}
}