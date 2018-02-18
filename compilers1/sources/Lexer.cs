using System;
using System.Collections.Generic;
using System.Text;

namespace compilers1
{
	public enum TokenType
	{
		KEYWORD,
		IDENTIFIER,
		SEPARATOR,
		OPERATOR,
		NUMBER,
		STRING,
		BOOLEAN,
		COMMENT,
		BLOCKCOMMENT,
	}

	public class Lexeme
	{
		public TokenType t;
		public string s;
		public Input.Pos pos;

		public Lexeme (TokenType t, Input.Pos pos, string s)
		{
			this.t = t;
			this.s = s;
			this.pos = pos;
		}

		public string GetInfoString ()
		{
			int len = 0;
			foreach (string name in Enum.GetNames(typeof(TokenType)))
				len = Math.Max (len, name.Length);
			return String.Format ("{0," + len.ToString () + "} pos: {1} len: {2,-3} token: \"{3}\"", t, pos.ToString (), s.Length, s);
		}

		public override string ToString ()
		{
			return String.Format ("{{{0} \"{1}\"}}", t, s);
		}
	}

	public class Lexer
	{
		class LexEx : Exception
		{
			public LexEx (string message, Input.Pos position) : base (message)
			{
				this.position = position;
			}

			public Input.Pos position { get; }

			public override string ToString ()
			{
				return this.Message;
			}
		}

		public Lexer (Input input, IO io)
		{
			this.input = input;
			this.io = io;
		}

