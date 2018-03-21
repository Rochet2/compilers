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
        public abstract void WriteLine(string str);
        public abstract void WriteLine(string fmt, params object[] args);
        public abstract void Write(string str);
        public abstract void Write(string fmt, params object[] args);
        public abstract int Read();
    }

    /*
     * Normal console input and ouput
     */
    public class IOConsole : IO
    {
        public override void WriteLine(string str)
        {
            Console.WriteLine(str);
        }

        public override void WriteLine(string fmt, params object[] args)
        {
            Console.WriteLine(fmt, args);
        }

        public override void Write(string str)
        {
            Console.Write(str);
        }

        public override void Write(string fmt, params object[] args)
        {
            Console.Write(fmt, args);
        }

        public override int Read()
        {
            return Console.Read();
        }
    }

    /*
     * IO used for testing.
     * Can be initialized with multiple predefined input strings,
     * which are considered to be separate lines.
     * Exposes output variable, which is a list of strings written to output.
     */
    public class IOTest : IO
    {
        public IOTest()
        {
            input = "";
        }

        public IOTest(params string[] a)
        {
            input = string.Join("\n", a);
        }

        public override void WriteLine(string str)
        {
            Write(str);
            output[output.Count - 1] += '\n';
        }

        public override void WriteLine(string fmt, params object[] args)
        {
            WriteLine(string.Format(fmt, args));
        }

        public override void Write(string str)
        {
            if (output.Count <= 0)
                output.Add(str);
            else
                output[output.Count - 1] += str;
        }

        public override void Write(string fmt, params object[] args)
        {
            Write(string.Format(fmt, args));
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
