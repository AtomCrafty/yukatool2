using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Yuka.IO;
using Yuka.IO.Formats;
using Yuka.Script.Data;
using Yuka.Script.Instructions;
using Yuka.Script.Syntax.Expr;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script {
	public class Compiler : ISyntaxVisitor {

		protected readonly YukaScript Script;

		public Compiler(YukaScript script) {
			Script = script;
		}

		public void Compile() {
			Debug.Assert(Script.IsDecompiled);

			throw new NotImplementedException();
		}

		internal static void WriteHeader(YksFormat.Header header, BinaryWriter w) {
			w.Write(header.Signature);
			w.Write(header.Encryption);
			w.Write(header.HeaderLength);
			w.Write(header.Unknown1);
			w.Write(header.InstrOffset);
			w.Write(header.InstrCount);
			w.Write(header.IndexOffset);
			w.Write(header.IndexCount);
			w.Write(header.DataOffset);
			w.Write(header.DataLength);
			w.Write(header.MaxLocals);
			w.Write(header.Unknown2);
		}

		#region Visitor methods

		public List<Instruction> Visit(FunctionCallExpr stmt) {
			throw new NotImplementedException();
		}

		public List<Instruction> Visit(IntLiteral stmt) {
			throw new NotImplementedException();
		}

		public List<Instruction> Visit(JumpLabelExpr stmt) {
			throw new NotImplementedException();
		}

		public List<Instruction> Visit(OperatorExpr stmt) {
			throw new NotImplementedException();
		}

		public List<Instruction> Visit(StringLiteral stmt) {
			throw new NotImplementedException();
		}

		public List<Instruction> Visit(VariableExpr stmt) {
			throw new NotImplementedException();
		}

		public List<Instruction> Visit(AssignmentStmt stmt) {
			throw new NotImplementedException();
		}

		public List<Instruction> Visit(BlockStmt stmt) {
			throw new NotImplementedException();
		}

		public List<Instruction> Visit(BodyFunctionStmt stmt) {
			throw new NotImplementedException();
		}

		public List<Instruction> Visit(FunctionCallStmt stmt) {
			throw new NotImplementedException();
		}

		public List<Instruction> Visit(IfStmt stmt) {
			throw new NotImplementedException();
		}

		public List<Instruction> Visit(JumpLabelStmt stmt) {
			throw new NotImplementedException();
		}

		#endregion
	}

	public interface ISyntaxVisitor {
		List<Instruction> Visit(FunctionCallExpr stmt);
		List<Instruction> Visit(IntLiteral stmt);
		List<Instruction> Visit(JumpLabelExpr stmt);
		List<Instruction> Visit(OperatorExpr stmt);
		List<Instruction> Visit(StringLiteral stmt);
		List<Instruction> Visit(VariableExpr stmt);

		List<Instruction> Visit(AssignmentStmt stmt);
		List<Instruction> Visit(BlockStmt stmt);
		List<Instruction> Visit(BodyFunctionStmt stmt);
		List<Instruction> Visit(FunctionCallStmt stmt);
		List<Instruction> Visit(IfStmt stmt);
		List<Instruction> Visit(JumpLabelStmt stmt);
	}
}