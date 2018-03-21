using Interpreter;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace compilers1
{
    [TestFixture()]
    public class ProgramTest
    {
        [Test()]
        public void TestCase()
        {
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer("123456", 2), new IOConsole()).ExpectNumber().token == "123456", "number parsing invalid");
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer("//test", 2), new IOConsole()).ExpectComment().token == "test", "comment parsing invalid");
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer("id_1", 2), new IOConsole()).ExpectIdentifierOrKeyword().token == "id_1", "identifier parsing invalid");
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer("\"test string\"", 2), new IOConsole()).ExpectString().token == "\"test string\"", "string parsing invalid");
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer(@"""\""\""\""""", 2), new IOConsole()).ExpectString().token == "\"\"\"", "string parsing invalid");
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer("/*comment/*nested*/*/", 2), new IOConsole()).ExpectBlockComment().token == "comment/*nested*/", "blockcomment parsing invalid");
            Debug.Assert(new Lexer(InputBuffer.ToInputBuffer("/*/*/**//**/*/*//**/", 2), new IOConsole()).ExpectBlockComment().token == "/*/**//**/*/", "blockcomment parsing invalid");
        }
    }
}
