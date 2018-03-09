using System.Diagnostics;
using Yuka.IO;
using Yuka.Script.Data;
using Yuka.Script.Instructions;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script {
	public class YukaScript {

		public string Name;

		// only set if compiled
		public InstructionList InstructionList;

		// only set if decompiled
		public BlockStmt Body;
		public StringTable Strings;

		public YukaScript(string name, InstructionList instructions) {
			Name = name;
			InstructionList = instructions;
		}

		public YukaScript(string name, BlockStmt body) {
			Name = name;
			Body = body;
		}

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

			if(Options.YkdExternalizeStrings) {
				ExternalizeStrings();
			}
		}

		public void EnsureCompiled() => Compile();
		public void Compile() {
			if(IsCompiled) return;
			new Compiler(this).Compile();
		}

		public void ExternalizeStrings() {
			if(!IsDecompiled || Strings != null) return;

			Strings = new StringTable();
			new StringExternalizer(Strings).Visit(Body);
		}

		public void InternalizeStrings() {
			if(!IsDecompiled || Strings == null) return;

			new StringInternalizer(this).Visit(Body);

			Strings = null;
		}
	}
}