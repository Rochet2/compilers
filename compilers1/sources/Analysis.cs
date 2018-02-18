using System;
using System.Collections.Generic;

namespace compilers1
{
	public class Analysis : Visitor
	{
		class SemEx : Exception
		{
			public SemEx (string message, AST node) : base (message)
			{
				this.node = node;
			}

			public readonly AST node;
		}

		T As<T> (AST v, ASTType expectedtype, AST errnode = null) where T : AST
		{
			var x = v as T;
			if (x != null)
				return x;
			throw new SemEx (String.Format ("expected type {0}, got {1}", expectedtype, v.t), errnode ?? v);
		}

		void Expect<T> (AST v, ASTType expectedtype, AST errnode = null) where T : AST
		{
			var x = v as T;
			if (x != null)
				return;
			throw new SemEx (String.Format ("expected type {0}, got {1}", expectedtype, v.t), errnode ?? v);
		}

		void ExpectNull (AST x, AST errnode = null)
		{
			if (x == null)
				return;
			throw new SemEx (String.Format ("return value expected to be null, got {0}", x.t), errnode ?? x);
		}

		AST ExpectNotNull (AST x, AST errnode = null)
		{
			if (x != null)
				return x;
			throw new SemEx ("return value expected not to be null", errnode ?? x);
		}

		bool Is<T> (AST v) where T : AST
		{
			var x = v as T;
			if (x != null)
				return true;
			return false;
		}

		AST GetVar (AstIdentifier ident, AST errnode = null)
		{
			if (!symbols.ContainsKey (ident.name))
				throw new SemEx (String.Format ("using undefined identifier {0}", ident.name), errnode ?? ident);
			return symbols [ident.name];
		}

