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

		public Lexeme lexeme { get; }
	}

	public class Parser
	{
		public Parser (Lexer lexer, IO io)
		{
			this.lexer = lexer;
			this.io = io;
		}

		void next ()
		{
			curr_tok = lexer.next ();
		}

		void eat (TokenType t)
		{
			if (curr_tok == null || curr_tok.t != t)
				throw new ParEx (String.Format ("expected token of type {0}, got {1}", t, curr_tok), curr_tok);
			next ();
		}

		void eat (string s, TokenType t)
		{
			if (curr_tok == null || curr_tok.s != s || curr_tok.t != t)
				throw new ParEx (String.Format ("expected token {{0}, \"{1}\"}, got {2}", t, s, curr_tok), curr_tok);
			next ();
		}

		bool istype (TokenType t)
		{
			return curr_tok != null && curr_tok.t == t;
		}

		bool istype (string s)
		{
			return curr_tok != null && curr_tok.s == s;
		}

		bool istype (string s, TokenType t)
		{
			return istype (s) && istype (t);
		}

		AST NUM ()
		{
			var tok = curr_tok;
			eat (TokenType.NUMBER);
			return new AstNumber (tok.s);
		}

		AST STR ()
		{
			var tok = curr_tok;
			eat (TokenType.STRING);
			return new AstString (tok.s);
		}

		AstIdentifier IDENT ()
		{
			var tok = curr_tok;
			eat (TokenType.IDENTIFIER);
			return new AstIdentifier (tok.s);
		}

		AST TYPE ()
		{
			var tok = curr_tok;
			eat (TokenType.KEYWORD);
			return new AstTypename (tok.s);
		}

		AST UNARY ()
		{
			var v = new AstUnaryOperator ();
			var tok = curr_tok;
			eat (TokenType.OPERATOR);
			v.op = tok.s;
			v.v = OPND ();
			return v;
		}

		AST BINOP ()
		{
			var v = new BinaryOperator ();
			var tok = curr_tok;
			eat (TokenType.OPERATOR);
			v.op = tok.s;
			v.r = OPND ();
			return v;
		}

		AST OPND ()
		{
			switch (curr_tok.t) {
			case TokenType.SEPARATOR:
				{
					eat ("(", TokenType.SEPARATOR);
					var node = EXPR ();
					eat (")", TokenType.SEPARATOR);
					return node;
				}
			case TokenType.NUMBER:
				return NUM ();
			case TokenType.STRING:
				return STR ();
			case TokenType.IDENTIFIER:
				return IDENT ();
			}
			throw new ParEx ("OPND", curr_tok);
		}

		AST EXPRTAIL ()
		{
			if (istype (TokenType.OPERATOR))
				return BINOP ();
			return null;
		}

		AST EXPR ()
		{
			if (istype (TokenType.OPERATOR))
				return UNARY ();
			var v = new AstExpr ();
			v.lopnd = OPND ();
			v.rtail = EXPRTAIL ();
			return v;
		}

		AST PRINT ()
		{
			eat ("print", TokenType.KEYWORD);
			return new AstPrint (EXPR ());
		}

		AST ASSERT ()
		{
			var v = new AstAssert ();
			eat ("assert", TokenType.KEYWORD);
			eat ("(", TokenType.SEPARATOR);
			v.cond = EXPR ();
			eat (")", TokenType.SEPARATOR);
			return v;
		}

		AST VARTAIL ()
		{
			if (istype (":=", TokenType.SEPARATOR)) {
				eat (":=", TokenType.SEPARATOR);
				return EXPR ();
			}
			return null;
		}

		AST VAR ()
		{
			var v = new AstVariable ();
			eat ("var", TokenType.KEYWORD);
			v.ident = IDENT ().name;
			eat (":", TokenType.SEPARATOR);
			v.type = TYPE ();
			v.value = VARTAIL ();
			return v;
		}

		AST ASSIGN ()
		{
			var v = new AstAssign ();
			v.ident = IDENT ().name;
			eat (":=", TokenType.SEPARATOR);
			v.value = EXPR ();
			return v;
		}

		AST READ ()
		{
			eat ("read", TokenType.KEYWORD);
			return new AstRead (IDENT ().name);
		}

		AST FORLOOP ()
		{
			var v = new AstForLoop ();
			eat ("for", TokenType.KEYWORD);
			v.ident = IDENT ().name;
			eat ("in", TokenType.KEYWORD);
			v.begin = EXPR ();
			eat ("..", TokenType.SEPARATOR);
			v.end = EXPR ();
			eat ("do", TokenType.KEYWORD);
			v.stmts = STMTS ();
			eat ("end", TokenType.KEYWORD);
			eat ("for", TokenType.KEYWORD);
			return v;
		}

		AST STMT ()
		{
			if (istype (TokenType.KEYWORD)) {
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
			if (istype (TokenType.IDENTIFIER))
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
			var stmts = new AstStatements ();
			stmts.stmt = STMT ();

			if (stmts.stmt == null)
				return null;

			eat (";", TokenType.SEPARATOR);
			stmts.stmttail = STMTSTAIL ();
			return stmts;
		}

		AST PROG ()
		{
			next (); // get first token
			AST ast = STMTS ();
			if (curr_tok != null)
				throw new ParEx ("unexpected token after program end", curr_tok);
			return ast;
		}

		void ERRORTAIL ()
		{
			try {
				STMTSTAIL ();
			} catch (ParEx e) {
				errored = true;
				io.WriteLine ("Parser error in {0} at {1}: {2}", lexer.input.name, e.lexeme.pos, e.Message);
				skip_to_next_stmt ();
				ERRORTAIL ();
			}
		}

		public AST Parse ()
		{
			try {
				return PROG ();
			} catch (ParEx e) {
				errored = true;
				io.WriteLine ("Parser error in {0} at {1}: {2}", lexer.input.name, e.lexeme.pos, e.Message);
				skip_to_next_stmt ();
				ERRORTAIL ();
			}
			return null;
		}

		void skip_to_next_stmt ()
		{
			while (curr_tok != null && !istype (";", TokenType.SEPARATOR))
				next ();
			next ();
		}

		Lexer lexer { get; }

		Lexeme curr_tok = null;

		public bool errored { get; private set; } = false;

		IO io { get; }
	}
}

