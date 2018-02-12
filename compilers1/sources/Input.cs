using System;
using System.IO;
using System.Text;

namespace compilers1
{
	public class Input
	{
		static MemoryStream ToStream (string s)
		{
			MemoryStream stream = new MemoryStream ();
			StreamWriter writer = new StreamWriter (stream);
			writer.Write (s);
			writer.Flush ();
			stream.Position = 0;
			return stream;
		}

		public static Input ToInput (string s)
		{
			return new Input (ToStream (s));
		}

		public Input (System.IO.Stream stream, string name = null)
		{
			this.name = name ?? "<unknown>";
			this.stream = stream;
			for (int i = 0; i < buff.Length; ++i)
				Next ();
			pos = new Pos ();
		}

		public bool Has ()
		{
			return buff [0] >= 0;
		}

		public bool HasNext ()
		{
			return buff [1] >= 0;
		}

		public bool Next ()
		{
			int previous = buff [0];
			for (int i = 1; i < buff.Length; ++i)
				buff [i - 1] = buff [i];
			buff [buff.Length - 1] = stream.ReadByte ();

			++pos.pos;
			++pos.col;
			if (previous == '\n') {
				++pos.line;
				pos.col = 1;
			}
			return buff [0] >= 0;
		}

		public char Peek (int idx = 0)
		{
			if (idx >= buff.Length)
				throw new IndexOutOfRangeException ("invalid peek index");
			return Convert.ToChar (buff [idx]);
		}

		public char PeekNext ()
		{
			return Peek (1);
		}

		public Pos GetPos ()
		{
			return pos.Copy ();
		}

		public class Pos
		{
			public int pos = 0;
			public int line = 1;
			public int col = 1;

			public Pos Copy ()
			{
				return (Pos)this.MemberwiseClone ();
			}

			public override string ToString ()
			{
				return string.Format ("{0}:{1}", line, col);
			}
		}

		Pos pos = new Pos ();
		int[] buff = new int[2];
		System.IO.Stream stream;

		public string name { get; }
	}
}
