using System;
using Yuka.Script.Syntax;

namespace Yuka.Script.Source {
	public class UnexpectedTokenException : Exception {

		public readonly Token Found;
		public readonly TokenKind[] Expected;

		public UnexpectedTokenException(Token found, params TokenKind[] expected) {
			Found = found;
			Expected = expected;
		}

		public override string Message => $"Unexpected {Found.Kind} {Found.Start.InLineColumnOfFile()}.\n  Expected one of the following: {string.Join(", ", Expected)}";
	}
}
