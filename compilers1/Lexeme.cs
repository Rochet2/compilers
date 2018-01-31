using System;

namespace compilers1
{
	public class Lexeme
	{
		public Type t;
		public string s;
		public int start;
		public int end;

		public Lexeme (Type t, int start, string s, int end = -1)
		{
			this.t = t;
			this.s = s;
			this.start = start;
			this.end = end < 0 ? start + s.Length : end;
		}

		public override string ToString ()
		{
			int len = 0;
			foreach (string name in Enum.GetNames(typeof(Type)))
				len = Math.Max (len, name.Length);
			return String.Format ("{0,"+len.ToString()+"} from: {1,-3} to: {2,-3} len: {4,-3} token: \"{3}\"", t, start, end, s, s.Length);
		}
	}
}

