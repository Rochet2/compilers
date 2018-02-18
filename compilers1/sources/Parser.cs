using System;
using System.Collections.Generic;

namespace compilers1
{
	public class Parser
	{
		class ParEx : Exception
		{
			public ParEx (string message, Lexeme lexeme) : base (message)
			{
				this.lexeme = lexeme;
			}

			public Lexeme lexeme { get; }
		}

		public Parser (Lexer lexer, IO io)
		{
			this.lexer = lexer;
			this.io = io;
		}

		void next ()
		{
			curr_tok = lexer.next ();
		}

		Lexeme eat (TokenType t)
		{
			var v = curr_tok;
			if (v == null)
				throw new ParEx (String.Format ("expected token of type {0}", t), null);
			if (v.t != t)
				throw new ParEx (String.Format ("expected token of type {0}, got {1}", t, v == null ? "null" : v.ToString ()), v);
			next ();
			return v;
		}

		Lexeme eat (string s, TokenType t)
		{
			var v = curr_tok;
			if (v == null)
				throw new ParEx (String.Format ("expected token {{{0}, \"{1}\"}}", t, s), null);
			if (v.s != s || v.t != t)
				throw new ParEx (String.Format ("expected token {{{0}, \"{1}\"}}, got {2}", t, s, v == null ? "null" : v.ToString ()), v);
			next ();
			return v;
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
			var v = new AstNumber (curr_tok.s);
			v.tok = eat (TokenType.NUMBER);
			return v;
		}

		AST STR ()
		{
			var v = new AstString (curr_tok.s);
			v.tok = eat (TokenType.STRING);
			return v;
		}

		AstIdentifier IDENT ()
		{
			var v = new AstIdentifier (curr_tok.s);
			v.tok = eat (TokenType.IDENTIFIER);
			return v;
		}

		AST TYPE ()
		{
			var v = new AstTypename (curr_tok.s);
			v.tok = eat (TokenType.KEYWORD);
			return v;
		}

		AST UNARY ()
		{
			var v = new AstUnaryOperator ();
			v.tok = eat (TokenType.OPERATOR);
			v.op = v.tok.s;
			v.v = OPND ();
			return v;
		}

		AST BINOP ()
		{
			var v = new BinaryOperator ();
			v.tok = eat (TokenType.OPERATOR);
			v.op = v.tok.s;
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
			throw new ParEx ("operand expected", curr_tok);
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
			v.tok = curr_tok;
			v.lopnd = OPND ();
			v.rtail = EXPRTAIL ();
			return v;
		}

		AST PRINT ()
		{
			var v = new AstPrint ();
			v.tok = eat ("print", TokenType.KEYWORD);
			v.toprint = EXPR ();
			return v;
		}

		AST ASSERT ()
		{
			var v = new AstAssert ();
			v.tok = eat ("assert", TokenType.KEYWORD);
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
			var v = new AstDefinition ();
			v.tok = eat ("var", TokenType.KEYWORD);
			v.ident = IDENT ();
			eat (":", TokenType.SEPARATOR);
			v.type = TYPE ();
			v.value = VARTAIL ();
			return v;
		}

		AST ASSIGN ()
		{
			var v = new AstAssign ();
			v.ident = IDENT ();
			v.tok = eat (":=", TokenType.SEPARATOR);
			v.value = EXPR ();
			return v;
		}

		AST READ ()
		{
			var v = new AstRead ();
			v.tok = eat ("read", TokenType.KEYWORD);
			v.ident = IDENT ();
			return v;
		}

		AST FORLOOP ()
		{
			var v = new AstForLoop ();
			v.tok = eat ("for", TokenType.KEYWORD);
			v.ident = IDENT ();
			eat ("in", TokenType.KEYWORD);
			v.begin = EXPR ();
			eat ("..", TokenType.SEPARATOR);
			v.end = EXPR ();
			eat ("do", TokenType.KEYWORD);
			v.stmts = STMTS_FORLOOP ();
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
			throw new ParEx (String.Format ("statement expected"), curr_tok);
		}

		AST STMTSTAIL_FORLOOP ()
		{
			var v = new AstStatements ();
			if (curr_tok == null)
				return null; // no more statements
			v.stmt = STMT ();
			if (v.stmt == null)
				return null; // end for
			eat (";", TokenType.SEPARATOR);
			v.stmttail = STMTSTAIL_FORLOOP ();
			return v;
		}

		AST STMTS_FORLOOP ()
		{
			var v = new AstStatements ();
			v.stmt = STMT ();
			if (v.stmt == null)
				throw new ParEx (String.Format ("statement expected"), curr_tok);
			eat (";", TokenType.SEPARATOR);
			v.stmttail = STMTSTAIL_FORLOOP ();
			return v;
		}

		AST STMTSTAIL ()
		{
			var v = new AstStatements ();
			if (curr_tok == null)
				return null; // no more statements
			v.stmt = STMT ();
			if (v.stmt == null)
				throw new ParEx (String.Format ("statement expected"), curr_tok);
			eat (";", TokenType.SEPARATOR);
			v.stmttail = STMTSTAIL ();
			return v;
		}

		AST STMTS ()
		{
			var v = new AstStatements ();
			v.stmt = STMT ();
			if (v.stmt == null)
				throw new ParEx (String.Format ("statement expected"), curr_tok);
			eat (";", TokenType.SEPARATOR);
			v.stmttail = STMTSTAIL ();
			return v;
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
				error (e);
				ERRORTAIL ();
			}
		}

		public AST Parse ()
		{
			try {
				return PROG ();
			} catch (ParEx e) {
				errored = true;
				error (e);
				ERRORTAIL ();
			}
			return null;
		}

		void error (ParEx e)
		{
			errored = true;
			if (e.lexeme == null)
				io.WriteLine ("Parser error at <end of file>: {0}", e.Message);
			else
				io.WriteLine ("Parser error at {0}: {1}", e.lexeme.pos, e.Message);
			skip_to_next_stmt ();
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

