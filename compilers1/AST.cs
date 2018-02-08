using System;
using System.Collections.Generic;

namespace compilers1
{
	public enum ASTType
	{
		BINOP,
		UOP,
		NUMBER,
		STRING,
		BOOLEAN,
		PRINT,
		ASSERT,
		IDENTIFIER,
		TYPE,
		STMTS,
		READ,
		VAR,
		ASSIGN,
		FORLOOP,
	}

	public class AST
	{
		public AST (ASTType t)
		{
			this.t = t;
		}

		public readonly ASTType t;
	}

	public abstract class Printable : AST
	{
		public Printable (ASTType t) : base (t)
		{
		}

		public abstract object Value ();
	}

	public class BinOp : AST
	{
		public BinOp (AST l, string op, AST r) :
			base (ASTType.BINOP)
		{
			this.l = l;
			this.r = r;
			this.op = op;
		}

		public AST l;
		public AST r;
		public string op;
	}

	public class AstNumber : Printable
	{
		public AstNumber (string v) :
			base (ASTType.NUMBER)
		{
			this.v = int.Parse (v);
		}

		public AstNumber (int v) :
			base (ASTType.NUMBER)
		{
			this.v = v;
		}

		public override object Value ()
		{
			return v;
		}

		public int v;
	}

	public class AstString : Printable
	{
		public AstString (string v) :
			base (ASTType.STRING)
		{
			this.v = v;
		}

		public override object Value ()
		{
			return v;
		}

		public string v;
	}

	public class AstBool : AST
	{
		public AstBool (bool v) :
			base (ASTType.BOOLEAN)
		{
			this.v = v;
		}

		public bool v;
	}

	public class AstIdent : AST
	{
		public AstIdent (string name) :
			base (ASTType.IDENTIFIER)
		{
			this.name = name;
		}

		public string name;
	}

	public class AstPrint : AST
	{
		public AstPrint (AST toprint) :
		base (ASTType.PRINT)
		{
			this.toprint = toprint;
		}

		public AST toprint;
	}

	public class AstRead : AST
	{
		public AstRead (string ident) :
		base (ASTType.READ)
		{
			this.ident = ident;
		}

		public string ident;
	}

	public class AstAssert : AST
	{
		public AstAssert (AST cond) :
		base (ASTType.ASSERT)
		{
			this.cond = cond;
		}

		public AST cond;
	}

	public class AstUnary : AST
	{
		public AstUnary (string op, AST v) :
			base (ASTType.UOP)
		{
			this.op = op;
			this.v = v;
		}

		public string op;
		public AST v;
	}

	public class AstType : AST
	{
		public AstType (string name) :
		base (ASTType.TYPE)
		{
			this.name = name;
		}

		public string name;
	}

	public class AstStmts : AST
	{
		public AstStmts () :
		base (ASTType.STMTS)
		{
		}

		public List<AST> stmts = new List<AST>();
	}

	public class AstVar : AST
	{
		public AstVar () :
		base (ASTType.VAR)
		{
		}

		public string ident;
		public AST type;
		public AST value = null;
	}

	public class AstAssign : AST
	{
		public AstAssign () :
		base (ASTType.ASSIGN)
		{
		}

		public string ident;
		public AST value;
	}

	public class AstForLoop : AST
	{
		public AstForLoop () :
		base (ASTType.FORLOOP)
		{
		}

		public string ident;
		public AST begin, end, stmts;
	}
}
