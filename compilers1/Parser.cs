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
			curr_tok = lexer.next ();
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

		AST UNARY ()
		{
			var v = new AstUnary ();
			var tok = curr_tok;
			eat (Type.OPERATOR);
			v.op = tok.s;
			v.v = OPND ();
			return v;
		}

		AST BINOP ()
		{
			var v = new BinOp ();
			var tok = curr_tok;
			eat (Type.OPERATOR);
			v.op = tok.s;
			v.r = OPND ();
			return v;
		}

		AST OPND ()
		{
			switch (curr_tok.t) {
			case Type.SEPARATOR:
				{
					eat ("(", Type.SEPARATOR);
					var node = EXPR ();
					eat (")", Type.SEPARATOR);
					return node;
				}
			case Type.NUMBER:
				return NUM ();
			case Type.STRING:
				return STR ();
			case Type.IDENTIFIER:
				return IDENT ();
			}
			throw new ParEx ("OPND", curr_tok);
		}

		AST EXPRTAIL ()
		{
			if (istype (Type.OPERATOR))
				return BINOP ();
			return null;
		}

		AST EXPR ()
		{
			if (istype (Type.OPERATOR))
				return UNARY ();
			var v = new AstExpr ();
			v.lopnd = OPND ();
			v.rtail = EXPRTAIL ();
			return v;
		}

		AST PRINT ()
		{
			eat ("print", Type.KEYWORD);
			return new AstPrint (EXPR ());
		}

		AST ASSERT ()
		{
			var v = new AstAssert ();
			eat ("assert", Type.KEYWORD);
			eat ("(", Type.SEPARATOR);
			v.cond = EXPR ();
			eat (")", Type.SEPARATOR);
			return v;
		}

		AST VARTAIL ()
		{
			if (istype (":=", Type.SEPARATOR)) {
				eat (":=", Type.SEPARATOR);
				return EXPR ();
			}
			return null;
		}

		AST VAR ()
		{
			var v = new AstVar ();
			eat ("var", Type.KEYWORD);
			v.ident = IDENT ().name;
			eat (":", Type.SEPARATOR);
			v.type = TYPE ();
			v.value = VARTAIL ();
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
			eat ("read", Type.KEYWORD);
			return new AstRead (IDENT ().name);
		}

		AST FORLOOP ()
		{
			var v = new AstForLoop ();
			eat ("for", Type.KEYWORD);
			v.ident = IDENT ().name;
			eat ("in", Type.KEYWORD);
			v.begin = EXPR ();
			eat ("..", Type.SEPARATOR);
			v.end = EXPR ();
			eat ("do", Type.KEYWORD);
			v.stmts = STMTS ();
			eat ("end", Type.KEYWORD);
			eat ("for", Type.KEYWORD);
			return v;
		}

		AST STMT ()
		{
			if (istype (Type.KEYWORD)) {
				switch (curr_tok.s) {
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
				case "end":
					return null;
				}
			}
			if (istype (Type.IDENTIFIER))
				return ASSIGN ();
			throw new ParEx ("STMT", curr_tok);
		}

		AST STMTSTAIL ()
		{
			if (curr_tok == null)
				return null;
			return STMTS ();
		}

		AST STMTS ()
		{
			var stmts = new AstStmts ();
			stmts.stmt = STMT ();

			if (stmts.stmt == null)
				return null;

			eat (";", Type.SEPARATOR);
			stmts.stmttail = STMTSTAIL ();
			return stmts;
		}

		public AST Parse ()
		{
			next (); // get first token
			AST ast = STMTS ();
			if (curr_tok != null)
				throw new ParEx ("unexpected token after program end", curr_tok);
			return ast;
		}

		Lexer lexer;
		Lexeme curr_tok = null;
	}
}

