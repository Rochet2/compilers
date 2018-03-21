using System;
using System.Collections.Generic;

namespace Interpreter
{
    /*
     * Abstract IO class from which other IO classes inherit.
     * Used to abstract away the implementation of the IO.
     * IO is used for input and output, for example from/to console.
     */
    public abstract class IO
    {
        public virtual void WriteLine(string str)
        {
            Write(str);
            Write("\n");
        }
        public virtual void WriteLine(string fmt, params object[] args)
        {
            WriteLine(string.Format(fmt, args));
        }
        public abstract void Write(string str);
        public virtual void Write(string fmt, params object[] args)
        {
            Write(string.Format(fmt, args));
        }
        public abstract int Read();
    }

    /*
     * Normal console input and ouput
     */
    public class ConsoleIO : IO
    {
        public override void Write(string str)
        {
            Console.Write(str);
        }

        public override int Read()
        {
            return Console.Read();
        }
    }

    /*
     * String input and output.
     */
    public class StringIO : IO
    {
        public StringIO() { this.input = ""; }
        public StringIO(string input) { this.input = input; }

        public override void Write(string str)
        {
            output += str;
        }

        public override int Read()
        {
            if (input.Length < currentInputPosition)
                return -1;
            return input[currentInputPosition++];
        }

        public string input;
        public string output = "";
        private int currentInputPosition = 0;
    }

    /*
     * IO used for testing.
     * Can be initialized with multiple predefined input strings,
     * which are considered to be separate lines.
     * Exposes output variable, which is a list of strings written to output.
     */
    public class TestIO : IO
    {
        public TestIO()
        {
            input = "";
        }

        public TestIO(params string[] a)
        {
            input = string.Join("\n", a);
        }

        public override void Write(string str)
        {
            if (output.Count <= 0)
                output.Add(str);
            else
                output[output.Count - 1] += str;
        }

        public override int Read()
        {
            if (input.Length < currentInputPosition)
                return -1;
            return input[currentInputPosition++];
        }

        private int currentInputPosition = 0;
        private string input;
        public List<string> output { get; private set; } = new List<string> { "" };
    }
}
