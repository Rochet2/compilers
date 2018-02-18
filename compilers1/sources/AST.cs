using System;
using System.Collections.Generic;

namespace compilers1
{
	public enum ASTType
	{
		NUMBER,
		STRING,
		BOOLEAN,
		IDENTIFIER,
		TYPENAME,
		PRINT,
		PRINTABLE,
		ASSERT,
		READ,
		VARIABLE,
		ASSIGN,
		FORLOOP,
		EXPR,
		BINARYOP,
		UNARYOP,
		STATEMENTS,
	}

	public class AST
	{
		public AST (ASTType t, Lexeme tok = null)
		{
			this.t = t;
			this.tok = tok;
		}

		public readonly ASTType t;
		public Lexeme tok;
	}

	public abstract class Printable : AST
	{
		public Printable (ASTType t) : base (t)
		{
		}

		public abstract object Value ();
	}

	public class AstExpr : AST
	{
		public AstExpr () :
			base (ASTType.EXPR)
		{
		}

		public AST lopnd;
		public AST rtail;
	}

	public class BinaryOperator : AST
	{
		public BinaryOperator () :
			base (ASTType.BINARYOP)
		{
		}

		public string op;
		public AST r;
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

	public class AstBool : Printable
	{
		public AstBool (bool v) :
			base (ASTType.BOOLEAN)
		{
			this.v = v;
		}

		public override object Value ()
		{
			return v;
		}

		public bool v;
	}

	public class AstIdentifier : AST
	{
		public AstIdentifier (string name) :
			base (ASTType.IDENTIFIER)
		{
			this.name = name;
		}

		public string name;
	}

	public class AstPrint : AST
	{
		public AstPrint () :
			base (ASTType.PRINT)
		{
		}

		public AST toprint;
	}

	public class AstRead : AST
	{
		public AstRead () :
			base (ASTType.READ)
		{
		}

		public AST ident;
	}

	public class AstAssert : AST
	{
		public AstAssert () :
			base (ASTType.ASSERT)
		{
		}

		public AST cond;
	}

	public class AstUnaryOperator : AST
	{
		public AstUnaryOperator () :
			base (ASTType.UNARYOP)
		{
		}

		public string op;
		public AST v;
	}

	public class AstTypename : AST
	{
		public AstTypename (string name) :
			base (ASTType.TYPENAME)
		{
			this.name = name;
		}

		public string name;
	}

	public class AstStatements : AST
	{
		public AstStatements () :
			base (ASTType.STATEMENTS)
		{
		}

		public AST stmt;
		public AST stmttail;
	}

	public class AstVariable : AST
	{
		public AstVariable () :
			base (ASTType.VARIABLE)
		{
		}

		public AST ident;
		public AST type;
		public AST value = null;
	}

	public class AstAssign : AST
	{
		public AstAssign () :
			base (ASTType.ASSIGN)
		{
		}

		public AST ident;
		public AST value;
	}

	public class AstForLoop : AST
	{
		public AstForLoop () :
			base (ASTType.FORLOOP)
		{
		}

		public AST ident;
		public AST begin, end, stmts;
	}
}
