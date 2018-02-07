using System;

namespace compilers1
{
	public class Lexeme
	{
		public Type t;
		public string s;
		public Input.Pos pos;

		public Lexeme (Type t, Input.Pos pos, string s)
		{
			this.t = t;
			this.s = s;
			this.pos = pos;
		}

		public override string ToString ()
		{
			int len = 0;
			foreach (string name in Enum.GetNames(typeof(Type)))
				len = Math.Max (len, name.Length);
			return String.Format ("{0," + len.ToString () + "} pos: {1} len: {2,-3} token: \"{3}\"", t, pos.ToString (), s.Length, s);
		}
	}
}
