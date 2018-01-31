using System;

namespace compilers1
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Input input = new Input ();
			Lexer lexer = new Lexer (input.input);
			lexer.lexall ();
			// Parser parser = new Parser (lexer);
			// parser.Parse (lexer.lexed);
			foreach (Lexeme l in lexer.lexed)
				Console.WriteLine (l.ToString ());
		}
	}
}
