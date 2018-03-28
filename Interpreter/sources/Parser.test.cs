using NUnit.Framework;
using System;
namespace Interpreter.sources
{
    [TestFixture()]
    public class ParserTest
    {
        [Test()]
        public void UnexpectedTokenTypeErrors()
        {
            var io = new StringIO();
            var lexer = new Lexer(InputBuffer.ToInputBuffer("assert 0;"), io);
            var parser = new Parser(lexer, io);
            parser.Parse();
            Assert.AreEqual(true, parser.errored);
        }
        [Test()]
        public void UnexpectedTokenValueErrors()
        {
            var io = new StringIO();
            var lexer = new Lexer(InputBuffer.ToInputBuffer("assert );"), io);
            var parser = new Parser(lexer, io);
            parser.Parse();
            Assert.AreEqual(true, parser.errored);
        }
        [Test()]
        public void EmptyStatementErrors()
        {
            var io = new StringIO();
            var lexer = new Lexer(InputBuffer.ToInputBuffer(";"), io);
            var parser = new Parser(lexer, io);
            parser.Parse();
            Assert.AreEqual(true, parser.errored);
        }
        [Test()]
        public void MissingTokensErrors()
        {
            var io = new StringIO();
            var lexer = new Lexer(InputBuffer.ToInputBuffer("var X :"), io);
            var parser = new Parser(lexer, io);
            parser.Parse();
            Assert.AreEqual(true, parser.errored);
        }
        [Test()]
        public void TokensAfterProgramEndErrors()
        {
            var io = new StringIO();
            var lexer = new Lexer(InputBuffer.ToInputBuffer("assert (1=1);123"), io);
            var parser = new Parser(lexer, io);
            parser.Parse();
            Assert.AreEqual(true, parser.errored);
        }
        [Test()]
        public void ErrorFlagFalseInitially()
        {
            var io = new StringIO();
            var lexer = new Lexer(InputBuffer.ToInputBuffer(""), io);
            var parser = new Parser(lexer, io);
            Assert.AreEqual(false, parser.errored);
        }
        [Test()]
        public void ErrorFlagTrueAfterError()
        {
            var io = new StringIO();
            var lexer = new Lexer(InputBuffer.ToInputBuffer(""), io);
            var parser = new Parser(lexer, io);
            parser.Parse();
            StringAssert.Contains(
                "error",
                io.output
            );
            Assert.AreEqual(true, parser.errored);
        }
        [Test()]
        public void UnexpectedForLoopEndErrors()
        {
            var io = new StringIO();
            var lexer = new Lexer(InputBuffer.ToInputBuffer("print 1;end;print 2;"), io);
            var parser = new Parser(lexer, io);
            parser.Parse();
            Assert.AreEqual(true, parser.errored);
        }
    }
}
