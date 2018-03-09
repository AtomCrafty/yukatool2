using System.Diagnostics;
using Yuka.Script.Data;
using Yuka.Script.Instructions;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script {
	public class YukaScript {

		// only set if compiled
		public InstructionList InstructionList;
		// only set if decompiled
		public BlockStmt Body;
		public StringTable Strings;

		public bool IsCompiled {
			get {
				Debug.Assert(Body == null || InstructionList == null, "Script is neither compiled nor decompiled");
				Debug.Assert(Body != null || InstructionList != null, "Script is compiled and decompiled at the same time");
				return InstructionList != null;
			}
		}
		public bool IsDecompiled {
			get {
				Debug.Assert(Body == null || InstructionList == null, "Script is neither compiled nor decompiled");
				Debug.Assert(Body != null || InstructionList != null, "Script is compiled and decompiled at the same time");
				return Body != null;
			}
		}

		public void EnsureDecompiled() => Decompile();
		public void Decompile() {
			if(IsDecompiled) return;
			new Decompiler(this).Decompile();
		}

		public void EnsureCompiled() => Compile();
		public void Compile() {
			if(IsCompiled) return;
			new Compiler(this).Compile();
		}
	}
}