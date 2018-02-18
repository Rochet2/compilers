using System;
using System.IO;
using System.Diagnostics;

namespace compilers1
{
	class MainClass
	{

		public static void Main (string[] args)
		{
			Debug.Assert (new Lexer (Input.ToInput ("123456"), new IOConsole ()).expectnumber ().s == "123456", "number parsing invalid");
			Debug.Assert (new Lexer (Input.ToInput ("//test"), new IOConsole ()).expectcomment ().s == "test", "comment parsing invalid");
			Debug.Assert (new Lexer (Input.ToInput ("id_1"), new IOConsole ()).expectidentifierorkeyword ().s == "id_1", "identifier parsing invalid");
			Debug.Assert (new Lexer (Input.ToInput ("\"test string\""), new IOConsole ()).expectstring ().s == "\"test string\"", "string parsing invalid");
			Debug.Assert (new Lexer (Input.ToInput (@"""\""\""\"""""), new IOConsole ()).expectstring ().s == "\"\"\"", "string parsing invalid");
			Debug.Assert (new Lexer (Input.ToInput ("/*comment/*nested*/*/"), new IOConsole ()).expectblockcomment ().s == "comment/*nested*/", "blockcomment parsing invalid");
			Debug.Assert (new Lexer (Input.ToInput ("/*/*/**//**/*/*//**/"), new IOConsole ()).expectblockcomment ().s == "/*/**//**/*/", "blockcomment parsing invalid");
			Input input = new Input (File.Open ("input.txt", FileMode.Open));
			IO io = new IOConsole ();
			Lexer lexer = new Lexer (input, io); // (Input.ToInput ("var x : int := 45+5;print x+1;"));
			lexer.lexall ();
			Parser parser = new Parser (lexer, io);
			AST ast = parser.Parse ();
			if (lexer.errored || parser.errored)
				return;
			Analysis semanticanalysis = new Analysis (ast, io);
			semanticanalysis.visit();
			if (semanticanalysis.errored)
				return;
			Interpreter inter = new Interpreter (ast, io);
			inter.visit ();
			if (inter.errored) {
				io.WriteLine ("Interpreter terminated with errors");
				return;
			}
		}
	}
}
