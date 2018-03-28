using NUnit.Framework;
using System;

namespace Interpreter
{
    [TestFixture()]
    public class ProgramTest
    {
        static string Join(params string[] strings)
        {
            return string.Join(Environment.NewLine, strings);
        }

        [Test()]
        public void ExampleCode1()
        {
            var io = new StringIO();
            MainClass.Run(
                io,
                true,
                "var X : int := 4 + (6 * 2);",
                "print X;"
            );
            Assert.AreEqual("16", io.output);
        }
        [Test()]
        public void ExampleCode2()
        {
            var io = new StringIO("3\n");
            MainClass.Run(
                io,
                true,
                "var nTimes : int := 0;",
                "print \"How many times?\";",
                "read nTimes;",
                "var x : int;",
                "for x in 0..nTimes-1 do",
                "print x;",
                "print \" : Hello, World!\\n\";",
                "end for;",
                "assert (x = nTimes);"
            );
            Assert.AreEqual(
                Join(
                    "How many times?3",
                    "0 : Hello, World!",
                    "1 : Hello, World!",
                    "2 : Hello, World!",
                    ""
                ),
                io.output
            );
        }
        [Test()]
        public void ExampleCode3()
        {
            var io = new StringIO("5\n");
            MainClass.Run(
                io,
                true,
                "print \"Give a number\";",
                "var n : int;",
                "read n;",
                "var v : int := 1;",
                "var i : int;",
                "for i in 1..n do",
                "v := v * i;",
                "end for;",
                "print \"The result is: \";",
                "print v;"
            );
            Assert.AreEqual(
                Join(
                    "Give a number5",
                    "The result is: 120"
                ),
                io.output
            );
        }
    }
}
