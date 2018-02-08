using System;
using System.Collections.Generic;

namespace compilers1
{
	class ParEx : Exception
	{
		public ParEx (string message, Lexeme lexeme) : base (message)
		{
			this.lexeme = lexeme;
		}

		Lexeme lexeme;
	}

	public class Parser
	{
		public Parser (Lexer lexer)
		{
			this.lexer = lexer;
		}

		void next ()
		{
			curr_tok = next_tok;
			next_tok = lexer.next ();
		}

		void eat (string s)
		{
			if (curr_tok == null || curr_tok.s != s)
				throw new ParEx ("could not find expected token", curr_tok);
			next ();
		}

		void eat (Type t)
		{
			if (curr_tok == null || curr_tok.t != t)
				throw new ParEx ("could not find expected token", curr_tok);
			next ();
		}

		void eat (string s, Type t)
		{
			if (curr_tok == null || curr_tok.s != s)
				throw new ParEx ("could not find expected token", curr_tok);
			if (curr_tok == null || curr_tok.t != t)
				throw new ParEx ("could not find expected token", curr_tok);
			next ();
		}

		bool istype (Type t)
		{
			return curr_tok != null && curr_tok.t == t;
		}

		bool istype (string s)
		{
			return curr_tok != null && curr_tok.s == s;
		}

		bool istype (string s, Type t)
		{
			return istype (s) && istype (t);
		}

		AST NUM ()
		{
			var tok = curr_tok;
			eat (Type.NUMBER);
			return new AstNumber (tok.s);
		}

		AST STR ()
		{
			var tok = curr_tok;
			eat (Type.STRING);
			return new AstString (tok.s);
		}

		AstIdent IDENT ()
		{
			var tok = curr_tok;
			eat (Type.IDENTIFIER);
			return new AstIdent (tok.s);
		}

		AST TYPE ()
		{
			var tok = curr_tok;
			eat (Type.KEYWORD);
			return new AstType (tok.s);
		}

		AST OPND ()
		{
			var tok = curr_tok;
			switch (tok.t) {
			case Type.SEPARATOR:
				{
					eat ("(");
					var node = EXPR ();
					eat (")");
					return node;
				}
			case Type.NUMBER:
				return NUM ();
			case Type.STRING:
				return STR ();
			case Type.IDENTIFIER:
				return IDENT ();
			}
			throw new ParEx ("OPND", tok);
		}

		AST EXPR ()
		{
			var tok = curr_tok;
			if (tok.t == Type.OPERATOR) {
				eat (Type.OPERATOR);
				return new AstUnary (tok.s, OPND ());
			}
			var opnd = OPND ();
			tok = curr_tok;
			if (!istype (Type.OPERATOR))
				return opnd;
			eat (Type.OPERATOR);
			return new BinOp (opnd, tok.s, OPND ());
		}

		AST PRINT ()
		{
			eat ("print");
			AST toprint = EXPR ();
			return new AstPrint (toprint);
		}

		AST ASSERT ()
		{
			eat ("assert");
			eat ("(");
			AST cond = EXPR ();
			eat (")");
			return new AstAssert (cond);
		}

		AST VAR ()
		{
			var v = new AstVar ();
			eat ("var");
			v.ident = IDENT ().name;
			eat (":");
			v.type = TYPE ();
			if (istype (":=", Type.SEPARATOR)) {
				eat (":=", Type.SEPARATOR);
				v.value = EXPR ();
			}
			return v;
		}

		AST ASSIGN ()
		{
			var v = new AstAssign ();
			v.ident = IDENT ().name;
			eat (":=", Type.SEPARATOR);
			v.value = EXPR ();
			return v;
		}

		AST READ ()
		{
			eat ("read");
			return new AstRead (IDENT ().name);
		}

		AST FORLOOP ()
		{
			var v = new AstForLoop ();
			eat ("for");
			v.ident = IDENT ().name;
			eat ("in");
			v.begin = EXPR ();
			eat ("..", Type.SEPARATOR);
			v.end = EXPR ();
			eat ("do");
			v.stmts = STMTS ();
			eat ("end");
			eat ("for");
			return v;
		}

		AST STMT ()
		{
			var tok = curr_tok;
			if (tok.t == Type.KEYWORD) {
				switch (tok.s) {
				case "read":
					return  READ ();
				case "assert":
					return  ASSERT ();
				case "print":
					return PRINT ();
				case "var":
					return VAR ();
				case "for":
					return FORLOOP ();
				}
			}
			if (tok.t == Type.IDENTIFIER)
				return ASSIGN ();
			throw new ParEx ("STMT", tok);
		}

		AST STMTS ()
		{
			var stmts = new AstStmts ();
			stmts.stmts.Add (STMT ());
			eat (";", Type.SEPARATOR);
			while (istype (Type.KEYWORD)) {
				if (istype ("end"))
					break;
				stmts.stmts.Add (STMT ());
				eat (";", Type.SEPARATOR);
			}
			return stmts;
		}

		public AST Parse ()
		{
			next ();
			next ();
			return STMTS ();
		}

		Lexer lexer;
		Lexeme curr_tok = null;
		Lexeme next_tok = null;
	}
}

