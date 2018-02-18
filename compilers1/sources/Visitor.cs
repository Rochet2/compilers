using System;
using System.Collections.Generic;

namespace compilers1
{
	public abstract class Visitor
	{
		public Visitor (string name, AST ast, IO io)
		{
			this.name = name;
			this.io = io;
			this.ast = ast;
		}

		public AST visit (AST v)
		{
			return visitors [v.t] (v);
		}

		public T As<T> (AST v, ASTType expectedtype, AST errnode = null) where T : AST
		{
			var x = v as T;
			if (x != null)
				return x;
			throw new VisitEx (String.Format ("expected type {0}, got {1}", expectedtype, v.t), errnode ?? v);
		}

		public void Expect<T> (AST v, ASTType expectedtype, AST errnode = null) where T : AST
		{
			var x = v as T;
			if (x != null)
				return;
			throw new VisitEx (String.Format ("expected type {0}, got {1}", expectedtype, v.t), errnode ?? v);
		}

		public void ExpectNull (AST x, AST errnode = null)
		{
			if (x == null)
				return;
			throw new VisitEx (String.Format ("return value expected to be null, got {0}", x.t), errnode ?? x);
		}

		public AST ExpectNotNull (AST x, AST errnode = null)
		{
			if (x != null)
				return x;
			throw new VisitEx ("return value expected not to be null", errnode ?? x);
		}

		public bool Is<T> (AST v) where T : AST
		{
			var x = v as T;
			if (x != null)
				return true;
			return false;
		}

		public Variable GetVar (AstIdentifier ident, AST errnode = null)
		{
			if (!variables.ContainsKey (ident.name))
				throw new VisitEx (String.Format ("using undefined identifier {0}", ident.name), errnode ?? ident);
			return variables [ident.name];
		}

		public Variable ExpectMutable (AstIdentifier ident, AST errnode = null)
		{
			var v = GetVar (ident, errnode);
			if (!v.immutable)
				return v;
			throw new VisitEx(String.Format("trying to change immutable variable {0}", ident.name), errnode ?? ident);
		}

		public void PrintError (VisitEx e)
		{
			errored = true;
			if (e.node == null || e.node.tok == null)
				io.WriteLine ("{0} error at ?: {1}", name, e.Message);
			else
				io.WriteLine ("{0} error at {1}: {2}", name, e.node.tok.pos, e.Message);
		}

		private string name;

		public delegate AST visitor (AST ast);

		public virtual void visit ()
		{
			try {
				ExpectNull (visit (ast));
			} catch (VisitEx e) {
				PrintError (e);
			}
		}

		public bool errored { get; protected set; } = false;

		protected AST ast;
		protected Dictionary<string /*identifier*/, Variable> variables = new Dictionary<string, Variable> ();
		protected Dictionary<ASTType, visitor> visitors = new Dictionary<ASTType, visitor> ();

		protected IO io { get; }

		public class Variable
		{
			public Variable (AstVariable variable, bool immutable = false)
			{
				this.value = variable;
				this.immutable = immutable;
			}

			public readonly AstVariable value;
			public bool immutable = false;
		}

		public class VisitEx : Exception
		{
			public VisitEx (string message, AST node = null) : base (message)
			{
				this.node = node;
			}

			public readonly AST node;
		}
	}
}

