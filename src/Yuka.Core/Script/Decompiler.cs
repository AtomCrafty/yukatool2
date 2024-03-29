﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yuka.IO.Formats;
using Yuka.Script.Data;
using Yuka.Script.Instructions;
using Yuka.Script.Syntax;
using Yuka.Script.Syntax.Expr;
using Yuka.Script.Syntax.Stmt;
using Yuka.Util;

namespace Yuka.Script {

	/// <summary>
	/// Converts an instruction list tree to a syntax
	/// </summary>
	public class Decompiler {

		public const int LocalFlagTableSize = 65536;
		public const int GlobalFlagTableSize = 10000;
		public const int LocalStringTableSize = 65536;
		public const int GlobalStringTableSize = 10000;

		protected readonly YukaScript Script;

		protected Dictionary<uint, Expression> _locals = new Dictionary<uint, Expression>();
		protected int _currentInstructionOffset;
		protected AssignmentTarget _currentAssignmentTarget;
		private int _flagPointerId;

		public Decompiler(YukaScript script) {
			Script = script;
		}

		public void Decompile() {
			Debug.Assert(!Script.IsDecompiled);

			Script.Body = ReadBlockStatement();
			Script.InstructionList = null;

			// TODO externalize string table
		}

		protected Instruction CurrentInstruction => _currentInstructionOffset < Script.InstructionList.Count ? Script.InstructionList[_currentInstructionOffset] : null;
		protected Instruction ReadInstruction() => Script.InstructionList[_currentInstructionOffset++];

		protected BlockStmt ReadBlockStatement() {
			var statements = new List<Statement>();
			while(_currentInstructionOffset < Script.InstructionList.Count) {

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

				// special strings
				case DataElement.SStr sstr:
					_currentAssignmentTarget = new AssignmentTarget.SpecialString(sstr.FlagType.StringValue);
					break;




				// integer variable pointers
				case DataElement.VInt vint when vint.FlagId.IsPointer:
					_currentAssignmentTarget = new AssignmentTarget.VariablePointer(vint.FlagType.StringValue, vint.FlagId.PointerId);
					break;



				// integer variables
				case DataElement.VInt vint:
					// range checks
					if(vint.FlagId.IntValue < 0) throw new ArgumentOutOfRangeException(nameof(vint.FlagId.IntValue), vint.FlagId.IntValue, "Flag index must be positive");
					if(vint.FlagId.IntValue >= GlobalFlagTableSize && vint.FlagType.StringValue == YksFormat.GlobalFlag)
						throw new ArgumentOutOfRangeException(nameof(vint.FlagId.IntValue), vint.FlagId.IntValue, "Global flag index must be smaller than " + GlobalFlagTableSize);
					if(vint.FlagId.IntValue >= LocalFlagTableSize && vint.FlagType.StringValue == YksFormat.Flag)
						throw new ArgumentOutOfRangeException(nameof(vint.FlagId.IntValue), vint.FlagId.IntValue, "Local flag index must be smaller than " + LocalFlagTableSize);

					_currentAssignmentTarget = new AssignmentTarget.Variable(vint.FlagType.StringValue, vint.FlagId.IntValue);
					break;



				// string variable pointers
				case DataElement.VStr vstr when vstr.FlagId.IsPointer:
					_currentAssignmentTarget = new AssignmentTarget.VariablePointer(vstr.FlagType.StringValue, vstr.FlagId.PointerId);
					break;



				// integer variables
				case DataElement.VStr vstr:
					// range checks
					if(vstr.FlagId.IntValue < 0) throw new ArgumentOutOfRangeException(nameof(vstr.FlagId.IntValue), vstr.FlagId.IntValue, "String index must be positive");
					if(vstr.FlagId.IntValue >= GlobalStringTableSize && vstr.FlagType.StringValue == YksFormat.GlobalString)
						throw new ArgumentOutOfRangeException(nameof(vstr.FlagId.IntValue), vstr.FlagId.IntValue, "String flag index must be smaller than " + GlobalStringTableSize);
					if(vstr.FlagId.IntValue >= LocalStringTableSize && vstr.FlagType.StringValue == YksFormat.String)
						throw new ArgumentOutOfRangeException(nameof(vstr.FlagId.IntValue), vstr.FlagId.IntValue, "String flag index must be smaller than " + LocalStringTableSize);

					_currentAssignmentTarget = new AssignmentTarget.Variable(vstr.FlagType.StringValue, vstr.FlagId.IntValue);
					break;



				// local variables
				case DataElement.VLoc vloc:
					if(vloc.Id >= Script.InstructionList.MaxLocals)
						throw new ArgumentOutOfRangeException(nameof(vloc.Id), vloc.Id, "Local variable id must be smaller than local variable pool size (" + Script.InstructionList.MaxLocals + ")");
					_currentAssignmentTarget = new AssignmentTarget.Local(vloc.Id);
					break;



				// int pointers
				case DataElement.CInt cint:
					cint.Value.PointerId = _flagPointerId++;
					_currentAssignmentTarget = new AssignmentTarget.IntPointer(cint.Value.PointerId);
					break;




				default:
					throw new ArgumentOutOfRangeException(nameof(instruction), "Invalid assignment target: " + instruction);
			}
		}

