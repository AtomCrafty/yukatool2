namespace Yuka.Script.Syntax {

	public class Token {
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
		At,                 // @
		Comma,              // ,
		Colon,              // :
		Semicolon,          // ;
		OpenParen,          // (
		CloseParen,         // )
		OpenBracket,        // {
		CloseBracket,       // }
		Assign,             // =
		Operator,           // + - * / > < ==

		Directive,          // !<Identifier>
	}
}
