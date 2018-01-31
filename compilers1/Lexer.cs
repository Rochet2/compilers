using System;
using System.Collections.Generic;
using System.Text;

namespace compilers1
{
	class LexEx : Exception
	{
		public LexEx (string message) : base (message)
		{
		}
	}

	public class Lexer
	{
		public Lexer(string input)
		{
			this.input = input;
			this.pos = 0;
		}

		public static List<string> keywords = new List<string> {
			"var",
			"for",
			"end",
			"in",
			"do",
			"read",
			"print",
			"int",
			"string",
			"bool",
			"assert",
		};

		public int find (string input, string s, int pos = 0)
		{
			return input.IndexOf (s, Math.Min (pos, input.Length - 1));
		}

		public string sub (string input, int start, int end)
		{
			end = Math.Min (end, input.Length);
			if (start >= end)
				return "";
			return input.Substring (start, end - start);
		}

		public int findend (int pos, string start, string end, string input)
		{
			// what does nested mean?..
			// /* /* */ allowed?
			int i = pos + start.Length;
			int j = 0;
			for (; i < input.Length; ++i) {
				if (input [i] == end [j])
					++j;
				if (j >= end.Length)
					return i;
			}
			return -1;
		}

		// %d+
		public void expectnumber ()
		{
			int i = pos;
			if (!Char.IsDigit (input [i]))
				throw new LexEx("number expected");
			for (; i < input.Length; ++i)
				if (!Char.IsDigit (input [i]))
					break;
			string s = input.Substring (pos, i - pos);
			lexed.Add (new Lexeme (Type.NUMBER, pos, s));
			pos = i - 1;
		}

		public void expectstring ()
		{
			int start = pos + 1; // skip first quote
			int i = start;
			for (; i < input.Length; ++i) {
				if (input [i] == '\\') {
					++i;
					continue;
				}
				if (input [i] == '"')
					break;
				if (i == input.Length - 1)
					throw new LexEx ("unexpected end of a string");
			}
			string s = input.Substring (start, i - start);
			s = s.Replace ("\\\\", "\\").Replace ("\\\"", "\"");
			lexed.Add (new Lexeme (Type.STRING, pos, s, i));
			pos = i;
		}

		public void expectcomment ()
		{
			int start = pos + 2; // skip comment start
			int end = start;
			for (; end < input.Length; ++end)
				if (input [end] == '\n')
					break;
			string s = input.Substring (start, end - start);
			lexed.Add (new Lexeme (Type.COMMENT, pos, s, end));
			pos = end;
		}

		public void expectblockcomment ()
		{
			int start = pos + 2; // skip comment start
			if (start == input.Length - 1)
				throw new LexEx ("unexpected end of a blockcomment");

			int end = input.IndexOf ("*/", start);
			int substart = input.IndexOf ("/*", start);
			while (true) {
				if (end < start) // cannot find end
					throw new LexEx ("Unexpected end of blockcomment");
				if (substart < start || end < substart)
					break; // found end
				// have nested blockcomment, find again
				end = input.IndexOf ("*/", end + 1);
				substart = input.IndexOf ("/*", substart + 1);
			}
			string s = input.Substring (start, end - start);
			lexed.Add (new Lexeme (Type.BLOCKCOMMENT, pos, s, end + 1));
			pos = end + 1;
		}

		public void expectidentifierorkeyword ()
		{
			int start = pos;
			if (!Char.IsLetter (input [start]))
				throw new LexEx ("Identifier must start with a letter");
			int end = start;
			for (; end < input.Length; ++end) {
				char c = input [end];
				if (!(Char.IsLetterOrDigit (c) || c == '_'))
					break;
			}
			string s = input.Substring (start, end - start);
			lexed.Add (new Lexeme (keywords.Contains (s) ? Type.KEYWORD : Type.IDENTIFIER, pos, s));
			pos = end - 1;
		}

		public void lexnext()
		{
			char current = input [pos];
			char next = input [pos + 1];

			if (current == '/' && next == '/') {
				expectcomment ();
			} else if (current == '/' && next == '*') {
				expectblockcomment ();
			} else if (Char.IsLetter (current)) {
				expectidentifierorkeyword ();
			} else if (
				current == ':' && next == '=') {
				lexed.Add (new Lexeme (Type.SEPARATOR, pos, current.ToString () + next.ToString ()));
				++pos;
			} else if (
				current == '.' && next == '.') {
				lexed.Add (new Lexeme (Type.SEPARATOR, pos, current.ToString () + next.ToString ()));
				++pos;
			} else if (
				current == ':' ||
				current == '(' ||
				current == ')' ||
				current == ';') {
				lexed.Add (new Lexeme (Type.SEPARATOR, pos, current.ToString ()));
			} else if (
				current == '<' ||
				current == '=' ||
				current == '!' ||
				current == '&' ||
				current == '+' ||
				current == '-' ||
				current == '/' ||
				current == '*') {
				lexed.Add (new Lexeme (Type.OPERATOR, pos, current.ToString ()));
			} else if (Char.IsDigit (current)) {
				expectnumber ();
			} else if (current == '"') {
				expectstring ();
			} else if (Char.IsWhiteSpace (current)) {
				// skip
			} else {
				throw new LexEx (String.Format ("Unrecognized tokens {0}{1} at index {2}", current, next, pos));
			}
			++pos;
		}

		public void lexall ()
		{
			while (pos >= 0 && pos + 1 < input.Length) {
				// Console.Out.WriteLine (i); // debug
				lexnext();
			}
		}

		public bool hasnext()
		{
			return lexed.Count > 0;
		}

		public Lexeme next()
		{
			if (!hasnext ())
				lexnext ();
			if (!hasnext ())
				return null;
			Lexeme l = lexed[0];
			lexed.RemoveAt(0);
			return l;
		}

		public List<Lexeme> lexed = new List<Lexeme> ();
		int pos;
		string input;
	}
}

