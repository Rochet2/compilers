using System;
using System.Collections.Generic;

namespace compilers1
{
	// All functions consume input and leave it at the first not expected token
	public class TokenParser
	{
		Input input;

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

		public TokenParser (Input input)
		{
			this.input = input;
		}

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
					return new Lexeme (Type.NUMBER, pos, s);
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
			return new Lexeme (Type.COMMENT, pos, s);
		}

		// "(\\.|([^"]))"
		public Lexeme expectstring ()
		{
			Input.Pos pos = input.GetPos ();
			if (!expecttoken ("\""))
				throw new LexEx ("string expected", pos);
			string s = "";
			if (input.Has ()) {
				do {
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
						s = System.Text.RegularExpressions.Regex.Unescape(s);
						return new Lexeme (Type.STRING, pos, s);
					}
					s += input.Peek ();
				} while (input.Next ());
			}
			throw new LexEx ("unexpected end of string", input.GetPos ());
		}

		// comment := "/*" commentend
		// commentend := ([^/][^*] | comment) "*/"
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
							return new Lexeme (Type.BLOCKCOMMENT, pos, s);
						}
						s += input.Peek ();
						if (input.Next ())
							s += input.Peek ();
						--nestedComments;
						continue;
					}
				} while (input.Next ());
			}
			throw new LexEx ("unexpected end of blockcomment", input.GetPos ());
		}

		// [\a_][\a\d_]
		public Lexeme expectidentifierorkeyword ()
		{
			Input.Pos pos = input.GetPos ();
			if (!input.Has ())
				throw new LexEx ("identifier or keyword expected", pos);
			if (!(Char.IsLetter (input.Peek ()) || input.Peek () == '_'))
				throw new LexEx ("identifier must start with a letter or underscore", pos);
			string s = "";
			do {
				char c = input.Peek ();
				if (!(Char.IsLetterOrDigit (c) || c == '_'))
					break;
				s += c;
			} while (input.Next ());
			return new Lexeme (keywords.Contains (s) ? Type.KEYWORD : Type.IDENTIFIER, pos, s);
		}
	}
}
