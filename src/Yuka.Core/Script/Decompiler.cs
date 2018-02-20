using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using Yuka.IO;
using Yuka.IO.Formats;
using Yuka.Script.Syntax;
using Yuka.Script.Syntax.Expr;
using Yuka.Script.Syntax.Stmt;
using Yuka.Util;

namespace Yuka.Script {
	public class Decompiler {

		public YukaScript Disassemble(string name, Stream s) {

			#region Prepare data

			var r = s.NewReader();

			var header = ReadHeader(r);

			// read instructions
			var instr = new uint[header.InstrCount];
			s.Seek(header.InstrOffset);
			for(int i = 0; i < header.InstrCount; i++) {
				instr[i] = r.ReadUInt32();
			}

			// prepare data stream
			Stream dataStream = new ReadOnlySubStream(s, header.DataOffset, header.DataLength);
			if(header.Encryption == 1) dataStream = new XorStream(dataStream, Options.ScriptDataXorKey);
			var dataReader = dataStream.NewReader();

			// read index
			var index = new YksFormat.IndexEntry[header.IndexCount];
			s.Seek(header.IndexOffset);
			for(int i = 0; i < header.IndexCount; i++) {
				index[i] = ReadIndexEntry(r, dataReader);
			}

			foreach(var e in index) Console.WriteLine(e);

			#endregion





			#region Disassemble code

			AssignmentTarget currentAssignmentTarget = null;
			var locals = new Dictionary<uint, ExpressionSyntaxNode>();

			int instrIndex = 0;

			// iterate instructions
			return new YukaScript { Body = ReadBlockStatement() };

			#endregion

			YksFormat.IndexEntry PeekEntry() => instrIndex < instr.Length ? index[instr[instrIndex]] : null;
			YksFormat.IndexEntry ReadEntry() => index[instr[instrIndex++]];

			BlockStmt ReadBlockStatement() {
				var statements = new List<StatementSyntaxNode>();
				while(instrIndex < instr.Length) {

					// check if block end was reached
					if(PeekEntry() is YksFormat.IndexEntry.Ctrl ctrl && ctrl.Name == "}") {
						// skip closing brace
						instrIndex++;
						break;
					}

					// otherwise, read statement
					var stmt = ReadStatement();
					if(stmt != null) statements.Add(stmt);
				}
				return new BlockStmt { Statements = statements };
			}

			StatementSyntaxNode ReadStatement() {
				var entry = ReadEntry();
				switch(entry) {
					case YksFormat.IndexEntry.Ctrl ctrl:
						Debug.Assert(ctrl.Name != "else" && ctrl.Name != "{" && ctrl.Name != "}");
						return new JumpLabelStmt { Name = ctrl.Name };
					case YksFormat.IndexEntry.Func func:
						// read call arguments
						uint argc = instr[instrIndex++];
						var argv = new ExpressionSyntaxNode[argc];
						for(int i = 0; i < argc; i++) {
							argv[i] = ReadExpression();
						}

						// handle special functions
						if(func.Name == "=") {
							if(currentAssignmentTarget == null) throw new InvalidOperationException("No assignment target set");

							var expr = argc == 0 ? ReadExpression() : ParseExpression(argv);

							if(currentAssignmentTarget is AssignmentTarget.Local local) {
								locals[local.Id] = expr;
								currentAssignmentTarget = null;
								return null;
							}

							var assigment = new AssignmentStmt { Target = currentAssignmentTarget, Expression = expr };
							currentAssignmentTarget = null;
							return assigment;
						}
						var call = new FunctionCallStmt { MethodName = func.Name, Arguments = argv };

						if(!(PeekEntry() is YksFormat.IndexEntry.Ctrl start) || start.Name != "{")
							// regular function call (no body)
							return call;

						// skip opening brace
						instrIndex++;
						var body = ReadBlockStatement();

						if(call.MethodName.ToLower() != "if")
							return new BodyFunctionStmt { Function = call, Body = body };

						if(!(PeekEntry() is YksFormat.IndexEntry.Ctrl elsekw) || elsekw.Name != "else")
							return new IfStmt { Function = call, Body = body };

						// skip else keyword
						instrIndex++;
						if(!(PeekEntry() is YksFormat.IndexEntry.Ctrl elseStart) || elseStart.Name != "{")
							throw new InvalidOperationException("Else keyword must be followed by an opening brace");

						// skip opening brace
						instrIndex++;
						var elseBody = ReadBlockStatement();

						return new IfStmt { Function = call, Body = body, ElseBody = elseBody };
					// ReSharper disable UnusedVariable
					case YksFormat.IndexEntry.SStr sstr:
					case YksFormat.IndexEntry.VInt vint:
					case YksFormat.IndexEntry.VStr vstr:
					case YksFormat.IndexEntry.VLoc vloc:
						SetAssignmentTarget(entry);
						return null;
					// ReSharper enable UnusedVariable
					default:
						throw new InvalidOperationException("Invalid statement type: " + entry.Type);
				}
			}

			ExpressionSyntaxNode ParseExpression(ExpressionSyntaxNode[] parts) {
				// odd number of elements (one less operator than operands)
				Debug.Assert(parts.Length % 2 == 1);
				var operators = new string[parts.Length / 2];
				var operands = new ExpressionSyntaxNode[parts.Length / 2 + 1];

				for(int i = 0; i < parts.Length; i++) {
					if(i % 2 == 0) {
						Debug.Assert(!(parts[i] is JumpLabelExpr), "Operators are only allowed at odd indices");
						operands[i / 2] = parts[i];
					}
					else {
						Debug.Assert(parts[i] is JumpLabelExpr, "Operands are only allowed at even indices");
						string op = ((JumpLabelExpr)parts[i]).LabelStmt.Name;
						operators[i / 2] = op;
					}
				}
				return new OperatorExpr { Operands = operands, Operators = operators };
			}

			ExpressionSyntaxNode ReadExpression() {
				var entry = ReadEntry();
				switch(entry) {
					case YksFormat.IndexEntry.Func func:
						// re-read as statement
						instrIndex--;
						return new FunctionCallExpr { CallStmt = (FunctionCallStmt)ReadStatement() };
					case YksFormat.IndexEntry.Ctrl ctrl:
						return new JumpLabelExpr { LabelStmt = new JumpLabelStmt { Name = ctrl.Name } };
					case YksFormat.IndexEntry.CInt cint:
						return new IntLiteral { Value = cint.Value };
					case YksFormat.IndexEntry.CStr cstr:
						return new StringLiteral { Value = cstr.Value };
					case YksFormat.IndexEntry.SStr sstr:
						return new VariableExpr { FlagType = sstr.FlagType };
					case YksFormat.IndexEntry.VInt vint:
						return new VariableExpr { FlagType = vint.FlagType, FlagId = vint.FlagId };
					case YksFormat.IndexEntry.VStr vstr:
						return new VariableExpr { FlagType = vstr.FlagType, FlagId = vstr.FlagId };
					case YksFormat.IndexEntry.VLoc vloc:
						if(!locals.ContainsKey(vloc.Id)) throw new InvalidOperationException("Use of undefined local variable");
						var local = locals[vloc.Id];
						if(local == null) throw new InvalidOperationException("Repeated use of the same local variable");
						locals[vloc.Id] = null;
						return local;
					default:
						throw new InvalidOperationException("Invalid expression type: " + entry.Type);
				}
			}

			void SetAssignmentTarget(YksFormat.IndexEntry entry) {
				if(currentAssignmentTarget != null) throw new InvalidOperationException("Assignment target already set");
				switch(entry) {
					case YksFormat.IndexEntry.SStr sstr:
						currentAssignmentTarget = new AssignmentTarget.SpecialString(sstr.FlagType);
						break;
					case YksFormat.IndexEntry.VInt vint when vint.FlagType == "GlobalFlag":
						currentAssignmentTarget = new AssignmentTarget.GlobalFlag(vint.FlagId);
						break;
					case YksFormat.IndexEntry.VInt vint when vint.FlagType == "Flag":
						currentAssignmentTarget = new AssignmentTarget.LocalFlag(vint.FlagId);
						break;
					case YksFormat.IndexEntry.VStr vstr when vstr.FlagType == "GlobalString":
						currentAssignmentTarget = new AssignmentTarget.GlobalString(vstr.FlagId);
						break;
					case YksFormat.IndexEntry.VStr vstr when vstr.FlagType == "String":
						currentAssignmentTarget = new AssignmentTarget.LocalString(vstr.FlagId);
						break;
					case YksFormat.IndexEntry.VLoc vloc:
						currentAssignmentTarget = new AssignmentTarget.Local(vloc.Id);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(entry), "Invalid assignment target: " + entry);
				}
			}
		}

