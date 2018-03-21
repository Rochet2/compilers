using System;
using System.IO;
using System.Diagnostics;

namespace Interpreter
{
    class MainClass
    {
        /*
         * A function used to run the program as a whole
         * with given IO and InputBuffer.
         */
        public static void Run(IO io, InputBuffer input)
        {
            Lexer lexer = new Lexer(input, io);
            lexer.LexAll();
            Parser parser = new Parser(lexer, io);
            ASTNode ast = parser.Parse();
            if (lexer.errored || parser.errored)
                return;
            Analysis semanticanalysis = new Analysis(ast, io);
            semanticanalysis.Visit();
            if (semanticanalysis.errored)
                return;
            Interpreter inter = new Interpreter(ast, io);
            inter.Visit();
            if (inter.errored)
                io.WriteLine("Interpreter terminated with errors");
        }

        public static void Main(string[] args)
        {
            IO io = new IOConsole();
            if (args.Length != 1)
            {
                io.WriteLine("You must give 1 program argument, which is the input file path");
                io.WriteLine("You gave {0} arguments", args.Length);
                return;
            }
            try
            {
                using (FileStream fileStream = File.Open(args[0], FileMode.Open))
                {
                    InputBuffer input = new InputBuffer(fileStream, 2);
                    Run(io, input);
                }
            }
            catch (Exception e)
            {
                // log any file opening exceptions
                io.WriteLine("There were problems with using file {0}:", args[0]);
                io.WriteLine(e.ToString());
            }
        }
    }
}
