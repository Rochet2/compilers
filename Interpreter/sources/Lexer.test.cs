using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Interpreter.sources
{
    [TestFixture()]
    public class LexerTest
    {
        [Test()]
        public void TestCase()
        {
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer("123456", 2), new ConsoleIO()).ExpectNumber().token == "123456", "number parsing invalid");
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer("//test", 2), new ConsoleIO()).ExpectComment().token == "test", "comment parsing invalid");
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer("id_1", 2), new ConsoleIO()).ExpectIdentifierOrKeyword().token == "id_1", "identifier parsing invalid");
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer("\"test string\"", 2), new ConsoleIO()).ExpectString().token == "\"test string\"", "string parsing invalid");
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer(@"""\""\""\""""", 2), new ConsoleIO()).ExpectString().token == "\"\"\"", "string parsing invalid");
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer("/*comment/*nested*/*/", 2), new ConsoleIO()).ExpectBlockComment().token == "comment/*nested*/", "blockcomment parsing invalid");
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer("/*/*/**//**/*/*//**/", 2), new ConsoleIO()).ExpectBlockComment().token == "/*/**//**/*/", "blockcomment parsing invalid");
        }
    }
}
