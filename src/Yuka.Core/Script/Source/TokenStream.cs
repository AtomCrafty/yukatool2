using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Yuka.Script.Source {
	public class TokenStream : IEnumerable<Token>, IEnumerator<Token> {

		public readonly Lexer Lexer;
		public readonly List<Token> TokenCache = new List<Token>();
		public int CurrentTokenIndex { get; protected set; } = -1;

		public TokenStream(Lexer lexer) {
			Lexer = lexer;
		}

		public Token this[int index] => GetTokenAt(index);

		public Token GetTokenAt(int index) {
			while(TokenCache.Count <= index) {
				var token = Lexer.LexToken();
				// avoid multiple eof tokens
				if(TokenCache.Last().Kind != TokenKind.EndOfFile) {
					TokenCache.Add(token);
				}

				// stop lexing if end of file has been reached
				if(token.Kind == TokenKind.EndOfFile) return token;
			}

			return TokenCache[index];
		}

		public Token PeekNext() => LookAhead(1);

		public Token LookAhead(int tokens) => GetTokenAt(CurrentTokenIndex + tokens);

		public Token LookBack(int tokens) => GetTokenAt(CurrentTokenIndex - tokens);

		#region Interface implementation

		public bool MoveNext() {
			Debug.Assert(CurrentTokenIndex < TokenCache.Count, "CurrentTokenIndex should always be smaller than TokenCache.Count");

			if(CurrentTokenIndex == -1) {
				// lex initial element
				TokenCache.Add(Lexer.LexToken());
				CurrentTokenIndex = 0;
			}
			else {
				// Eof token was already lexed
				if(TokenCache[CurrentTokenIndex].Kind == TokenKind.EndOfFile) return false;

				// increment index and lex next token if necessary
				CurrentTokenIndex++;
				if(CurrentTokenIndex == TokenCache.Count) {
					var token = Lexer.LexToken();
					TokenCache.Add(token);
				}
			}

			// return false if new token is eof
			return TokenCache[CurrentTokenIndex].Kind != TokenKind.EndOfFile;
		}
		public void Reset() {
			CurrentTokenIndex = 0;
		}

		public Token Current => TokenCache[CurrentTokenIndex];
		object IEnumerator.Current => ((IEnumerator<Token>)this).Current;
		IEnumerator<Token> IEnumerable<Token>.GetEnumerator() => this;
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Token>)this).GetEnumerator();

		#endregion

		public void Dispose() {
			Lexer.Dispose();
		}
	}
}
