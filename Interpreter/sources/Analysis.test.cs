using NUnit.Framework;
using System;
namespace Interpreter.sources
{
    [TestFixture()]
    public class AnalysisTest
    {
        [Test()]
        public void AssignVariableNotDefinedErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "x := 5;");
            StringAssert.Contains(
                "using undefined identifier x",
                io.output
            );
        }
        [Test()]
        public void AssignVariableTypeNotMatchingErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "var x : int; x := \"string\";");
            StringAssert.Contains(
                "variable x type NUMBER does not match value type STRING",
                io.output
            );
        }
        [Test()]
        public void ForLoopStartNotNumberErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "var x : int;for x in (1=1)..2 do print x; end for;");
            StringAssert.Contains(
                "expected type NUMBER, got BOOLEAN",
                io.output
            );
        }
        [Test()]
        public void ForLoopEndNotNumberErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "var x : int;for x in 1..(\"str\") do print x; end for;");
            StringAssert.Contains(
                "expected type NUMBER, got STRING",
                io.output
            );
        }
        [Test()]
        public void ForLoopControlVariableNotDefinedErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "for y in 1..2 do print y; end for;");
            StringAssert.Contains(
                "using undefined identifier y",
                io.output
            );
        }
        [Test()]
        public void ForLoopControlVariableNotNumberErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "var y : string; for y in 1..2 do print y; end for;");
            StringAssert.Contains(
                "expected type NUMBER, got STRING",
                io.output
            );
        }
        [Test()]
        public void DuplicateDefinitionErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "var z : bool; var z : bool;");
            StringAssert.Contains(
                "variable z already defined",
                io.output
            );
        }
        [Test()]
        public void UnknownVariableTypeErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "var a : print;");
            StringAssert.Contains(
                "unknown identifier type name print",
                io.output
            );
        }
        [Test()]
        public void VariableTypeAndValueMismatchErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "var b : bool := \"str\";");
            StringAssert.Contains(
                "variable b type BOOLEAN does not match value type STRING",
                io.output
            );
        }
        [Test()]
        public void PrintedVariableNotDefinedErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "print c;");
            StringAssert.Contains(
                "using undefined identifier c",
                io.output
            );
        }
        [Test()]
        public void ReadOputputVariableNotDefinedErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "read d;");
            StringAssert.Contains(
                "using undefined identifier d",
                io.output
            );
        }
        [Test()]
        public void ReadVariableTypeNotSuitableErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "var e : bool; read e;");
            StringAssert.Contains(
                "variable e has unsupported type BOOLEAN to read from input",
                io.output
            );
        }
        [Test()]
        public void AssertConditionNotBooleanErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "assert (5);");
            StringAssert.Contains(
                "expected type BOOLEAN, got NUMBER",
                io.output
            );
        }
        [Test()]
        public void InvalidIntegerBinaryOperatorErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "print 1&2;");
            StringAssert.Contains(
                "unknown integer binary operator &",
                io.output
            );
        }
        [Test()]
        public void InvalidBinaryOperatorErrors1()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "print 1&\"a\";");
            StringAssert.Contains(
                "unknown binary operator & for operand types left: NUMBER, right: STRING",
                io.output
            );
        }
        [Test()]
        public void InvalidBinaryOperatorErrors2()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "print 1=\"a\";");
            StringAssert.Contains(
                "unknown binary operator = for operand types left: NUMBER, right: STRING",
                io.output
            );
        }
        [Test()]
        public void InvalidUnaryOperatorErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "print &1;");
            StringAssert.Contains(
                "unrecognized unary operator & for operand type NUMBER",
                io.output
            );
        }
        [Test()]
        public void ForLoopVariableAssignErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "var x1 : int; for x1 in 1..2 do x1 := 10; end for;");
            StringAssert.Contains(
                "trying to change immutable variable x1",
                io.output
            );
        }
        [Test()]
        public void ForLoopVariableReadErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "var x2 : int; for x2 in 1..2 do read x2; end for;");
            StringAssert.Contains(
                "trying to change immutable variable x2",
                io.output
            );
        }
        [Test()]
        public void ForLoopVariableNestedErrors()
        {
            var io = new StringIO();
            MainClass.Run(io, true, "var i : int; for i in 1..2 do for i in 1..2 do var x : int; end for; end for;");
            StringAssert.Contains(
                "trying to change immutable variable i",
                io.output
            );
        }
    }
}
