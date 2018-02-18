using System;
using System.Collections.Generic;

namespace compilers1
{
	public abstract class Visitor
	{
		public Visitor (AST ast, IO io)
		{
			this.io = io;
			this.ast = ast;
		}
		public AST visit (AST v)
		{
			return visitors [v.t] (v);
		}
		public delegate AST visitor (AST ast);
		public abstract void visit ();
		public bool errored { get; protected set; } = false;
		protected AST ast;
		protected Dictionary<string /*ident*/, AST> symbols = new Dictionary<string, AST> ();
		protected Dictionary<ASTType, visitor> visitors = new Dictionary<ASTType, visitor> ();
		protected IO io { get; }
	}
}

