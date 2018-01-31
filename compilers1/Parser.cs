using System;
using System.Collections.Generic;

namespace compilers1
{
	public class Parser
	{
		public Parser (Lexer lexer)
		{
			this.lexer = lexer;
		}

		void skip(string tok)
		{
			Lexeme l = lexer.next ();
			if (l.s == tok)
				return;
		}

		public void Parse()
		{
			
		}

		Lexer lexer;
	}
}