		public static List<string> keywords { get; } = new List<string> {
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

		// token
		public bool expecttoken (string token)
		{
			if (token.Length <= 0)
				return true;
			if (input.Peek () < 0)
				return false;
			int i = 0;
			while (i < token.Length) {
				if (token [i] != input.Peek ())
					return false;
				++i;
				if (!input.Next ())
					return i >= token.Length;
			}
			return true;
		}

		// %d+
		public Lexeme expectnumber ()
		{
			Input.Pos pos = input.GetPos ();
			String s = "";
			if (input.Has ()) {
				do {
					if (!Char.IsDigit (input.Peek ()))
						break;
					s += input.Peek ();
				} while (input.Next ());
				if (s.Length > 0) {
					int parsed = 0;
					if (!int.TryParse (s, out parsed))
						throw new LexEx ("too high constant value", pos);
					return new Lexeme (TokenType.NUMBER, pos, s);
				}
			}
			throw new LexEx ("number expected", pos);
		}

		// //[^\n]*
		public Lexeme expectcomment ()
		{
			Input.Pos pos = input.GetPos ();
			if (!expecttoken ("//"))
				throw new LexEx ("comment expected", pos);
			string s = "";
			if (input.Has ()) {
				do {
					if (input.Peek () == '\n')
						break;
					s += input.Peek ();
				} while (input.Next ());
			}
			return new Lexeme (TokenType.COMMENT, pos, s);
		}

		// "(\\.|([^"]))*"
		public Lexeme expectstring ()
		{
			Input.Pos pos = input.GetPos ();
			if (!expecttoken ("\""))
				throw new LexEx ("string expected", pos);
			string s = "";
			if (input.Has ()) {
				do {
					if (input.Peek () == '\n') {
						// unexpected newline
						break;
					}
					if (input.Peek () == '\\') {
						// skip escape character
						s += input.Peek ();
						input.Next ();
						s += input.Peek ();
						continue;
					}
					if (input.Peek () == '"') {
						input.Next ();
						// Handle escape characters, for example convert \n to a newline
						// I used the regex package here since I didnt want to hardcode
						// the cases for no real reason to the IF above. This way the system is more flexible
						s = System.Text.RegularExpressions.Regex.Unescape (s);
						return new Lexeme (TokenType.STRING, pos, s);
					}
					s += input.Peek ();
				} while (input.Next ());
			}
			throw new LexEx (String.Format ("unexpected end of string starting at {0}", pos.ToString ()), input.GetPos ());
		}

		// comment := "/*" commentend
		// commentend := ([^/][^*] | comment)* "*/"
		public Lexeme expectblockcomment ()
		{
			Input.Pos pos = input.GetPos ();
			if (!expecttoken ("/*"))
				throw new LexEx ("blockcomment expected", pos);
			string s = "";
			int nestedComments = 0;
			if (input.Has ()) {
				do {
					if (input.HasNext () && input.Peek () == '/' && input.PeekNext () == '*') {
						s += input.Peek ();
						// skip /*
						if (input.Next ())
							s += input.Peek ();
						++nestedComments;
						continue;
					}
					if (input.HasNext () && input.Peek () == '*' && input.PeekNext () == '/') {
						// skip */
						if (nestedComments <= 0) {
							input.Next ();
							input.Next ();
							return new Lexeme (TokenType.BLOCKCOMMENT, pos, s);
						}
						s += input.Peek ();
						if (input.Next ())
							s += input.Peek ();
						--nestedComments;
						continue;
					}
				} while (input.Next ());
			}
			throw new LexEx (String.Format ("unexpected end of blockcomment starting at {0}", pos.ToString ()), input.GetPos ());
		}

		// (\a|_)(\a|\d|_)*
		public Lexeme expectidentifierorkeyword ()
		{
			Input.Pos pos = input.GetPos ();
			if (!input.Has ())
				throw new LexEx ("identifier or keyword expected", pos);
			if (!(Char.IsLetter (input.Peek ()) || input.Peek () == '_'))
				throw new LexEx ("identifier or keyword must start with a letter or underscore", pos);
			string s = "";
			do {
				char c = input.Peek ();
				if (!(Char.IsLetterOrDigit (c) || c == '_'))
					break;
				s += c;
			} while (input.Next ());
			return new Lexeme (keywords.Contains (s) ? TokenType.KEYWORD : TokenType.IDENTIFIER, pos, s);
		}

		public bool lexnext ()
		{
			if (!input.Has ())
				return false;
			char current = input.Peek ();
			char next = input.HasNext () ? input.PeekNext () : ' ';

			try {
				if (current == '/' && next == '/') {
					lexed.Add (expectcomment ());
				} else if (current == '/' && next == '*') {
					lexed.Add (expectblockcomment ());
				} else if (Char.IsLetter (current) || current == '_') {
					lexed.Add (expectidentifierorkeyword ());
				} else if (
					current == ':' && next == '=') {
					lexed.Add (new Lexeme (TokenType.SEPARATOR, input.GetPos (), current.ToString () + next.ToString ()));
					input.Next ();
					input.Next ();
				} else if (
					current == '.' && next == '.') {
					lexed.Add (new Lexeme (TokenType.SEPARATOR, input.GetPos (), current.ToString () + next.ToString ()));
					input.Next ();
					input.Next ();
				} else if (
					current == '(' ||
					current == ')' ||
					current == ':' ||
					current == ';') {
					lexed.Add (new Lexeme (TokenType.SEPARATOR, input.GetPos (), current.ToString ()));
					input.Next ();
				} else if (
					current == '<' ||
					current == '=' ||
					current == '!' ||
					current == '&' ||
					current == '+' ||
					current == '-' ||
					current == '/' ||
					current == '*') {
					lexed.Add (new Lexeme (TokenType.OPERATOR, input.GetPos (), current.ToString ()));
					input.Next ();
				} else if (Char.IsDigit (current)) {
					lexed.Add (expectnumber ());
				} else if (current == '"') {
					lexed.Add (expectstring ());
				} else if (Char.IsWhiteSpace (current)) {
					// skip
					input.Next ();
				} else {
					Input.Pos pos = input.GetPos ();
					input.Next ();
					throw new LexEx (String.Format ("unrecognized token {0}{1}", current, next), pos);
				}
			} catch (LexEx e) {
				errored = true;
				io.WriteLine ("Lexical error at {0}: {1}", e.position, e.ToString ());
				skip_to_next_line ();
				return lexnext ();
			}
			return true;
		}

		public void lexall ()
		{
			while (input.Has ()) {
				if (!lexnext ())
					break;
			}
		}

		public bool hasnext ()
		{
			return lexed.Count > 0;
		}

		public Lexeme next ()
		{
			if (!hasnext () && !lexnext ())
				return null;
			Lexeme l = lexed [0];
			lexed.RemoveAt (0);
			return l;
		}

		void skip_to_next_line ()
		{
			while (input.Has () && input.Peek () != '\n')
				input.Next ();
		}

		public List<Lexeme> lexed { get; } = new List<Lexeme> ();

		public Input input { get; }

		IO io { get; }

		public bool errored { get; private set; } = false;
	}
}

