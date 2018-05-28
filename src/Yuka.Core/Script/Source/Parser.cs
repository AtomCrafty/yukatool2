using System;
using System.Collections.Generic;
using System.Diagnostics;
using Yuka.IO.Formats;
using Yuka.Script.Data;
using Yuka.Script.Syntax;
using Yuka.Script.Syntax.Expr;
using Yuka.Script.Syntax.Stmt;
using Yuka.Util;

namespace Yuka.Script.Source {
	public class Parser {

		protected readonly string FileName;
		protected readonly TokenStream TokenStream;
		public readonly Dictionary<string, Expression> DefinedVariables = new Dictionary<string, Expression>();
		public readonly StringTable StringTable;

		public Parser(string fileName, StringTable stringTable, Lexer lexer) {
			FileName = fileName;
			StringTable = stringTable;
			TokenStream = new TokenStream(lexer);

			// move read head to the first token
			TokenStream.MoveNext();
		}

		#region Parser methods

		public YukaScript ParseScript() {
			ParseDirectives();

			var statements = ParseStatementList();

			ConsumeToken(TokenKind.EndOfFile);

			return new YukaScript(FileName, new BlockStmt {
				Statements = statements
			}) { Strings = StringTable };
		}

		public void ParseDirectives() {
			// TODO parse directives
		}

		#region Statements

		public static bool IsPossibleStatementStart(TokenKind kind) {
			return kind.IsOneOf(TokenKind.Identifier, TokenKind.LabelLiteral, TokenKind.Dollar, TokenKind.Ampersand, TokenKind.OpenBrace);
		}

		public Statement ParseStatement() {
			switch(CurrentToken.Kind) {

				case TokenKind.Dollar:      // $local = ...;
				case TokenKind.Ampersand:   // &pointer = ...;
				case TokenKind.Identifier when NextToken.Kind == TokenKind.Colon: // Type:id = ... or Type:&pointer = ...;
					return ParseAssignmentStatement();

				case TokenKind.Identifier when NextToken.Kind == TokenKind.OpenParen: // Method( ... );
					return ParseFunctionCallStatement();

				case TokenKind.Identifier: // Neither one of the above identifier cases were matched
					throw new UnexpectedTokenException(NextToken, TokenKind.Colon, TokenKind.OpenParen);

				case TokenKind.LabelLiteral: // :LABEL
					return ParseJumpLabelStatement();

				case TokenKind.OpenBrace: // { ... }
					return ParseBlockStatement();

				default:
					throw new UnexpectedTokenException(CurrentToken, TokenKind.Identifier, TokenKind.LabelLiteral, TokenKind.Dollar, TokenKind.Ampersand, TokenKind.OpenBrace);
			}
		}

		public List<Statement> ParseStatementList() {
			var list = new List<Statement>();

			while(IsPossibleStatementStart(CurrentToken.Kind)) {
				list.Add(ParseStatement());
			}

			return list;
		}

		public BlockStmt ParseBlockStatement() {
			ConsumeToken(TokenKind.OpenBrace);
			var statements = ParseStatementList();
			ConsumeToken(TokenKind.CloseBrace);

			return new BlockStmt { Statements = statements };
		}

		public JumpLabelStmt ParseJumpLabelStatement() {
			return new JumpLabelStmt {
				Name = ConsumeToken(TokenKind.LabelLiteral).Source.TrimStart(':').EscapeIdentifier()
			};
		}

		public Statement ParseFunctionCallStatement() {
			string function = ConsumeToken(TokenKind.Identifier).Source;

			ConsumeToken(TokenKind.OpenParen);

			var arguments = ParseArgumentList();

			ConsumeToken(TokenKind.CloseParen);

			var call = new FunctionCallStmt {
				MethodName = function,
				Arguments = arguments.ToArray()
			};

			switch(CurrentToken.Kind) {

				case TokenKind.Semicolon:
					ConsumeToken();
					break;

				case TokenKind.OpenBrace:
					if(function.Equals("if", StringComparison.CurrentCultureIgnoreCase)) {

						var body = ParseBlockStatement();
						BlockStmt elseBody = null;

						if(CurrentToken.Kind == TokenKind.Identifier && CurrentToken.Source.Equals("else", StringComparison.CurrentCultureIgnoreCase)) {
							ConsumeToken();
							elseBody = ParseBlockStatement();
						}

						return new IfStmt {
							Function = call,
							Body = body,
							ElseBody = elseBody
						};
					}

					return new BodyFunctionStmt {
						Function = call,
						Body = ParseBlockStatement()
					};

				default:
					throw new UnexpectedTokenException(CurrentToken, TokenKind.Semicolon, TokenKind.OpenBrace);
			}

			return call;
		}

