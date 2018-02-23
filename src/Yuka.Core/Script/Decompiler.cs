using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yuka.Script.Data;
using Yuka.Script.Instructions;
using Yuka.Script.Syntax;
using Yuka.Script.Syntax.Expr;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script {
	public class Decompiler {

		public const int LocalFlagTableSize = 65536;
		public const int GlobalFlagTableSize = 10000;
		public const int LocalStringTableSize = 65536;
		public const int GlobalStringTableSize = 10000;



		protected readonly YukaScript Script;

		protected Dictionary<uint, ExpressionSyntaxNode> _locals = new Dictionary<uint, ExpressionSyntaxNode>();
		protected int _currentInstructionOffset;
		protected AssignmentTarget _currentAssignmentTarget;

		public Decompiler(YukaScript script) {
			Script = script;
		}

		public void Decompile() {
			Debug.Assert(!Script.IsDecompiled);

			Script.Body = ReadBlockStatement();

			Script.Instructions = null;
			Script.Header = null;
			Script.Index = null;
		}

		protected Instruction CurrentInstruction => _currentInstructionOffset < Script.Instructions.Length ? Script.Instructions[_currentInstructionOffset] : null;
		protected Instruction ReadInstruction() => Script.Instructions[_currentInstructionOffset++];

		protected BlockStmt ReadBlockStatement() {
			var statements = new List<StatementSyntaxNode>();
			while(_currentInstructionOffset < Script.Instructions.Length) {

				// check if block end was reached
				if(CurrentInstruction is LabelInstruction label && label.Name == "}") {
					// skip closing brace
					_currentInstructionOffset++;
					break;
				}

				// otherwise, read statement
				var stmt = ReadStatement();
				if(stmt != null) statements.Add(stmt);
			}
			return new BlockStmt { Statements = statements };
		}

		protected void SetAssignmentTarget(TargetInstruction instruction) {
			if(_currentAssignmentTarget != null) throw new FormatException("Assignment target already set");
			switch(instruction.Target) {
				case DataElement.SStr sstr:
					_currentAssignmentTarget = new AssignmentTarget.SpecialString(sstr.FlagType);
					break;
				case DataElement.VInt vint when vint.FlagType == "GlobalFlag":
					if(vint.FlagId >= GlobalFlagTableSize)
						throw new ArgumentOutOfRangeException(nameof(vint.FlagId), vint.FlagId, "Global flag index must be smaller than " + GlobalFlagTableSize);
					_currentAssignmentTarget = new AssignmentTarget.GlobalFlag(vint.FlagId);
					break;
				case DataElement.VInt vint when vint.FlagType == "Flag":
					if(vint.FlagId >= LocalFlagTableSize)
						throw new ArgumentOutOfRangeException(nameof(vint.FlagId), vint.FlagId, "Local flag index must be smaller than " + LocalFlagTableSize);
					_currentAssignmentTarget = new AssignmentTarget.LocalFlag(vint.FlagId);
					break;
				case DataElement.VStr vstr when vstr.FlagType == "GlobalString":
					if(vstr.FlagId >= GlobalFlagTableSize)
						throw new ArgumentOutOfRangeException(nameof(vstr.FlagId), vstr.FlagId, "Global string index must be smaller than " + GlobalStringTableSize);
					_currentAssignmentTarget = new AssignmentTarget.GlobalString(vstr.FlagId);
					break;
				case DataElement.VStr vstr when vstr.FlagType == "String":
					if(vstr.FlagId >= LocalFlagTableSize)
						throw new ArgumentOutOfRangeException(nameof(vstr.FlagId), vstr.FlagId, "Local string index must be smaller than " + LocalStringTableSize);
					_currentAssignmentTarget = new AssignmentTarget.LocalString(vstr.FlagId);
					break;
				case DataElement.VLoc vloc:
					if(vloc.Id >= Script.Header.LocalCount)
						throw new ArgumentOutOfRangeException(nameof(vloc.Id), vloc.Id, "Local variable id must be smaller than local variable pool size (" + Script.Header.LocalCount + ")");
					_currentAssignmentTarget = new AssignmentTarget.Local(vloc.Id);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(instruction), "Invalid assignment target: " + instruction);
			}
		}

		protected ExpressionSyntaxNode ToExpression(DataElement element) {
			switch(element) {
				case DataElement.Ctrl ctrl:
					return new JumpLabelExpr { LabelStmt = new JumpLabelStmt { Name = ctrl.Name } };
				case DataElement.CInt cint:
					return new IntLiteral { Value = cint.Value };
				case DataElement.CStr cstr:
					return new StringLiteral { Value = cstr.Value };
				case DataElement.SStr sstr:
					return new VariableExpr { FlagType = sstr.FlagType };
				case DataElement.VInt vint:
					return new VariableExpr { FlagType = vint.FlagType, FlagId = vint.FlagId };
				case DataElement.VStr vstr:
					return new VariableExpr { FlagType = vstr.FlagType, FlagId = vstr.FlagId };
				case DataElement.VLoc vloc:
					if(!_locals.ContainsKey(vloc.Id)) throw new FormatException("Use of undefined local variable");
					var local = _locals[vloc.Id];
					if(local == null) throw new FormatException("Repeated use of the same local variable");
					_locals[vloc.Id] = null;
					return local;
				default:
					throw new FormatException("Invalid expression element: " + element.Type);
			}
		}

		protected ExpressionSyntaxNode[] ToExpressions(DataElement[] elements) {
			return elements.Select(ToExpression).ToArray();
		}

		protected ExpressionSyntaxNode ToOperatorExpression(DataElement[] parts) {
			// odd number of elements (one less operator than operands)
			Debug.Assert(parts.Length % 2 == 1);
			var operators = new string[parts.Length / 2];
			var operands = new ExpressionSyntaxNode[parts.Length / 2 + 1];

			for(int i = 0; i < parts.Length; i++) {
				if(i % 2 == 0) {
					Debug.Assert(!(parts[i] is DataElement.Ctrl), "Operators are only allowed at odd indices");
					operands[i / 2] = ToExpression(parts[i]);
				}
				else {
					Debug.Assert(parts[i] is DataElement.Ctrl, "Operands are only allowed at even indices");
					operators[i / 2] = ((DataElement.Ctrl)parts[i]).Name;
				}
			}
			return new OperatorExpr { Operands = operands, Operators = operators };
		}

		protected StatementSyntaxNode ReadStatement() {
			var instruction = ReadInstruction();
			switch(instruction) {

				case LabelInstruction label:
					Debug.Assert(label.Name != "else" && label.Name != "{" && label.Name != "}");
					return new JumpLabelStmt { Name = label.Name };

				case CallInstruction func:

					#region Handle assignments
					if(func.Name == "=") {
						if(_currentAssignmentTarget == null) throw new FormatException("No assignment target set");

						ExpressionSyntaxNode expr;
						if(func.Arguments.Length == 0) {
							// a call to =() with no arguments means the result of the following function call is assigned
							if(CurrentInstruction is CallInstruction callInstruction) {
								expr = new FunctionCallExpr { CallStmt = new FunctionCallStmt { MethodName = callInstruction.Name, Arguments = ToExpressions(callInstruction.Arguments) } };
								_currentInstructionOffset++;
							}
							else {
								throw new FormatException("A parameterless call to =() must be followed by a function call, found " + CurrentInstruction);
							}
						}
						else {
							// otherwise, the arguments are alternating operands and operators
							expr = ToOperatorExpression(func.Arguments);
						}

						if(_currentAssignmentTarget is AssignmentTarget.Local local) {
							_locals[local.Id] = expr;
							_currentAssignmentTarget = null;
							return null;
						}

						var assigment = new AssignmentStmt { Target = _currentAssignmentTarget, Expression = expr };
						_currentAssignmentTarget = null;
						return assigment;
					}
					#endregion

					var callStatement = new FunctionCallStmt { MethodName = func.Name, Arguments = ToExpressions(func.Arguments) };

					#region Handle body functions

					if(CurrentInstruction is LabelInstruction startLabel && startLabel.Name == "{") {
						_currentInstructionOffset++; // skip opening brace

						var body = ReadBlockStatement();

						#region Handle if statements

						if(callStatement.MethodName.ToLower() == "if") {
							if(!(Script.Instructions[_currentInstructionOffset] is LabelInstruction elseKeyword) || elseKeyword.Name != "else")
								// no else block
								return new IfStmt { Function = callStatement, Body = body };

							// skip else keyword
							_currentInstructionOffset++;
							if(!(Script.Instructions[_currentInstructionOffset] is LabelInstruction elseStart) || elseStart.Name != "{")
								throw new FormatException("Else keyword must be followed by an opening brace");

							// skip opening brace
							_currentInstructionOffset++;
							var elseBody = ReadBlockStatement();

							return new IfStmt { Function = callStatement, Body = body, ElseBody = elseBody };
						}

						#endregion

						return new BodyFunctionStmt { Function = callStatement, Body = body };
					}

					#endregion

					return callStatement;

				case TargetInstruction target:
					SetAssignmentTarget(target);
					return null;

				default:
					throw new FormatException("Invalid statement instruction: " + instruction);
			}
		}
	}
}
