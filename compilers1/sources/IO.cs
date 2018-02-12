using System;
using System.Collections.Generic;

namespace compilers1
{
	public abstract class IO
	{
		public abstract void WriteLine (string fmt, params object[] args);

		public abstract void Write (string fmt, params object[] args);

		public abstract int Read ();
	}

	public class IOConsole : IO
	{
		public override void WriteLine (string fmt, params object[] args)
		{
			Console.WriteLine (fmt, args);
		}

		public override void Write (string fmt, params object[] args)
		{
			Console.Write (fmt, args);
		}

		public override int Read ()
		{
			return Console.Read ();
		}
	}

	public class IOTest : IO
	{
		public IOTest ()
		{
			input = "";
		}

		public IOTest (params string[] a)
		{
			input = String.Join ("\n", a);
		}

		public override void WriteLine (string fmt, params object[] args)
		{
			if (output.Count <= 0)
				output.Add (String.Format (fmt, args));
			else
				output [output.Count - 1] += String.Format (fmt, args);
			output [output.Count - 1] += '\n';
		}

		public override void Write (string fmt, params object[] args)
		{
			if (output.Count <= 0)
				output.Add (String.Format (fmt, args));
			else
				output [output.Count - 1] += String.Format (fmt, args);
		}

		public override int Read ()
		{
			if (input.Length < inputpos)
				return -1;
			return input [inputpos++];
		}

		int inputpos = 0;
		string input;

		public List<string> output { get; private set; } = new List<string> { "" };
	}
}