		protected Expression ToExpression(DataElement element) {
			switch(element) {

				case DataElement.Ctrl ctrl:
					return new JumpLabelExpr { LabelStmt = new JumpLabelStmt { Name = ctrl.Name.StringValue.EscapeIdentifier() } };

				case DataElement.CInt cint when cint.Value.IsPointer:
					return new PointerLiteral { PointerId = cint.Value.PointerId };

				case DataElement.CInt cint:
					return new IntegerLiteral { Value = cint.Value.IntValue };

				case DataElement.CStr cstr:
					return new StringLiteral { Value = cstr.Value.StringValue };

				case DataElement.SStr sstr:
					return new Variable { VariableType = sstr.FlagType.StringValue };

				case DataElement.VInt vint when vint.FlagId.IsPointer:
					return new VariablePointer { VariableType = vint.FlagType.StringValue, PointerId = vint.FlagId.PointerId };

				case DataElement.VInt vint:
					return new Variable { VariableType = vint.FlagType.StringValue, VariableId = vint.FlagId.IntValue };

				case DataElement.VStr vstr when vstr.FlagId.IsPointer:
					return new VariablePointer { VariableType = vstr.FlagType.StringValue, PointerId = vstr.FlagId.PointerId };

				case DataElement.VStr vstr:
					return new Variable { VariableType = vstr.FlagType.StringValue, VariableId = vstr.FlagId.IntValue };

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

		protected Expression ToExpression(DataElement[] parts) {
			if(parts.Length == 1) return ToExpression(parts[0]);

			var operators = new List<string>();
			var operands = new List<Expression>();

			for(int i = 0; i < parts.Length; i++) {
				// operand
				if(parts[i] is DataElement.Ctrl ctrl) {
					i++;
					// create nested expression (0-x) to represent signed values -x
					operands.Add(new OperatorExpr {
						Operands = new[] { new IntegerLiteral { Value = 0 }, ToExpression(parts[i]) },
						Operators = new[] { ctrl.Name.StringValue }
					});
				}
				else {
					operands.Add(ToExpression(parts[i]));
				}

				// operator
				i++;
				if(i < parts.Length) {
					operators.Add(((DataElement.Ctrl)parts[i]).Name.StringValue);
				}
			}
			return new OperatorExpr { Operands = operands.ToArray(), Operators = operators.ToArray() };
		}

		protected Expression[] MapToExpressions(DataElement[] elements) {
			// not using linq here, because it's a performance critical function
			var expressions = new Expression[elements.Length];
			for(int i = 0; i < elements.Length; i++) {
				expressions[i] = ToExpression(elements[i]);
			}
			return expressions;
		}

		protected Statement ReadStatement() {
			var instruction = ReadInstruction();
			switch(instruction) {

				case LabelInstruction label:
					Debug.Assert(label.Name != "else" && label.Name != "}");
					if(label.Name == "{") {
						return ReadBlockStatement();
					}
					return new JumpLabelStmt { Name = label.Name.EscapeIdentifier() };

				case CallInstruction func:

					#region Handle assignments
					if(func.Name == "=") {
						if(_currentAssignmentTarget == null) throw new FormatException("No assignment target set");

						Expression expr;
						if(func.Arguments.Length == 0) {
							// a call to =() with no arguments means the result of the following function call is assigned
							if(CurrentInstruction is CallInstruction callInstruction) {
								expr = new FunctionCallExpr { CallStmt = new FunctionCallStmt { MethodName = callInstruction.Name, Arguments = MapToExpressions(callInstruction.Arguments) } };
								_currentInstructionOffset++;
							}
							else {
								throw new FormatException("A parameterless call to =() must be followed by a function call, found " + CurrentInstruction);
							}
						}
						else {
							// otherwise, the arguments are alternating operands and operators
							expr = ToExpression(func.Arguments);
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

					var callStatement = new FunctionCallStmt { MethodName = func.Name, Arguments = MapToExpressions(func.Arguments) };

					#region Handle body functions

					if(CurrentInstruction is LabelInstruction startLabel && startLabel.Name == "{") {
						_currentInstructionOffset++; // skip opening brace

						var body = ReadBlockStatement();

						#region Handle if statements

						if(callStatement.MethodName.ToLower() == "if") {
							if(Script.InstructionList.Count <= _currentInstructionOffset
							   || !(Script.InstructionList[_currentInstructionOffset] is LabelInstruction elseKeyword)
							   || elseKeyword.Name != "else")
								// no else block
								return new IfStmt { Function = callStatement, Body = body };

							BlockStmt elseBody;
							// skip else keyword
							_currentInstructionOffset++;
							if(Script.InstructionList[_currentInstructionOffset] is LabelInstruction elseStart && elseStart.Name == "{") {
								// skip opening brace
								_currentInstructionOffset++;
								elseBody = ReadBlockStatement();
							}
							else {
								// "Kimi o Aogi Otome wa Hime ni" has these bodyless else keywords in data01:system/selectjumpstart.yks
								Console.WriteLine("Warning: Loose else keyword without body");
								elseBody = new BlockStmt();
								//throw new FormatException("Else keyword must be followed by an opening brace");
							}

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