		public AssignmentStmt ParseAssignmentStatement() {
			var target = ParseAssignmentTarget();

			ConsumeToken(TokenKind.Assign);

			var expression = ParseExpression();

			ConsumeToken(TokenKind.Semicolon);

			return new AssignmentStmt {
				Target = target,
				Expression = expression
			};
		}

		public AssignmentTarget ParseAssignmentTarget() {
			var start = CurrentToken.Start;
			var expression = ParseExpression();

			switch(expression) {
				case PointerLiteral pointer:
					return new AssignmentTarget.IntPointer(pointer.PointerId);

				case Variable variable when variable.VariableType.IsOneOf(YksFormat.TempGlobalString, YksFormat.主人公, YksFormat.汎用文字変数):
					return new AssignmentTarget.SpecialString(variable.VariableType);

				case Variable variable:
					return new AssignmentTarget.Variable(variable.VariableType, variable.VariableId);

				case VariablePointer pointer:
					return new AssignmentTarget.VariablePointer(pointer.VariableType, pointer.PointerId);

				default:
					throw new FormatException($"Invalid assignment target: '{expression}' {start.InLineColumnOfFile()}");
			}
		}

		#endregion

		#region Expressions

		public static bool IsPossibleExpressionStart(TokenKind kind) {
			switch(kind) {
				case TokenKind.Identifier:
				case TokenKind.StringLiteral:
				case TokenKind.IntegerLiteral:
				case TokenKind.LabelLiteral:
				case TokenKind.Dollar:
				case TokenKind.Ampersand:
				case TokenKind.At:
				case TokenKind.OpenParen:
				case TokenKind.Operator:
					return true;
				default:
					return false;
			}
		}

		public Expression ParseExpression() {
			if(CurrentToken.Kind == TokenKind.LabelLiteral) {
				return ParseLabelLiteral();
			}

			var expression = ParsePrimaryExpression();

			while(CurrentToken.Kind == TokenKind.Operator) {
				expression = ParseOperatorExpression(expression);
			}

			return expression;
		}

		public List<Expression> ParseArgumentList() {
			var list = new List<Expression>();

			if(!IsPossibleExpressionStart(CurrentToken.Kind)) return list;

			// first expression
			list.Add(ParseExpression());

			while(CurrentToken.Kind == TokenKind.Comma) {
				ConsumeToken(); // consume comma
				list.Add(ParseExpression());
			}

			return list;
		}

		public Expression ParsePrimaryExpression() {
			switch(CurrentToken.Kind) {

				case TokenKind.StringLiteral:
					return ParseStringLiteral();

				case TokenKind.Operator: // signed int literals
				case TokenKind.IntegerLiteral:
					return ParseIntegerLiteral();

				case TokenKind.Identifier when NextToken.Kind == TokenKind.Colon:
				case TokenKind.Dollar:
					return ParseVariableExpression();

				case TokenKind.Identifier when NextToken.Kind == TokenKind.OpenParen:
					return ParseFunctionCallExpression();

				case TokenKind.Identifier:
					throw new UnexpectedTokenException(NextToken, TokenKind.Colon, TokenKind.OpenParen);

				case TokenKind.Ampersand:
					return ParsePointerLiteral();

				case TokenKind.At:
					return ParseExternalStringLiteral();

				case TokenKind.OpenParen:
					return ParseNestedExpression();

				default:
					throw new UnexpectedTokenException(CurrentToken, TokenKind.StringLiteral, TokenKind.IntegerLiteral, TokenKind.Identifier, TokenKind.Dollar, TokenKind.Ampersand, TokenKind.At, TokenKind.OpenParen);
			}
		}

