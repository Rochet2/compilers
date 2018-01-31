using System;
using System.IO;
using System.Text;

namespace compilers1
{
	class MainClass
	{
		public static string Read ()
		{
			string path = @"input.txt";
			string readText = "";
			try {
				readText = File.ReadAllText (path);
			} catch (System.IO.IOException e) {
				Console.WriteLine ("Error reading file: {0}", e.ToString ());
			}
			return readText;
		}

		public static void Main (string[] args)
		{
			Lexer lexer = new Lexer ();
			string input = Read ();
			lexer.Lex (input);
			foreach (Lexeme l in lexer.lexed)
				Console.WriteLine (l.ToString ());
			Parser parser = new Parser ();
			parser.Parse (lexer.lexed);
		}
	}
}
