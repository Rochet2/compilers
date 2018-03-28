using System;
using System.IO;
using System.Diagnostics;

namespace Interpreter
{
    class MainClass
    {
        public static int inputBufferSize = 2;

        /*
         * A function used to run the interpreter as a whole
         * with given IO and InputBuffer.
         */
        public static void Run(IO io, InputBuffer input, bool semanticAnalysis)
        {
            // Lexical analysis
            Lexer lexer = new Lexer(input, io);
            lexer.LexAll(); // tokenize whole input to get all lexical errors immediately

            // Parse to create the AST
            Parser parser = new Parser(lexer, io);
            ASTNode ast = parser.Parse();
            if (lexer.errored || parser.errored)
                return; // Parser or lexer had errors, exit

            if (semanticAnalysis)
            {
                // Pefrorm semantic analysis
                Analysis semanticanalysis = new Analysis(ast, io);
                semanticanalysis.Visit();
                if (semanticanalysis.errored)
                    return; // Semantic analysis failed, exit
            }

            // Run the interpreter
            Interpreter interpreter = new Interpreter(ast, io);
            interpreter.Visit(); // Run interpreter
            if (interpreter.errored)
                io.WriteLine("Interpreter terminated with errors");
        }

        public static void Run(IO io, bool semanticAnalysis, params string[] code)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(string.Join(Environment.NewLine, code));
                    writer.Flush();
                    stream.Position = 0;
                    var input = new InputBuffer(stream, inputBufferSize);
                    Run(io, input, semanticAnalysis);
                }
            }
        }

        /*
         * The entry point of the program.
         * Handles input parameters.
         */
        public static void Main(string[] args)
        {
            IO io = new ConsoleIO();
            if (args.Length != 1)
            {
                io.WriteLine("You must give 1 program argument, which is the input file path");
                io.WriteLine("You gave {0} arguments", args.Length);
                return;
            }
            try
            {
                // Try read input file and run with it as the input
                using (FileStream fileStream = File.Open(args[0], FileMode.Open))
                {
                    InputBuffer input = new InputBuffer(fileStream, inputBufferSize);
                    Run(io, input, true);
                }
            }
            catch (Exception e)
            {
                // log any exceptions, for example related to file opening
                io.WriteLine("There were problems with using file {0}:", args[0]);
                io.WriteLine(e.ToString());
            }
        }
    }
}