		public Analysis (AST ast, IO io) : base(ast, io)
		{
			this.visitors.Add (ASTType.NUMBER, x => x);
			this.visitors.Add (ASTType.STRING, x => x);
			this.visitors.Add (ASTType.BOOLEAN, x => x);
			this.visitors.Add (ASTType.TYPENAME, x => x);
			this.visitors.Add (ASTType.UNARYOP, x => {
				var op = As<AstUnaryOperator> (x, ASTType.UNARYOP);
				var v = ExpectNotNull (visit (op.v), op.v);
				if (Is<AstBool> (v)) {
					switch (op.op) {
					case "!":
						return new AstBool (false);
					}
					throw new SemEx (String.Format ("unrecognized boolean unary operator {0}", op.op), op);
				}
				throw new SemEx (String.Format ("unrecognized unary operator {0} for operand type {1}", op.op, v.t), op);
			});
			this.visitors.Add (ASTType.EXPR, x => {
				var exp = As<AstExpr> (x, ASTType.EXPR);
				if (exp.rtail == null)
					return ExpectNotNull (visit (exp.lopnd), exp.lopnd);
				var rtail = As<BinaryOperator> (exp.rtail, ASTType.BINARYOP);
				var opl = ExpectNotNull (visit (exp.lopnd),exp.lopnd);
				var opr = ExpectNotNull (visit (rtail.r), rtail.r);
				if (Is<AstNumber> (opl) && Is<AstNumber> (opr)) {
					switch (rtail.op) {
					case "+":
					case "-":
					case "*":
					case "/":
						return new AstNumber (0);
					case "=":
					case "<":
						return new AstBool (false);
					}
					throw new SemEx (String.Format ("unknown integer binary operator {0}", rtail.op), rtail);
				}
				if (Is<AstString> (opl) && Is<AstString> (opr)) {
					switch (rtail.op) {
					case "+":
						return new AstString ("");
					case "=":
					case "<":
						return new AstBool (false);
					}
					throw new SemEx (String.Format ("unknown string binary operator {0}", rtail.op), rtail);
				}
				if (Is<AstBool> (opl) && Is<AstBool> (opr)) {
					switch (rtail.op) {
					case "&":
					case "=":
					case "<":
						return new AstBool (false);
					}
					throw new SemEx (String.Format ("unknown boolean binary operator {0}", rtail.op), rtail);
				}
				throw new SemEx (String.Format ("unknown binary operator {0} for operand types left: {1}, right: {2}", rtail.op, visit (opl).t, visit (opr).t), rtail);
			});
			this.visitors.Add (ASTType.PRINT, x => {
				var f = As<AstPrint> (x, ASTType.PRINT);
				Expect<Printable> (visit (f.toprint), ASTType.PRINTABLE, f.toprint);
				return null;
			});
			this.visitors.Add (ASTType.STATEMENTS, x => {
				var v = As<AstStatements> (x, ASTType.STATEMENTS);
				try {
					ExpectNull (visit (v.stmt), v.stmt);
				} catch (SemEx e) {
					error (e);
				}
				if (v.stmttail != null) {
					try {
						ExpectNull (visit (v.stmttail), v.stmttail);
					} catch (SemEx e) {
						error (e);
					}
				}
				return null;
			});
			this.visitors.Add (ASTType.ASSERT, x => {
				var f = As<AstAssert> (x, ASTType.ASSERT);
				Expect<AstBool> (visit (f.cond), ASTType.BOOLEAN, f.cond);
				return null;
			});
			this.visitors.Add (ASTType.READ, x => {
				var f = As<AstRead> (x, ASTType.READ);
				var ident = As<AstIdentifier> (f.ident, ASTType.IDENTIFIER);
				var var = GetVar (ident);
				var type = var.t;
				switch (type) {
				case ASTType.NUMBER:
					symbols [ident.name] = new AstNumber (0);
					break;
				case ASTType.STRING:
					symbols [ident.name] = new AstString ("");
					break;
				default:
					throw new SemEx (String.Format ("variable {0} has unsupported type {1} to read from input", ident.name, type), f.ident);
				}
				return null;
			});
			this.visitors.Add (ASTType.IDENTIFIER, x => {
				var v = As<AstIdentifier> (x, ASTType.IDENTIFIER);
				return GetVar (v);
			});
			this.visitors.Add (ASTType.VARIABLE, x => {
				var v = As<AstVariable> (x, ASTType.VARIABLE);
				var type = As<AstTypename> (v.type, ASTType.TYPENAME);
				var ident = As<AstIdentifier> (v.ident, ASTType.IDENTIFIER);
				ASTType typeasttype;
				switch (type.name) {
				case "int":
					typeasttype = ASTType.NUMBER;
					break;
				case "string":
					typeasttype = ASTType.STRING;
					break;
				case "bool":
					typeasttype = ASTType.BOOLEAN;
					break;
				default:
					throw new SemEx (String.Format ("unknown identifier type name {0}", type.name), v.type);
				}
				if (symbols.ContainsKey (ident.name))
					throw new SemEx (String.Format ("variable {0} already defined", ident.name), v.ident);
				if (v.value == null) {
					switch (type.name) {
					case "int":
						symbols [ident.name] = new AstNumber (0);
						break;
					case "string":
						symbols [ident.name] = new AstString ("");
						break;
					case "bool":
						symbols [ident.name] = new AstBool (false);
						break;
					}
				} else {
					var value = visit (v.value);
					if (value == null || value.t != typeasttype)
						throw new SemEx (String.Format ("variable {0} type {1} does not match value type {2}", ident.name, typeasttype, value == null ? "null" : value.t.ToString ()), ident);
					symbols [ident.name] = visit (value);
				}
				return null;
			});
			this.visitors.Add (ASTType.FORLOOP, x => {
				var v = As<AstForLoop> (x, ASTType.FORLOOP);
				var ident = As<AstIdentifier> (v.ident, ASTType.IDENTIFIER);
				Expect<AstNumber> (GetVar (ident), ASTType.NUMBER, v.ident);
				Expect<AstNumber> (visit (v.begin), ASTType.NUMBER, v.begin);
				Expect<AstNumber> (visit (v.end), ASTType.NUMBER, v.end);
				ExpectNull (visit (v.stmts), v.stmts);
				return null;
			});
			this.visitors.Add (ASTType.ASSIGN, x => {
				var v = As<AstAssign> (x, ASTType.ASSIGN);
				var ident = As<AstIdentifier> (v.ident, ASTType.IDENTIFIER);
				AST o = GetVar (ident);
				var value = visit (v.value);
				if (value == null || value.t != o.t)
					throw new SemEx (String.Format ("variable {0} type {1} does not match value type {2}", ident.name, o.t, value == null ? "null" : value.t.ToString ()), x);
				symbols [ident.name] = value;
				return null;
			});
		}

		public override void visit ()
		{
			try {
				ExpectNull (visit (ast));
			} catch (SemEx e) {
				error (e);
			}
		}

		void error (SemEx e)
		{
			errored = true;
			if (e.node == null || e.node.tok == null)
				io.WriteLine ("Semantic error at ?: {0}", e.Message);
			else
				io.WriteLine ("Semantic error at {0}: {1}", e.node.tok.pos, e.Message);
		}
	}
}
