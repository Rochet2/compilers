using System;
using System.IO;
using System.Text;

namespace compilers1
{
	public class Input
	{
		public Input ()
		{
			Read ();
		}

		public Input (string s)
		{
			this.input = s;
		}

		public void Read ()
		{
			this.input = "";
			this.pos = 0;
			string path = @"input.txt";
			try {
				input = File.ReadAllText (path);
			} catch (System.IO.IOException e) {
				Console.WriteLine ("Error reading file {1}: {0}", e.ToString (), path);
			}
		}

		public bool hasnext ()
		{
			return pos < input.Length;
		}

		public char next ()
		{
			if (pos >= input.Length)
				throw new IndexOutOfRangeException ("Input end reached");
			return input [pos++];
		}

		string input = "";
		int pos = 0;
	}
}
