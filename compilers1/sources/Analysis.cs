using System;
using System.Collections.Generic;

namespace compilers1
{
	public class Analysis : Visitor
	{
		public Analysis (AST ast, IO io) : base ("Semantic analysis", ast, io)
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
					throw new VisitEx (String.Format ("unrecognized boolean unary operator {0}", op.op), op);
				}
				throw new VisitEx (String.Format ("unrecognized unary operator {0} for operand type {1}", op.op, v.t), op);
			});
			this.visitors.Add (ASTType.EXPRESSION, x => {
				var exp = As<AstExpr> (x, ASTType.EXPRESSION);
				if (exp.rtail == null)
					return ExpectNotNull (visit (exp.lopnd), exp.lopnd);
				var rtail = As<BinaryOperator> (exp.rtail, ASTType.BINARYOP);
				var opl = ExpectNotNull (visit (exp.lopnd), exp.lopnd);
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
					throw new VisitEx (String.Format ("unknown integer binary operator {0}", rtail.op), rtail);
				}
				if (Is<AstString> (opl) && Is<AstString> (opr)) {
					switch (rtail.op) {
					case "+":
						return new AstString ("");
					case "=":
					case "<":
						return new AstBool (false);
					}
					throw new VisitEx (String.Format ("unknown string binary operator {0}", rtail.op), rtail);
				}
				if (Is<AstBool> (opl) && Is<AstBool> (opr)) {
					switch (rtail.op) {
					case "&":
					case "=":
					case "<":
						return new AstBool (false);
					}
					throw new VisitEx (String.Format ("unknown boolean binary operator {0}", rtail.op), rtail);
				}
				throw new VisitEx (String.Format ("unknown binary operator {0} for operand types left: {1}, right: {2}", rtail.op, visit (opl).t, visit (opr).t), rtail);
			});
			this.visitors.Add (ASTType.PRINT, x => {
				var f = As<AstPrint> (x, ASTType.PRINT);
				Expect<AstVariable> (visit (f.toprint), ASTType.VARIABLE, f.toprint);
				return null;
			});
			this.visitors.Add (ASTType.STATEMENTS, x => {
				var v = As<AstStatements> (x, ASTType.STATEMENTS);
				try {
					ExpectNull (visit (v.stmt), v.stmt);
				} catch (VisitEx e) {
					PrintError (e);
				}
				if (v.stmttail != null) {
					try {
						ExpectNull (visit (v.stmttail), v.stmttail);
					} catch (VisitEx e) {
						PrintError (e);
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
				var variable = ExpectMutable (ident, f.ident);
				var type = variable.value.t;
				switch (type) {
				case ASTType.NUMBER:
					variables [ident.name] = new Variable (new AstNumber (0));
					break;
				case ASTType.STRING:
					variables [ident.name] = new Variable (new AstString (""));
					break;
				default:
					throw new VisitEx (String.Format ("variable {0} has unsupported type {1} to read from input", ident.name, type), f.ident);
				}
				return null;
			});
			this.visitors.Add (ASTType.IDENTIFIER, x => {
				var v = As<AstIdentifier> (x, ASTType.IDENTIFIER);
				return GetVar (v).value;
			});
			this.visitors.Add (ASTType.DEFINITION, x => {
				var v = As<AstDefinition> (x, ASTType.DEFINITION);
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
					throw new VisitEx (String.Format ("unknown identifier type name {0}", type.name), v.type);
				}
				if (variables.ContainsKey (ident.name))
					throw new VisitEx (String.Format ("variable {0} already defined", ident.name), v.ident);
				if (v.value == null) {
					switch (type.name) {
					case "int":
						variables [ident.name] = new Variable (new AstNumber (0));
						break;
					case "string":
						variables [ident.name] = new Variable (new AstString (""));
						break;
					case "bool":
						variables [ident.name] = new Variable (new AstBool (false));
						break;
					}
				} else {
					var value = visit (v.value);
					if (value == null || value.t != typeasttype)
						throw new VisitEx (String.Format ("variable {0} type {1} does not match value type {2}", ident.name, typeasttype, value == null ? "null" : value.t.ToString ()), ident);
					variables [ident.name] = new Variable (As<AstVariable>(value, ASTType.VARIABLE));
				}
				return null;
			});
			this.visitors.Add (ASTType.FORLOOP, x => {
				var v = As<AstForLoop> (x, ASTType.FORLOOP);
				var ident = As<AstIdentifier> (v.ident, ASTType.IDENTIFIER);
				var control = ExpectMutable (ident, v.ident);
				Expect<AstNumber> (control.value, ASTType.NUMBER, v.ident);
				Expect<AstNumber> (visit (v.begin), ASTType.NUMBER, v.begin);
				Expect<AstNumber> (visit (v.end), ASTType.NUMBER, v.end);
				control.immutable = true;
				ExpectNull (visit (v.stmts), v.stmts);
				control.immutable = false;
				return null;
			});
			this.visitors.Add (ASTType.ASSIGN, x => {
				var v = As<AstAssign> (x, ASTType.ASSIGN);
				var ident = As<AstIdentifier> (v.ident, ASTType.IDENTIFIER);
				var variable = ExpectMutable (ident, v.ident);
				var value = visit (v.value);
				if (value == null || value.t != variable.value.t)
					throw new VisitEx (String.Format ("variable {0} type {1} does not match value type {2}", ident.name, variable.value.t, value == null ? "null" : value.t.ToString ()), x);
				variables [ident.name] = new Variable (As<AstVariable>(value, ASTType.VARIABLE));
				return null;
			});
		}
	}
}