		internal static YksFormat.IndexEntry ReadIndexEntry(BinaryReader index, BinaryReader data) {
			var type = (YksFormat.IndexEntryType)index.ReadUInt32();
			uint field1 = index.ReadUInt32();
			uint field2 = index.ReadUInt32();
			uint field3 = index.ReadUInt32();

			switch(type) {
				case YksFormat.IndexEntryType.Func:
					return new YksFormat.IndexEntry.Func(field1, field2, field3, data);
				case YksFormat.IndexEntryType.Ctrl:
					return new YksFormat.IndexEntry.Ctrl(field1, field2, field3, data);
				case YksFormat.IndexEntryType.CInt:
					return new YksFormat.IndexEntry.CInt(field1, field2, field3, data);
				case YksFormat.IndexEntryType.CStr:
					return new YksFormat.IndexEntry.CStr(field1, field2, field3, data);
				case YksFormat.IndexEntryType.SStr:
					return new YksFormat.IndexEntry.SStr(field1, field2, field3, data);
				case YksFormat.IndexEntryType.VInt:
					return new YksFormat.IndexEntry.VInt(field1, field2, field3, data);
				case YksFormat.IndexEntryType.VStr:
					return new YksFormat.IndexEntry.VStr(field1, field2, field3, data);
				case YksFormat.IndexEntryType.VLoc:
					return new YksFormat.IndexEntry.VLoc(field1, field2, field3, data);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported index entry type");
			}
		}

		internal static YksFormat.Header ReadHeader(BinaryReader r) {
			return new YksFormat.Header {
				Signature = r.ReadBytes(6),
				Encryption = r.ReadInt16(),
				HeaderLength = r.ReadInt32(),
				Unknown1 = r.ReadUInt32(),
				InstrOffset = r.ReadUInt32(),
				InstrCount = r.ReadUInt32(),
				IndexOffset = r.ReadUInt32(),
				IndexCount = r.ReadUInt32(),
				DataOffset = r.ReadUInt32(),
				DataLength = r.ReadUInt32(),
				LocalCount = r.ReadUInt32(),
				Unknown2 = r.ReadUInt32()
			};
		}
	}
}
