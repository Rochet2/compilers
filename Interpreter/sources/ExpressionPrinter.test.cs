using NUnit.Framework;
using System;
namespace Interpreter.sources
{
    [TestFixture()]
    public class ExpressionPrinterTest
    {
        [Test()]
        public void OutputsNodeToken()
        {
            var io = new StringIO();
            var ast = new IdentifierNode("");
            ast.lexeme = new Lexeme(TokenType.IDENTIFIER, new InputBuffer.Position(), "x");
            var visitor = new ExpressionPrinter(ast, io);
            visitor.Visit();
            Assert.AreEqual(false, visitor.errored);
            Assert.AreEqual("x", io.output);
        }
    }
}
