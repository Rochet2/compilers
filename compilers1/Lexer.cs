using System;
using System.Collections.Generic;
using System.Text;

namespace compilers1
{
	class LexEx : Exception
	{
		public LexEx (string message, Input.Pos position) : base (message)
		{
			this.position = position;
		}

		Input.Pos position;
	}

	public class Lexer
	{
		public Lexer (Input input)
		{
			this.input = input;
			this.tokenparser = new TokenParser (input);
		}

		public void lexnext ()
		{
			if (!input.Has ())
				return;
			char current = input.Peek ();
			char next = input.HasNext () ? input.PeekNext () : ' ';

			if (current == '/' && next == '/') {
				lexed.Add (tokenparser.expectcomment ());
			} else if (current == '/' && next == '*') {
				lexed.Add (tokenparser.expectblockcomment ());
			} else if (Char.IsLetter (current)) {
				lexed.Add (tokenparser.expectidentifierorkeyword ());
			} else if (
				current == ':' && next == '=') {
				lexed.Add (new Lexeme (Type.SEPARATOR, input.GetPos (), current.ToString () + next.ToString ()));
				input.Next ();
				input.Next ();
			} else if (
				current == '.' && next == '.') {
				lexed.Add (new Lexeme (Type.SEPARATOR, input.GetPos (), current.ToString () + next.ToString ()));
				input.Next ();
				input.Next ();
			} else if (
				current == '(' ||
				current == ')' ||
				current == ':' ||
				current == ';') {
				lexed.Add (new Lexeme (Type.SEPARATOR, input.GetPos (), current.ToString ()));
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
				lexed.Add (new Lexeme (Type.OPERATOR, input.GetPos (), current.ToString ()));
				input.Next ();
			} else if (Char.IsDigit (current)) {
				lexed.Add (tokenparser.expectnumber ());
			} else if (current == '"') {
				lexed.Add (tokenparser.expectstring ());
			} else if (Char.IsWhiteSpace (current)) {
				// skip
				input.Next ();
			} else {
				Console.WriteLine (String.Format ("Unrecognized tokens {0}{1} at index {2}", current, next, input.GetPos ().ToString ()));
			}
		}

		public void lexall ()
		{
			while (input.Has ()) {
				// Console.Out.WriteLine (i); // debug
				lexnext ();
			}
		}

		public bool hasnext ()
		{
			return lexed.Count > 0;
		}

		public Lexeme next ()
		{
			if (!hasnext ())
				lexnext ();
			if (!hasnext ())
				return null;
			Lexeme l = lexed [0];
			lexed.RemoveAt (0);
			return l;
		}

		public List<Lexeme> lexed = new List<Lexeme> ();
		Input input;
		TokenParser tokenparser;
	}
}

