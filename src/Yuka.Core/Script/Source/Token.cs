namespace Yuka.Script.Source {

	public class Token {
		public static readonly Token EndOfFile = new Token(TokenKind.EndOfFile, "", SourceRange.Invalid);

		public readonly TokenKind Kind;
		public readonly string Source;
		public readonly SourceRange Range;
		public SourcePosition Start => Range?.Start;
		public SourcePosition End => Range?.End;

		public Token(TokenKind kind, string source, SourceRange range) {
			Kind = kind;
			Source = source;
			Range = range;
		}
	}

	public class SourcePosition {
		public static SourcePosition Invalid => new SourcePosition(-1, -1, -1, "<invalid>");

		public readonly int Index, Line, Column;
		public readonly string FileName;

		public SourcePosition(int index, int line, int column = -1, string fileName = null) {
			Index = index;
			Line = line;
			Column = column;
			FileName = fileName;
		}

		public string InLineColumnOfFile() {
			return $"in line {Line}, column {Column} of {FileName ?? "<unknown file>"}";
		}
	}

	public class SourceRange {
		public static SourceRange Invalid => new SourceRange(SourcePosition.Invalid, SourcePosition.Invalid);

		public readonly SourcePosition Start, End;

		public SourceRange(SourcePosition start, SourcePosition end) {
			Start = start;
			End = end;
		}
	}

	public enum TokenKind {
		Identifier,         // [\w_][\w\d_]*
		StringLiteral,      // "(\\.|[^"\\])*"
		IntegerLiteral,     // -?\d+
		LabelLiteral,       // :[^\s]+

		Dollar,             // $
		Ampersand,          // &
		At,                 // @
		Comma,              // ,
		Semicolon,          // ;
		OpenParen,          // (
		CloseParen,         // )
		OpenBrace,          // {
		CloseBrace,         // }

		Colon,              // :
		Assign,             // =
		Operator,           // + - * / > < ==

		Directive,          // !<Identifier>
		EndOfFile
	}
}
