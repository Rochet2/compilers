using NUnit.Framework;
using System;
namespace Interpreter.sources
{
	[TestFixture()]
	public class StringIOTest
    {
        [Test()]
        public void InitializedEmpty()
        {
            var io = new StringIO();
            Assert.AreEqual("", io.input);
            Assert.AreEqual("", io.output);
        }

        [Test()]
        public void InitializedCorrectly()
        {
            var io = new StringIO("test string");
            Assert.AreEqual("test string", io.input);
            Assert.AreEqual("", io.output);
        }

        [Test()]
        public void WritesCorrectly()
        {
            var io = new StringIO("");
            io.Write("test");
            io.Write(" {0} {1}", 123, 456);
            Assert.AreEqual("test 123 456", io.output);
        }

        [Test()]
        public void WritesLinesCorrectly()
        {
            var io = new StringIO("");
            io.WriteLine("aaa");
            io.Write("bbb");
            io.WriteLine(" {0} {1}", 123, 456);
            Assert.AreEqual("aaa\nbbb 123 456\n", io.output);
        }

        [Test()]
        public void ReadsCorrectly()
        {
            string input = "123\n345";
            var io = new StringIO(input);
            for (int i = 0; i < input.Length; ++i)
                Assert.AreEqual(input[i], io.Read());
            Assert.AreEqual(-1, io.Read());
            Assert.AreEqual(-1, io.Read());
            Assert.AreEqual(-1, io.Read());
        }
	}
}
