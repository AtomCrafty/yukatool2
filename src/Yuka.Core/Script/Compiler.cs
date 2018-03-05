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
	public class Compiler : ISyntaxVisitor {

		protected readonly YukaScript Script;
		protected InstructionList _instructions;

		public Compiler(YukaScript script) {
			Script = script;
		}

		public void Compile() {
			Debug.Assert(Script.IsDecompiled);

			_instructions = new InstructionList();

			foreach(var statement in Script.Body.Statements) {
				// emit instructions for this statement
				statement.Accept(this);

				// all locals should be freed at the end of each statement
				Debug.Assert(!_usedLocals.ContainsValue(true), "Local not freed after statement evaluation");
			}

			_instructions.MaxLocals = (uint)_usedLocals.Count;
			Script.InstructionList = _instructions;
			Script.Body = null;
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

		public DataElement Visit(IntLiteral expr) {
			return CreateIntConstant(expr.Value);
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
			return CreateStringConstant(expr.Value);
		}

		public DataElement Visit(VariableExpr expr) {
			switch(expr.FlagType) {
				case "Flag":
				case "GlobalFlag":
					return CreateIntVariable(expr.FlagType, expr.FlagId);
				case "String":
				case "GlobalString":
					return CreateStringVariable(expr.FlagType, expr.FlagId);
				default:
					return CreateSpecialString(expr.FlagType);
			}
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
					EmitCall("=", new[] { stmt.Expression.Accept(this) });
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

		// TODO use DataSet instead

		#region DataElement

		protected Dictionary<(DataElementType type, object value), DataElement> _usedElements = new Dictionary<(DataElementType type, object value), DataElement>();

		protected TElem GetElement<TElem>(DataElementType type, object value, Func<TElem> producer) where TElem : DataElement {
			// try to re-use existing data element
			if(_usedElements.ContainsKey((type, value)) && _usedElements[(type, value)] is TElem existingElement) {
				return existingElement;
			}

			// if none was found, create a new one
			var newElement = producer();
			_usedElements[(type, value)] = newElement;
			return newElement;
		}

		protected DataElement.Func CreateFunction(string name) {
			return GetElement(DataElementType.Func, name, () => new DataElement.Func(name));
		}

		protected DataElement.CInt CreateIntConstant(int value) {
			return GetElement(DataElementType.CInt, value, () => new DataElement.CInt(value));
		}

		protected DataElement.CStr CreateStringConstant(string value) {
			return GetElement(DataElementType.CStr, value, () => new DataElement.CStr(value));
		}

		protected DataElement.VInt CreateIntVariable(string type, int id) {
			return GetElement(DataElementType.VInt, (type, id), () => new DataElement.VInt(type, id));
		}

		protected DataElement.VStr CreateStringVariable(string type, int id) {
			return GetElement(DataElementType.VStr, (type, id), () => new DataElement.VStr(type, id));
		}

		protected DataElement.SStr CreateSpecialString(string type) {
			return GetElement(DataElementType.SStr, type, () => new DataElement.SStr(type));
		}

		protected DataElement CreateTargetElement(AssignmentTarget target) {
			switch(target) {
				case AssignmentTarget.GlobalFlag gflg:
					return CreateStringVariable("GlobalFlag", gflg.Id);
				case AssignmentTarget.GlobalString gstr:
					return CreateStringVariable("GlobalString", gstr.Id);
				case AssignmentTarget.Flag lflg:
					return CreateStringVariable("Flag", lflg.Id);
				case AssignmentTarget.String lstr:
					return CreateStringVariable("String", lstr.Id);
				case AssignmentTarget.SpecialString sstr:
					return CreateSpecialString(sstr.Id);
				case AssignmentTarget.Local local:
					throw new FormatException($"Unexpected local variable in assignment: {local.Id}");
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
			return new CallInstruction(CreateFunction(name), arguments ?? new DataElement[0], _instructions);
		}

		protected void EmitCall(string function, DataElement[] arguments = null) {
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
			return new DataElement.Ctrl(name) { Id = _nextLabelId++ };
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

			var op = new DataElement.Ctrl(operation);
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
		DataElement Visit(IntLiteral expr);
		DataElement Visit(JumpLabelExpr expr);
		DataElement Visit(OperatorExpr expr);
		DataElement Visit(StringLiteral expr);
		DataElement Visit(VariableExpr expr);

		void Visit(AssignmentStmt stmt);
		void Visit(BlockStmt stmt);
		void Visit(BodyFunctionStmt stmt);
		void Visit(FunctionCallStmt stmt);
		void Visit(IfStmt stmt);
		void Visit(JumpLabelStmt stmt);
	}
}