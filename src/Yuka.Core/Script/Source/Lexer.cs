using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Yuka.Util;

namespace Yuka.Script.Source {
	public class Lexer : IDisposable {

		protected static readonly char[] OperatorChars = { '+', '-', '*', '/', '=', '<', '>' };
		protected static readonly Dictionary<char, TokenKind> SingleCharTokens = new Dictionary<char, TokenKind> {
			{ '$', TokenKind.Dollar       },
			{ '&', TokenKind.Ampersand    },
			{ '@', TokenKind.At           },
			{ ',', TokenKind.Comma        },
			{ ';', TokenKind.Semicolon    },
			{ '(', TokenKind.OpenParen    },
			{ ')', TokenKind.CloseParen   },
			{ '{', TokenKind.OpenBrace  },
			{ '}', TokenKind.CloseBrace }
		};

		public readonly string FileName;
		protected readonly TextReader Reader;
		protected int _currentIndex, _currentLine, _currentColumn;
		public SourcePosition CurrentPosition => new SourcePosition(_currentIndex, _currentLine, _currentColumn, FileName);

		protected readonly StringBuilder TokenBuilder = new StringBuilder();
		protected SourcePosition _currentTokenStart;

		public Lexer(TextReader reader, string fileName = null) {
			Reader = reader;
			FileName = fileName;
		}

		public Token LexToken() {
			SkipWhiteSpaceAndComments();

			if(CurrentChar == null) return Token.EndOfFile;

			char ch = CurrentChar.Value;
			var type = TypeOf(ch);

			StartToken();

			switch(type) {

				// identifier
				case CharType.Letter:
					ConsumeIdentifier();
					return FinishToken(TokenKind.Identifier);

				// integer literal
				case CharType.Digit:
					ConsumeIntegerLiteral();
					return FinishToken(TokenKind.IntegerLiteral);

				case CharType.Symbol:

					switch(ch) {

						// string literal
						case '"':
							ConsumeStringLiteral();
							return FinishToken(TokenKind.StringLiteral);

						// label literal
						case ':':
							ConsumeChar();

							if(TypeOf(CurrentChar) != CharType.Letter) {
								return FinishToken(TokenKind.Colon);
							}

							// next char is a letter -> this must be a label literal
							ConsumeIdentifier();
							return FinishToken(TokenKind.LabelLiteral);

						// compiler directive
						case '!':
							ConsumeChar();
							ConsumeIdentifier();
							return FinishToken(TokenKind.Directive);

						default:

							// operators
							if(ch.IsOneOf(OperatorChars)) {
								ConsumeChar();

								if(ch == '=') {
									// equality check has two equals signs
									if(CurrentChar == '=') ConsumeChar();

									// assignment operator has a different token kind
									else return FinishToken(TokenKind.Assign);
								}

								return FinishToken(TokenKind.Operator);
							}

							// $ , ; ( ) { }
							if(SingleCharTokens.ContainsKey(ch)) {
								ConsumeChar();
								return FinishToken(SingleCharTokens[ch]);
							}

							throw new FormatException($"Unexpected char '{ch}' of type {type} {CurrentPosition.InLineColumnOfFile()}");
					}

				default:
					throw new FormatException($"Unexpected char '{ch}' of type {type} {CurrentPosition.InLineColumnOfFile()}");
			}
		}

		protected void ConsumeIdentifier() {
			while(TypeOf(CurrentChar).IsOneOf(CharType.Letter, CharType.Digit)) ConsumeChar();
		}

		protected void ConsumeIntegerLiteral() {
			while(TypeOf(CurrentChar) == CharType.Digit) ConsumeChar();
		}

		protected void ConsumeStringLiteral() {
			var ch = ConsumeChar();
			Debug.Assert(ch == '"', "String literal must start with a quotation mark");

			while(true) {
				ch = ConsumeChar();

				switch(ch) {

					case null: throw new FormatException($"Unterminated string literal {CurrentPosition.InLineColumnOfFile()}");

					case '\\':
						// consume another character after the backslash
						ConsumeChar();
						break;

					case '"': return;
				}
			}
		}

		#region Token building

		protected void StartToken() {
			TokenBuilder.Clear();
			_currentTokenStart = CurrentPosition;
		}

		protected Token FinishToken(TokenKind kind) {
			return new Token(kind, TokenBuilder.ToString(), new SourceRange(_currentTokenStart, CurrentPosition));
		}

		#endregion

		#region Skipping

		protected void SkipWhiteSpaceAndComments() {
			SkipWhiteSpace();
			while(CurrentChar == '#') {
				SkipUntil('\n');
				SkipWhiteSpace();
			}
		}

		protected void SkipUntil(char ch) {
			while(CurrentChar != ch && CurrentChar != null) ConsumeChar();
		}

		protected void SkipWhiteSpace() {
			while(TypeOf(CurrentChar) == CharType.WhiteSpace) ConsumeChar();
		}

		#endregion

		#region Character operations

		protected char? CurrentChar {
			get {
				int ch = Reader.Peek();
				return ch != -1 ? (char?)ch : null;
			}
		}

		protected char? ConsumeChar() {
			int ch = Reader.Read();

			if(ch == -1) return null;

			TokenBuilder.Append((char)ch);
			_currentIndex++;
			_currentColumn++;

			if(ch == '\n') {
				_currentColumn = 0;
				_currentLine++;
			}
			else if(ch == '\t') {
				const int tabWidth = 4;
				_currentColumn += (tabWidth - _currentColumn % tabWidth) % tabWidth;
			}

			return (char?)ch;
		}

		protected static CharType TypeOf(char? c) {
			if(c == null) return CharType.Eof;

			char ch = c.Value;
			if(char.IsWhiteSpace(ch)) return CharType.WhiteSpace;
			if(char.IsLetter(ch) || ch == '_') return CharType.Letter;
			if(char.IsDigit(ch)) return CharType.Digit;
			return CharType.Symbol;
		}

		protected enum CharType {
			WhiteSpace,
			Letter,
			Digit,
			Symbol,
			Eof
		}

		#endregion

		public void Dispose() {
			Reader?.Dispose();
		}
	}
}
