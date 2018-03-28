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
            if (currentInputPosition >= input.Length)
                return -1;
            output += input[currentInputPosition];
            return input[currentInputPosition++];
        }

        public string input;
        public string output = "";
        private int currentInputPosition = 0;
    }
}
