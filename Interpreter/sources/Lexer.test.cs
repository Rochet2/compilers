using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Interpreter.sources
{
    [TestFixture()]
    public class LexerTest
    {
        [Test()]
        public void ErrorFlagIsTrueOnError()
        {
            var io = new StringIO();
            var lexer = new Lexer(
                InputBuffer.ToInputBuffer("\""),
                io
            );
            lexer.LexNext();
            Assert.AreEqual(true, lexer.errored);
        }
        [Test()]
        public void ErrorFlagIsFalseInitially()
        {
            var io = new StringIO();
            var lexer = new Lexer(
                InputBuffer.ToInputBuffer(""),
                io
            );
            Assert.AreEqual(false, lexer.errored);
        }
        [Test()]
        public void ScannerSkipsToNextLineOnError()
        {
            var io = new StringIO();
            var lexer = new Lexer(
                InputBuffer.ToInputBuffer("\"broken string\n123"),
                io
            );
            var lexeme = lexer.NextLexeme();
            Assert.AreEqual(true, lexer.errored);
            Assert.AreEqual("123", lexeme.token);
            Assert.AreEqual(TokenType.NUMBER, lexeme.type);
        }
        [Test()]
        public void UnrecognizedTokenHasError()
        {
            var io = new StringIO();
            var lexer = new Lexer(
                InputBuffer.ToInputBuffer("unrecognized token #;"),
                io
            );
            lexer.LexAll();
            Assert.AreEqual(true, lexer.errored);
        }
        [Test()]
        public void TooHighConstantHasError()
        {
            var io = new StringIO();
            var lexer = new Lexer(
                InputBuffer.ToInputBuffer("print 99999999999999999999999;"),
                io
            );
            lexer.LexAll();
            Assert.AreEqual(true, lexer.errored);
        }
        [Test()]
        public void UnknownEscapeCharacterHasError()
        {
            var io = new StringIO();
            var lexer = new Lexer(
                InputBuffer.ToInputBuffer("print \"test \\x\";"),
                io
            );
            lexer.LexAll();
            Assert.AreEqual(true, lexer.errored);
        }
        [Test()]
        public void EndlessStringLiteralHasError()
        {
            var io = new StringIO();
            var lexer = new Lexer(
                InputBuffer.ToInputBuffer("\"endless string literal"),
                io
            );
            lexer.LexAll();
            Assert.AreEqual(true, lexer.errored);
        }
        [Test()]
        public void EndlessBlockCommentHasError()
        {
            var io = new StringIO();
            var lexer = new Lexer(
                InputBuffer.ToInputBuffer("/* endless block comment"),
                io
            );
            lexer.LexAll();
            Assert.AreEqual(true, lexer.errored);
        }

        [Test()]
        public void NumberRecognized()
        {
            var io = new StringIO();
            var lexeme = new Lexer(
                InputBuffer.ToInputBuffer("123456"),
                io
            ).NextLexeme();
            Assert.AreEqual("", io.output);
            Assert.AreEqual("1:1", lexeme.position.ToString());
            Assert.AreEqual("123456", lexeme.token);
            Assert.AreEqual(TokenType.NUMBER, lexeme.type);
        }
        [Test()]
        public void CommentSkipped()
        {
            var io = new StringIO();
            var lexeme = new Lexer(
                InputBuffer.ToInputBuffer("// test\n123"),
                io
            ).NextLexeme();
            Assert.AreEqual("", io.output);
            Assert.AreEqual("2:1", lexeme.position.ToString());
            Assert.AreEqual("123", lexeme.token);
            Assert.AreEqual(TokenType.NUMBER, lexeme.type);
        }
        [Test()]
        public void CommentRecognized()
        {
            var io = new StringIO();
            var lexeme = new Lexer(
                InputBuffer.ToInputBuffer("// test"),
                io
            ).ExpectComment();
            Assert.AreEqual("", io.output);
            Assert.AreEqual("1:1", lexeme.position.ToString());
            Assert.AreEqual(" test", lexeme.token);
            Assert.AreEqual(TokenType.COMMENT, lexeme.type);
        }
        [Test()]
        public void BlockCommentSkipped()
        {
            var io = new StringIO();
            var lexeme = new Lexer(
                InputBuffer.ToInputBuffer("/**/\n123"),
                io
            ).NextLexeme();
            Assert.AreEqual("", io.output);
            Assert.AreEqual("2:1", lexeme.position.ToString());
            Assert.AreEqual("123", lexeme.token);
            Assert.AreEqual(TokenType.NUMBER, lexeme.type);
        }
        [Test()]
        public void BlockCommentRecognized()
        {
            var io = new StringIO();
            var lexeme = new Lexer(
                InputBuffer.ToInputBuffer("/*test*/"),
                io
            ).ExpectBlockComment();
            Assert.AreEqual("", io.output);
            Assert.AreEqual("1:1", lexeme.position.ToString());
            Assert.AreEqual("test", lexeme.token);
            Assert.AreEqual(TokenType.BLOCKCOMMENT, lexeme.type);
        }
        [Test()]
        public void BlockCommentNestedRecognized()
        {
            var io = new StringIO();
            var lexeme = new Lexer(
                InputBuffer.ToInputBuffer("/*test/*test*/test*/"),
                io
            ).ExpectBlockComment();
            Assert.AreEqual("", io.output);
            Assert.AreEqual("1:1", lexeme.position.ToString());
            Assert.AreEqual("test/*test*/test", lexeme.token);
            Assert.AreEqual(TokenType.BLOCKCOMMENT, lexeme.type);
        }
        public void WhitespaceSkipped()
        {
            var io = new StringIO();
            var lexer = new Lexer(
                InputBuffer.ToInputBuffer("  123 \n\n 456 "),
                io
            );
            var lexeme1 = lexer.NextLexeme();
            var lexeme2 = lexer.NextLexeme();
            Assert.AreEqual("", io.output);
            Assert.AreEqual("1:3", lexeme1.position.ToString());
            Assert.AreEqual("123", lexeme1.token);
            Assert.AreEqual("3:2", lexeme2.position.ToString());
            Assert.AreEqual("456", lexeme2.token);
        }
        [Test()]
        public void IdentifierRecognized()
        {
            var io = new StringIO();
            var lexeme = new Lexer(
                InputBuffer.ToInputBuffer("id_1"),
                io
            ).NextLexeme();
            Assert.AreEqual("", io.output);
            Assert.AreEqual("1:1", lexeme.position.ToString());
            Assert.AreEqual("id_1", lexeme.token);
            Assert.AreEqual(TokenType.IDENTIFIER, lexeme.type);
        }
        [Test()]
        public void KeywordRecognized()
        {
            var io = new StringIO();
            var lexeme = new Lexer(
                InputBuffer.ToInputBuffer("print"),
                io
            ).NextLexeme();
            Assert.AreEqual("", io.output);
            Assert.AreEqual("1:1", lexeme.position.ToString());
            Assert.AreEqual("print", lexeme.token);
            Assert.AreEqual(TokenType.KEYWORD, lexeme.type);
        }
        [Test()]
        public void StringRecognized()
        {
            var io = new StringIO();
            var lexeme = new Lexer(
                InputBuffer.ToInputBuffer("\"test\""),
                io
            ).NextLexeme();
            Assert.AreEqual("", io.output);
            Assert.AreEqual("1:1", lexeme.position.ToString());
            Assert.AreEqual("test", lexeme.token);
            Assert.AreEqual(TokenType.STRING, lexeme.type);
        }
        [Test()]
        public void SeparatorRecognized()
        {
            var io = new StringIO();
            var lexeme = new Lexer(
                InputBuffer.ToInputBuffer(";"),
                io
            ).NextLexeme();
            Assert.AreEqual("", io.output);
            Assert.AreEqual("1:1", lexeme.position.ToString());
            Assert.AreEqual(";", lexeme.token);
            Assert.AreEqual(TokenType.SEPARATOR, lexeme.type);
        }
        [Test()]
        public void OperatorRecognized()
        {
            var io = new StringIO();
            var lexeme = new Lexer(
                InputBuffer.ToInputBuffer("+"),
                io
            ).NextLexeme();
            Assert.AreEqual("", io.output);
            Assert.AreEqual("1:1", lexeme.position.ToString());
            Assert.AreEqual("+", lexeme.token);
            Assert.AreEqual(TokenType.OPERATOR, lexeme.type);
        }
    }
}
