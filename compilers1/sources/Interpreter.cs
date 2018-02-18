using System;
using System.Collections.Generic;

namespace compilers1
{
	public class Interpreter : Visitor
	{
		class IntEx : Exception
		{
			public IntEx (string message) : base (message)
			{
			}
		}

		T As<T> (AST v) where T : AST
		{
			var x = v as T;
			if (x != null)
				return x;
			throw new IntEx (String.Format ("expected type {0}, got {1}", typeof(T).Name, v.t));
		}

		public Interpreter (AST ast, IO io) : base(ast, io)
		{
			this.visitors.Add (ASTType.NUMBER, x => x);
			this.visitors.Add (ASTType.STRING, x => x);
			this.visitors.Add (ASTType.BOOLEAN, x => x);
			this.visitors.Add (ASTType.TYPENAME, x => x);
			this.visitors.Add (ASTType.UNARYOP, x => {
				var op = x as AstUnaryOperator;
				var opv = visit (op.v) as AstBool;
				switch (op.op) {
				case "!":
					return new AstBool (!opv.v);
				}
				throw new IntEx ("unrecognized unary operator");
			});
			this.visitors.Add (ASTType.EXPR, x => {
				var exp = x as AstExpr;
				var opl = visit (exp.lopnd);
				var rtail = exp.rtail as BinaryOperator;
				if (rtail == null)
					return opl;
				var opr = visit (rtail.r);
				if (opl.t == ASTType.NUMBER && opr.t == ASTType.NUMBER) {
					var l = opl as AstNumber;
					var r = opr as AstNumber;
					switch (rtail.op) {
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
					switch (rtail.op) {
					case "+":
						return new AstString (l.v + r.v);
					case "=":
						return new AstBool (l.v == r.v);
					case "<":
						return new AstBool (string.Compare (l.v, r.v) < 0);
					}
				}
				if (opl.t == ASTType.BOOLEAN && opr.t == ASTType.BOOLEAN) {
					var l = opl as AstBool;
					var r = opr as AstBool;
					switch (rtail.op) {
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
				io.Write ("{0}", v.Value ());
				return null;
			});
			this.visitors.Add (ASTType.STATEMENTS, x => {
				var v = x as AstStatements;
				this.visit (v.stmt);
				if (v.stmttail != null)
					this.visit (v.stmttail);
				return null;
			});
			this.visitors.Add (ASTType.ASSERT, x => {
				var f = x as AstAssert;
				var c = this.visit (f.cond) as AstBool;
				if (!c.v)
					io.WriteLine ("Assertion failed");
				return null;
			});
			this.visitors.Add (ASTType.READ, x => {
				var f = x as AstRead;
				var ident = As<AstIdentifier>(f.ident);
				switch (symbols [ident.name].t) {
				case ASTType.NUMBER:
					while (true) {
						try {
							symbols [ident.name] = new AstNumber (read ());
							break;
						} catch (System.FormatException /*e*/) {
							// Occurs when user inputs non number
							// Do nothing
						}
					}
					break;
				case ASTType.STRING:
					symbols [ident.name] = new AstString (read ());
					break;
				}
				return null;
			});
			this.visitors.Add (ASTType.IDENTIFIER, x => {
				var v = x as AstIdentifier;
				return visit (symbols [v.name]);
			});
			this.visitors.Add (ASTType.VARIABLE, x => {
				var v = x as AstVariable;
				var value = v.value;
				if (value == null) {
					var type = visit (v.type) as AstTypename;
					switch (type.name) {
					case "int":
						value = new AstNumber (0);
						break;
					case "string":
						value = new AstString ("");
						break;
					case "bool":
						value = new AstBool (false);
						break;
					default:
						throw new IntEx ("unknown identifier type");
					}
				}
				var ident = As<AstIdentifier>(v.ident);
				symbols [ident.name] = visit (value);
				return null;
			});
			this.visitors.Add (ASTType.FORLOOP, x => {
				var v = x as AstForLoop;
				var ident = As<AstIdentifier>(v.ident);
				var begin = visit (v.begin) as AstNumber;
				var end = visit (v.end) as AstNumber;
				int i = begin.v;
				for (; i <= end.v; ++i) {
					symbols [ident.name] = new AstNumber (i);
					visit (v.stmts);
				}
				symbols [ident.name] = new AstNumber (i);
				return null;
			});
			this.visitors.Add (ASTType.ASSIGN, x => {
				var v = x as AstAssign;
				var ident = As<AstIdentifier>(v.ident);
				symbols [ident.name] = visit (v.value);
				return null;
			});
		}

		public override void visit ()
		{
			visit (ast);
		}

		string read ()
		{
			int c = io.Read ();
			string s = "";
			while (c >= 0 && !Char.IsWhiteSpace ((char)c)) {
				s += (char)c;
				c = io.Read ();
			}
			return s;
		}
	}
}

