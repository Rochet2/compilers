using System;
using System.Collections.Generic;

namespace compilers1
{
	class IntEx : Exception
	{
		public IntEx (string message) : base (message)
		{
		}
	}

	delegate AST visitor (AST ast);

	public class Interpreter
	{
		public Interpreter (AST ast)
		{
			this.ast = ast;
			this.visitors.Add (ASTType.NUMBER, x => x);
			this.visitors.Add (ASTType.STRING, x => x);
			this.visitors.Add (ASTType.BOOLEAN, x => x);
			this.visitors.Add (ASTType.TYPE, x => x);
			this.visitors.Add (ASTType.UOP, x => {
				var op = x as AstUnary;
				var opv = visit(op.v) as AstBool;
				switch (op.op) {
				case "!":
					return new AstBool(!opv.v);
				}
				throw new IntEx ("unrecognized unary operator");
			});
			this.visitors.Add (ASTType.BINOP, x => {
				var op = x as BinOp;
				var opl = visit(op.l);
				var opr = visit(op.r);
				if (opl.t == ASTType.NUMBER && opr.t == ASTType.NUMBER) {
					var l = opl as AstNumber;
					var r = opr as AstNumber;
					switch (op.op) {
					case "+":
						return new AstNumber (l.v + r.v);
					case "-":
						return new AstNumber (l.v - r.v);
					case "*":
						return new AstNumber (l.v * r.v);
					case "/":
						return new AstNumber (l.v / r.v);
					case "=":
						return new AstBool (l.v == r.v);
					case "<":
						return new AstBool (l.v < r.v);
					}
				}
				if (opl.t == ASTType.STRING && opr.t == ASTType.STRING) {
					var l = opl as AstString;
					var r = opr as AstString;
					switch (op.op) {
					case "+":
						return new AstString (l.v + r.v);
					case "=":
						return new AstBool (l.v == r.v);
					case "<":
						return new AstBool (string.Compare(l.v, r.v) < 0);
					}
				}
				if (opl.t == ASTType.BOOLEAN && opr.t == ASTType.BOOLEAN) {
					var l = opl as AstBool;
					var r = opr as AstBool;
					switch (op.op) {
					case "&":
						return new AstBool (l.v && r.v);
					case "=":
						return new AstBool (l.v == r.v);
					case "<":
						return new AstBool (!l.v && r.v);
					}
				}
				throw new IntEx ("unrecognized binary operator");
			});
			this.visitors.Add (ASTType.PRINT, x => {
				var f = x as AstPrint;
				var v = this.visit (f.toprint) as Printable;
				Console.Write ("{0}", v.Value ());
				return null;
			});
			this.visitors.Add (ASTType.STMTS, x => {
				var v = x as AstStmts;
				foreach (AST e in v.stmts)
					this.visit (e);
				return null;
			});
			this.visitors.Add (ASTType.ASSERT, x => {
				var f = x as AstAssert;
				var c = this.visit(f.cond) as AstBool;
				if (!c.v)
					Console.WriteLine("Assertion failed");
				return null;
			});
			this.visitors.Add (ASTType.READ, x => {
				var f = x as AstRead;
				switch(symbols[f.ident].t) {
				case ASTType.NUMBER:
					symbols[f.ident] = new AstNumber(read());
					break;
				case ASTType.STRING:
					symbols[f.ident] = new AstString(read());
					break;
				}
				return null;
			});
			this.visitors.Add (ASTType.IDENTIFIER, x => {
				var v = x as AstIdent;
				return visit(symbols[v.name]);
			});
			this.visitors.Add (ASTType.VAR, x => {
				var v = x as AstVar;
				var value = v.value;
				if (value == null) {
				var type = visit(v.type) as AstType;
					switch (type.name) {
					case "int":
						value = new AstNumber(0);
						break;
					case "string":
						value = new AstString("");
						break;
					case "bool":
						value = new AstBool(false);
						break;
					default:
						throw new IntEx("unknown identifier type");
					}
				}
				symbols[v.ident] = visit(value);
				return null;
			});
			this.visitors.Add (ASTType.FORLOOP, x => {
				var v = x as AstForLoop;
				var begin = visit(v.begin) as AstNumber;
				var end = visit(v.end) as AstNumber;
				int i = begin.v;
				for (; i <= end.v; ++i) {
					symbols[v.ident] = new AstNumber(i);
					visit(v.stmts);
				}
				symbols[v.ident] = new AstNumber(i);
				return null;
			});
			this.visitors.Add (ASTType.ASSIGN, x => {
				var v = x as AstAssign;
				symbols[v.ident] = visit(v.value);
				return null;
			});
		}

		public AST visit (AST v)
		{
			if (!visitors.ContainsKey (v.t))
				throw new IntEx ("unknown AST type");
			return visitors [v.t] (v);
		}

		public void visit ()
		{
			visit (ast);
		}

		string read() {
			int c = Console.Read ();
			string s = "";
			while (c >= 0 && !Char.IsWhiteSpace ((char)c)) {
				s += (char)c;
				c = Console.Read ();
			}
			return s;
		}

		AST ast;
		Dictionary<string /*ident*/, AST> symbols = new Dictionary<string, AST>();
		Dictionary<ASTType, visitor> visitors = new Dictionary<ASTType, visitor> ();
	}
}

