using System;
using Yuka.Script.Data;
using Yuka.Script.Syntax.Expr;
using Yuka.Script.Syntax.Stmt;

namespace Yuka.Script.Syntax {
	public interface ISyntaxVisitor<T> {
		T Visit(FunctionCallExpr expr);
		T Visit(IntegerLiteral expr);
		T Visit(PointerLiteral expr);
		T Visit(JumpLabelExpr expr);
		T Visit(OperatorExpr expr);
		T Visit(StringLiteral expr);
		T Visit(Variable expr);
		T Visit(VariablePointer expr);

		void Visit(AssignmentStmt stmt);
		void Visit(BlockStmt stmt);
		void Visit(BodyFunctionStmt stmt);
		void Visit(FunctionCallStmt stmt);
		void Visit(IfStmt stmt);
		void Visit(JumpLabelStmt stmt);
	}

	public abstract class NodeVisitor : ISyntaxVisitor<object> {
		public virtual object Visit(FunctionCallExpr expr) {
			expr.CallStmt.Accept(this);
			return null;
		}

		public virtual object Visit(IntegerLiteral expr) {
			return null;
		}

		public virtual object Visit(PointerLiteral expr) {
			return null;
		}

		public virtual object Visit(JumpLabelExpr expr) {
			expr.LabelStmt.Accept(this);
			return null;
		}

		public virtual object Visit(OperatorExpr expr) {
			foreach(var operand in expr.Operands) {
				operand.Accept(this);
			}
			return null;
		}

		public virtual object Visit(StringLiteral expr) {
			return null;
		}

		public virtual object Visit(Variable expr) {
			return null;
		}

		public virtual object Visit(VariablePointer expr) {
			return null;
		}

		public virtual void Visit(AssignmentStmt stmt) {
			stmt.Expression.Accept(this);
		}

		public virtual void Visit(BlockStmt stmt) {
			foreach(var statement in stmt.Statements) {
				statement.Accept(this);
			}
		}

		public virtual void Visit(BodyFunctionStmt stmt) {
			stmt.Function.Accept(this);
			stmt.Body.Accept(this);
		}

		public virtual void Visit(FunctionCallStmt stmt) {
			foreach(var argument in stmt.Arguments) {
				argument.Accept(this);
			}
		}

		public virtual void Visit(IfStmt stmt) {
			stmt.Function.Accept(this);
			stmt.Body.Accept(this);
			stmt.ElseBody?.Accept(this);
		}

		public virtual void Visit(JumpLabelStmt stmt) {
		}
	}
}