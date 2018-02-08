using System;
using System.IO;
using System.Diagnostics;

namespace compilers1
{
	class MainClass
	{
		public static MemoryStream ToStream (string s)
		{
			MemoryStream stream = new MemoryStream ();
			StreamWriter writer = new StreamWriter (stream);
			writer.Write (s);
			writer.Flush ();
			stream.Position = 0;
			return stream;
		}

		public static Input ToInput (string s)
		{
			return new Input (ToStream (s));
		}

		public static void Main (string[] args)
		{
			Debug.Assert (new TokenParser (ToInput ("123456")).expectnumber ().s == "123456", "number parsing invalid");
			Debug.Assert (new TokenParser (ToInput ("//test")).expectcomment ().s == "test", "comment parsing invalid");
			Debug.Assert (new TokenParser (ToInput ("id_1")).expectidentifierorkeyword ().s == "id_1", "identifier parsing invalid");
			Debug.Assert (new TokenParser (ToInput ("\"test string\"")).expectstring ().s == "\"test string\"", "string parsing invalid");
			Debug.Assert (new TokenParser (ToInput (@"""\""\""\""""")).expectstring ().s == "\"\"\"", "string parsing invalid");
			Debug.Assert (new TokenParser (ToInput ("/*comment/*nested*/*/")).expectblockcomment ().s == "comment/*nested*/", "blockcomment parsing invalid");
			Debug.Assert (new TokenParser (ToInput ("/*/*/**//**/*/*//**/")).expectblockcomment ().s == "/*/**//**/*/", "blockcomment parsing invalid");
			Input input = new Input (File.Open ("input.txt", FileMode.Open));
			Lexer lexer = new Lexer (input); // (ToInput ("var x : int := 45+5;print x+1;"));
			lexer.lexall ();
			foreach (Lexeme l in lexer.lexed)
				Console.WriteLine (l.ToString ());
			Parser parser = new Parser (lexer);
			AST ast = parser.Parse ();
			Interpreter inter = new Interpreter (ast);
			inter.visit ();
		}
	}
}
