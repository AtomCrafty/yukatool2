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

	/// <summary>
	/// Converts a syntax tree to an instruction list
	/// </summary>
	public class Compiler : ISyntaxVisitor {

		protected readonly YukaScript Script;
		protected InstructionList _instructions;
		protected DataSet _dataSet;

		public Compiler(YukaScript script) {
			Script = script;
		}

		public void Compile() {
			Debug.Assert(Script.IsDecompiled);

			_instructions = new InstructionList();
			_dataSet = new DataSet();

			foreach(var statement in Script.Body.Statements) {
				// emit instructions for this statement
				statement.Accept(this);

				// all locals should be freed at the end of each statement
				Debug.Assert(!_usedLocals.ContainsValue(true), "Local not freed after statement evaluation");
			}

			_instructions.MaxLocals = (uint)_usedLocals.Count;
			Script.InstructionList = _instructions;
			Script.Body = null;

			// TODO internalize string table
		}

		#region Visitor methods

		#region Expressions

		public DataElement Visit(FunctionCallExpr expr) {
			// evaluate all of the call arguments
			// this may emit instructions and allocate locals
			var arguments = EvaluateArguments(expr.CallStmt.Arguments);

			// insert local and =() right before the actual function call
			var local = AllocateLocal();
			Emit(new TargetInstruction(local, _instructions));
			EmitCall("=");

			// emit the call instruction
			EmitCall(expr.CallStmt.MethodName, arguments);

			// return the local the functions result is written to
			return local;
		}

		public DataElement Visit(IntegerLiteral expr) {
			return _dataSet.CreateIntConstant(expr.Value);
		}

		public DataElement Visit(PointerLiteral expr) {
			return _dataSet.CreateIntPointer(expr.PointerId);
		}

		public DataElement Visit(JumpLabelExpr expr) {
			return GetUniqueLabel(expr.LabelStmt.Name);
		}

		public DataElement Visit(OperatorExpr expr) {
			// evaluate all of the operands
			// this may emit instructions and allocate locals
			var operands = EvaluateArguments(expr.Operands);

			// insert local and the evaluation funcion =(...)
			var local = AllocateLocal();
			Emit(new TargetInstruction(local, _instructions));
			EmitCall("=", InterleaveOperandsAndOperators(operands, expr.Operators));

			// return the local the result is written to
			return local;
		}

		public DataElement Visit(StringLiteral expr) {
			return _dataSet.CreateStringConstant(expr.Value);
		}

		public DataElement Visit(Variable expr) {
			return _dataSet.CreateVariable(expr.VariableType, expr.VariableId);
		}

		public DataElement Visit(VariablePointer expr) {
			return _dataSet.CreateVariablePointer(expr.VariableType, expr.PointerId);
		}

		#endregion

		#region Statements

		public void Visit(AssignmentStmt stmt) {
			switch(stmt.Expression) {
				case FunctionCallExpr call:
					// evaluate all of the call arguments
					// this may emit instructions and allocate locals
					var arguments = EvaluateArguments(call.CallStmt.Arguments);

					// insert assignment target and =() right before the actual function call
					Emit(new TargetInstruction(CreateTargetElement(stmt.Target), _instructions));
					EmitCall("=");

					// emit the call instruction
					EmitCall(call.CallStmt.MethodName, arguments);
					break;
				case OperatorExpr expr:
					// evaluate all of the operands
					// this may emit instructions and allocate locals
					var operands = EvaluateArguments(expr.Operands);

					// insert assignment target and the evaluation funcion =(...)
					Emit(new TargetInstruction(CreateTargetElement(stmt.Target), _instructions));
					EmitCall("=", InterleaveOperandsAndOperators(operands, expr.Operators));
					break;
				default:
					Emit(new TargetInstruction(CreateTargetElement(stmt.Target), _instructions));
					EmitCall("=", stmt.Expression.Accept(this));
					break;
			}
		}

		public void Visit(BlockStmt stmt) {
			var start = EmitBlockStart();

			foreach(var statement in stmt.Statements) {
				// emit instructions for this statement
				statement.Accept(this);

				// all locals should be freed at the end of each statement
				Debug.Assert(!_usedLocals.ContainsValue(true), "Local not freed after statement evaluation");
			}

			EmitBlockEnd(start);
		}

		public void Visit(BodyFunctionStmt stmt) {
			stmt.Function.Accept(this);
			stmt.Body.Accept(this);
		}

		public void Visit(FunctionCallStmt stmt) {
			var arguments = EvaluateArguments(stmt.Arguments);
			EmitCall(stmt.MethodName, arguments);
		}

		public void Visit(IfStmt stmt) {
			stmt.Function.Accept(this);
			stmt.Body.Accept(this);

			if(stmt.ElseBody != null) {
				// else label remains unlinked
				EmitLabel("else");
				stmt.ElseBody.Accept(this);
			}
		}

		public void Visit(JumpLabelStmt stmt) {
			var label = GetUniqueLabel(stmt.Name);

			// label should not be linked yet
			Debug.Assert(label.LinkedElement == null, $"Duplicate unique label '{label.Name}'");

			// unique labels are linked to themselves
			label.LinkedElement = label;

			// emit label
			Emit(new LabelInstruction(label, _instructions));
		}

		#endregion

		#endregion

		#region Helper methods

		protected int Emit(Instruction element) {
			int index = _instructions.Count;
			_instructions.Add(element);
			return index;
		}

		#region DataElement

		protected DataElement CreateTargetElement(AssignmentTarget target) {
			switch(target) {

				case AssignmentTarget.Variable variable:
					return _dataSet.CreateVariable(variable.VariableType, variable.VariableId);

				case AssignmentTarget.VariablePointer pointer:
					return _dataSet.CreateVariablePointer(pointer.VariableType, pointer.PointerId);

				case AssignmentTarget.SpecialString sstr:
					return _dataSet.CreateVariable(sstr.Id, 0);

				case AssignmentTarget.Local local:
					throw new FormatException($"Unexpected local variable in assignment: {local.Id}");

				case AssignmentTarget.IntPointer pointer:
					return _dataSet.CreateIntPointer(pointer.PointerId);

				default:
					throw new FormatException($"Invalid assignment target: {target.GetType().Name}");
			}
		}

		protected TargetInstruction CreateTargetInstruction(AssignmentTarget target) {
			return new TargetInstruction(CreateTargetElement(target), _instructions);
		}

		#endregion

		#region Calls

		protected CallInstruction CreateCallInstruction(string name, DataElement[] arguments = null) {
			return new CallInstruction(_dataSet.CreateFunction(name), arguments ?? new DataElement[0], _instructions);
		}

		protected void EmitCall(string function, params DataElement[] arguments) {
			Emit(CreateCallInstruction(function, arguments));

			// free all locals used as arguments
			if(arguments == null) return;
			foreach(var argument in arguments) {
				if(argument is DataElement.VLoc local) FreeLocal(local);
			}
		}

		protected DataElement[] EvaluateArguments(Expression[] expressions) {
			return expressions.Select(expression => expression.Accept(this)).ToArray();
		}

		protected DataElement[] InterleaveOperandsAndOperators(DataElement[] operands, string[] operators) {
			Debug.Assert(operands.Length == operators.Length + 1, $"Operand / operator count mismatch: {operands.Length} != {operators.Length} + 1");

			var interleaved = new DataElement[operands.Length + operators.Length];
			interleaved[0] = operands[0];
			for(int i = 0; i < operators.Length; i++) {
				interleaved[i * 2 + 1] = CreateOperator(operators[i]);
				interleaved[i * 2 + 2] = operands[i + 1];
			}
			return interleaved;
		}

		#endregion

		#region Labels

		protected int _nextLabelId;
		protected DataElement.Ctrl CreateLabel(string name) {
			return new DataElement.Ctrl(_dataSet.CreateScriptValue(name)) { Id = _nextLabelId++ };
		}

		protected DataElement.Ctrl EmitLabel(string name) {
			var label = CreateLabel(name);
			Emit(new LabelInstruction(label, _instructions));
			return label;
		}

		protected Dictionary<string, DataElement.Ctrl> _uniqueLabels = new Dictionary<string, DataElement.Ctrl>();
		protected DataElement.Ctrl GetUniqueLabel(string name) {
			if(_uniqueLabels.ContainsKey(name)) return _uniqueLabels[name];
			return _uniqueLabels[name] = CreateLabel(name);
		}

		protected void LinkLabels(DataElement.Ctrl a, DataElement.Ctrl b) {
			a.LinkedElement = b;
			b.LinkedElement = a;
		}

		protected DataElement.Ctrl EmitBlockStart() {
			return EmitLabel("{");
		}
		protected void EmitBlockEnd(DataElement.Ctrl start) {
			var end = EmitLabel("}");
			LinkLabels(start, end);
		}

		protected Dictionary<string, DataElement.Ctrl> _usedOperators = new Dictionary<string, DataElement.Ctrl>();
		protected DataElement.Ctrl CreateOperator(string operation) {
			// try to re-use existing operator symbol
			if(_usedOperators.ContainsKey(operation)) return _usedOperators[operation];

			var op = new DataElement.Ctrl(_dataSet.CreateScriptValue(operation));
			_usedOperators[operation] = op;
			return op;
		}

		#endregion

		#region Locals

		protected Dictionary<DataElement.VLoc, bool> _usedLocals = new Dictionary<DataElement.VLoc, bool>();

		protected DataElement.VLoc AllocateLocal() {
			// find first free local or create a new one
			var local = _usedLocals.FirstOrDefault(pair => !pair.Value).Key
							?? new DataElement.VLoc((uint)_usedLocals.Count);

			// mark local as in use
			_usedLocals[local] = true;
			return local;
		}

		protected void FreeLocal(DataElement.VLoc local) {
			Debug.Assert(_usedLocals.ContainsKey(local) && _usedLocals[local]);
			_usedLocals[local] = false;
		}

		#endregion

		#endregion
	}

	public interface ISyntaxVisitor {
		DataElement Visit(FunctionCallExpr expr);
		DataElement Visit(IntegerLiteral expr);
		DataElement Visit(PointerLiteral expr);
		DataElement Visit(JumpLabelExpr expr);
		DataElement Visit(OperatorExpr expr);
		DataElement Visit(StringLiteral expr);
		DataElement Visit(Variable expr);
		DataElement Visit(VariablePointer expr);

		void Visit(AssignmentStmt stmt);
		void Visit(BlockStmt stmt);
		void Visit(BodyFunctionStmt stmt);
		void Visit(FunctionCallStmt stmt);
		void Visit(IfStmt stmt);
		void Visit(JumpLabelStmt stmt);
	}
}