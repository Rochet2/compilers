using NUnit.Framework;
using System;
namespace Interpreter.sources
{
    [TestFixture()]
    public class PositionTest
    {
        [Test()]
        public void CopyNotEqualToOriginal()
        {
            var position = new InputBuffer.Position();
            Assert.AreNotSame(position, position.Copy());
        }
        [Test()]
        public void ToStringWorks()
        {
            var position = new InputBuffer.Position();
            position.line = 62;
            position.column = 4;
            Assert.AreEqual("62:4", position.ToString());
        }
    }

    [TestFixture()]
    public class InputBufferTest
    {
        [Test()]
        public void HasCharacterReturnsTrueWhenHasInput()
        {
            var buffer = InputBuffer.ToInputBuffer("my input");
            Assert.AreEqual(true, buffer.HasCharacter());
        }
        [Test()]
        public void HasCharacterReturnsFalseWhenNoInput()
        {
            var buffer = InputBuffer.ToInputBuffer("");
            Assert.AreEqual(false, buffer.HasCharacter());
        }
        [Test()]
        public void HasNextCharacterReturnsTrueWhenHasInput()
        {
            var buffer = InputBuffer.ToInputBuffer("my input");
            Assert.AreEqual(true, buffer.HasNextCharacter());
        }
        [Test()]
        public void HasNextCharacterReturnsFalseWhenNoInput()
        {
            var buffer = InputBuffer.ToInputBuffer("m");
            Assert.AreEqual(false, buffer.HasNextCharacter());
        }
        [Test()]
        public void NextReturnsTrueWhenHasInput()
        {
            var buffer = InputBuffer.ToInputBuffer("ab");
            Assert.AreEqual(true, buffer.Next());
        }
        [Test()]
        public void NextReturnsFalseWhenNoInput()
        {
            var buffer = InputBuffer.ToInputBuffer("a");
            Assert.AreEqual(false, buffer.Next());
        }
        [Test()]
        public void NextAdvancesInput()
        {
            var buffer = InputBuffer.ToInputBuffer("ab");
            Assert.AreEqual('a', buffer.PeekCharacter());
            Assert.AreEqual(true, buffer.Next());
            Assert.AreEqual('b', buffer.PeekCharacter());
        }
        [Test()]
        public void PeekCharacterReturnsCharacterWhenHasInput()
        {
            var buffer = InputBuffer.ToInputBuffer("ab");
            Assert.AreEqual('a', buffer.PeekCharacter());
        }
        [Test()]
        public void PeekNextReturnsNextCharacterWhenHasInput()
        {
            var buffer = InputBuffer.ToInputBuffer("ab");
            Assert.AreEqual('b', buffer.PeekNext());
        }
        [Test()]
        public void PeekCharacterThrowsWhenNoInput()
        {
            var buffer = InputBuffer.ToInputBuffer("");
            Assert.Throws<OverflowException>(() => buffer.PeekCharacter());
        }
        [Test()]
        public void PeekNextThrowsWhenNoInput()
        {
            var buffer = InputBuffer.ToInputBuffer("a");
            Assert.Throws<OverflowException>(() => buffer.PeekNext());
        }
        [Test()]
        public void GetPositionReturnsCorrectPositionsForCharacters()
        {
            var buffer = InputBuffer.ToInputBuffer("a\nb");
            Assert.AreEqual('a', buffer.PeekCharacter());
            Assert.AreEqual(1, buffer.GetPosition().line);
            Assert.AreEqual(1, buffer.GetPosition().column);
            buffer.Next();
            Assert.AreEqual('\n', buffer.PeekCharacter());
            Assert.AreEqual(1, buffer.GetPosition().line);
            Assert.AreEqual(2, buffer.GetPosition().column);
            buffer.Next();
            Assert.AreEqual('b', buffer.PeekCharacter());
            Assert.AreEqual(2, buffer.GetPosition().line);
            Assert.AreEqual(1, buffer.GetPosition().column);
            buffer.Next();
            Assert.AreEqual(false, buffer.HasCharacter());
            Assert.AreEqual(2, buffer.GetPosition().line);
            Assert.AreEqual(2, buffer.GetPosition().column);
            buffer.Next();
            Assert.AreEqual(false, buffer.HasCharacter());
            Assert.AreEqual(2, buffer.GetPosition().line);
            Assert.AreEqual(3, buffer.GetPosition().column);
        }
        [Test()]
        public void GetPositionReturnsCopy()
        {
            var buffer = InputBuffer.ToInputBuffer("ab");
            Assert.AreNotSame(buffer.GetPosition(), buffer.GetPosition());
        }
    }
}