		public Expression ParseVariableExpression() {
			switch(CurrentToken.Kind) {

				case TokenKind.Identifier:
					string varType = ConsumeToken().Source;
					ConsumeToken(TokenKind.Colon);

					switch(varType) {

						case YksFormat.TempGlobalString:
						case YksFormat.主人公:
						case YksFormat.汎用文字変数:
							return new Variable { VariableType = varType };

						default:
							switch(CurrentToken.Kind) {

								// variable pointers
								case TokenKind.Ampersand:
									ConsumeToken(); // consume &
									int pointerId = int.Parse(ConsumeToken().Source);
									return new VariablePointer() {
										VariableType = varType,
										PointerId = pointerId
									};

								// variables
								case TokenKind.IntegerLiteral:
									int varId = int.Parse(ConsumeToken().Source);
									return new Variable {
										VariableType = varType,
										VariableId = varId
									};

								default:
									throw new UnexpectedTokenException(CurrentToken, TokenKind.IntegerLiteral, TokenKind.Ampersand);
							}
					}

				case TokenKind.Dollar:
					ConsumeToken(); // consume $
					string varName = ConsumeToken(TokenKind.Identifier).Source;
					if(!DefinedVariables.ContainsKey(varName))
						throw new ArgumentException($"Use of undefined variable '{varName}' {TokenStream.LookBack(1).Start.InLineColumnOfFile()}");
					return DefinedVariables[varName];

				default:
					throw new UnexpectedTokenException(CurrentToken, TokenKind.Identifier, TokenKind.Dollar);
			}
		}

		public Expression ParseNestedExpression() {
			ConsumeToken(TokenKind.OpenParen);

			var expression = ParseExpression();

			ConsumeToken(TokenKind.CloseParen);

			return expression;
		}

		public FunctionCallExpr ParseFunctionCallExpression() {
			string function = ConsumeToken(TokenKind.Identifier).Source;

			ConsumeToken(TokenKind.OpenParen);

			var arguments = ParseArgumentList();

			ConsumeToken(TokenKind.CloseParen);

			return new FunctionCallExpr {
				CallStmt = new FunctionCallStmt {
					MethodName = function,
					Arguments = arguments.ToArray()
				}
			};
		}

		public OperatorExpr ParseOperatorExpression(Expression left) {
			var operands = new List<Expression> { left };
			var operators = new List<string>();

			while(CurrentToken.Kind == TokenKind.Operator) {
				string op = ConsumeToken().Source;
				operators.Add(op == "==" ? "=" : op);
				operands.Add(ParsePrimaryExpression());
			}

			return new OperatorExpr {
				Operands = operands.ToArray(),
				Operators = operators.ToArray()
			};
		}

		#region Literals

		public JumpLabelExpr ParseLabelLiteral() {
			return new JumpLabelExpr { LabelStmt = ParseJumpLabelStatement() };
		}

		public StringLiteral ParseStringLiteral() {
			string value = ConsumeToken(TokenKind.StringLiteral).Source;

			// trim quotation marks
			value = value.Substring(1, value.Length - 2);

			// resolve escape sequences
			value = value.Unescape();

			return new StringLiteral {
				Value = value
			};
		}

		public IntegerLiteral ParseIntegerLiteral() {
			int value = 1;

			// process optional sign
			if(CurrentToken.Kind == TokenKind.Operator) {
				switch(CurrentToken.Source) {
					case "+":
						ConsumeToken();
						break;
					case "-":
						ConsumeToken();
						value = -1;
						break;
					default:
						throw new UnexpectedTokenException(CurrentToken, TokenKind.IntegerLiteral);
				}
			}

			value *= int.Parse(ConsumeToken(TokenKind.IntegerLiteral).Source);

			return new IntegerLiteral { Value = value };
		}

		public PointerLiteral ParsePointerLiteral() {
			ConsumeToken(TokenKind.Ampersand);

			int pointerId = int.Parse(ConsumeToken(TokenKind.IntegerLiteral).Source);

			return new PointerLiteral { PointerId = pointerId };
		}

		public StringLiteral ParseExternalStringLiteral() {
			Debug.Assert(StringTable != null, "StringTable != null");

			ConsumeToken(TokenKind.At);
			string stringId = ConsumeToken(TokenKind.Identifier).Source;

			return new StringLiteral {
				StringTable = StringTable,
				ExternalKey = stringId
			};
		}

		#endregion

		#endregion

		#endregion

		#region Tokens

		protected Token CurrentToken => TokenStream.Current;
		protected Token NextToken => TokenStream.PeekNext();

		protected Token ConsumeToken(params TokenKind[] kind) {
			var token = TokenStream.Current;
			Debug.Assert(token != null, nameof(token) + " != null");

			// no kind passed -> allow everything
			if(kind.Length > 0 && !token.Kind.IsOneOf(kind))
				throw new UnexpectedTokenException(token, kind);

			TokenStream.MoveNext();
			return token;
		}

		#endregion
	}
}
