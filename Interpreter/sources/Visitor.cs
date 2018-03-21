using System;
using System.Collections.Generic;

namespace Interpreter
{
    /*
     * An abstract Visitor class for walking an AST tree.
     * All visitors inherit this class.
     * The class is equipped with methods for walking, checking types and
     * asserting.
     */
    public abstract class Visitor
    {
        /*
         * Initializes the Visitor with the name of the visitor, AST root node and IO.
         * The name of the visitor is used in error messages.
         */
        public Visitor(string name, ASTNode ast, IO io)
        {
            this.name = name;
            this.io = io;
            this.ast = ast;
        }

        /*
         * Visits an ASTNode by calling the visitor function of it's type
         */
        public ASTNode Visit(ASTNode node)
        {
            return visitorFunctions[node.type](node);
        }

        /*
         * Converts convertedNode to the type T and returns the result.
         * If conversion fails then throws a VisitorException.
         * The exception contains an error message and attaches errorNode or
         * if not given then convertedNode as the source of the error.
         */
        public T As<T>(ASTNode convertedNode, ASTNodeType expectedtype, ASTNode errorNode = null) where T : ASTNode
        {
            var converted = convertedNode as T;
            if (converted != null)
                return converted;
            throw new VisitorException(string.Format("expected type {0}, got {1}", expectedtype, convertedNode.type), errorNode ?? convertedNode);
        }

        /*
         * Asserts node to be of type T.
         * If conversion fails then throws a VisitorException.
         * The exception contains an error message and attaches errorNode or
         * if not given then node as the source of the error.
         */
        public void Expect<T>(ASTNode node, ASTNodeType expectedtype, ASTNode errorNode = null) where T : ASTNode
        {
            var converted = node as T;
            if (converted != null)
                return;
            throw new VisitorException(string.Format("expected type {0}, got {1}", expectedtype, node.type), errorNode ?? node);
        }

        /*
         * Asserts node to be null.
         * Otherwise throws a VisitorException.
         * The exception contains an error message and attaches errorNode or
         * if not given then node as the source of the error.
         */
        public void ExpectNull(ASTNode node, ASTNode errorNode = null)
        {
            if (node == null)
                return;
            throw new VisitorException(string.Format("return value expected to be null, got {0}", node.type), errorNode ?? node);
        }

        /*
         * Asserts node not to be null and returns the node
         * Otherwise throws a VisitorException.
         * The exception contains an error message and attaches errorNode or
         * if not given then node as the source of the error.
         */
        public ASTNode ExpectNotNull(ASTNode node, ASTNode errorNode = null)
        {
            if (node != null)
                return node;
            throw new VisitorException("return value expected not to be null", errorNode ?? node);
        }

        /*
         * Returns true if the node is of the given type T.
         * Returns false otherwise.
         */
        public bool Is<T>(ASTNode node) where T : ASTNode
        {
            var converted = node as T;
            if (converted != null)
                return true;
            return false;
        }

        /*
         * Returns the current Variable attached to the given identifier.
         * If variable not found throws a VisitorException with an
         * error message and errorNode or ident as source of the error.
         */
        public Variable GetVar(Identifier identifier, ASTNode errorNode = null)
        {
            if (!variables.ContainsKey(identifier.name))
                throw new VisitorException(string.Format("using undefined identifier {0}", identifier.name), errorNode ?? identifier);
            return variables[identifier.name];
        }

        /*
         * Returns the current Variable attached to the given identifier.
         * Asserts that the Variable is mutable and throws VisitorException otherwise.
         * If variable not found throws a VisitorException with an
         * error message and errorNode or ident as source of the error.
         */
        public Variable ExpectMutable(Identifier identifier, ASTNode errorNode = null)
        {
            var variable = GetVar(identifier, errorNode);
            if (!variable.immutable)
                return variable;
            throw new VisitorException(string.Format("trying to change immutable variable {0}", identifier.name), errorNode ?? identifier);
        }

        /*
         * Handles a VisitorException.
         * Sets error indicator to true,
         * prints the error message.
         */
        public void PrintError(VisitorException e)
        {
            errored = true;
            if (e.node == null || e.node.lexeme == null)
                io.WriteLine("{0} error at ?: {1}", name, e.Message);
            else
                io.WriteLine("{0} error at {1}: {2}", name, e.node.lexeme.position, e.Message);
        }

        /*
         * The main Visit function that begins by visiting the root node.
         * Prints any thrown VisitorException and stops visiting after printing.
         * Can be overridden.
         */
        public virtual void Visit()
        {
            try
            {
                ExpectNull(Visit(ast));
            }
            catch (VisitorException e)
            {
                PrintError(e);
            }
        }

        /*
         * A Variable class that represents a variable value.
         * Can define if the variable value is immutable.
         */
        public class Variable
        {
            public Variable(ASTVariable variable, bool immutable = false)
            {
                this.value = variable;
                this.immutable = immutable;
            }

            public readonly ASTVariable value;
            public bool immutable = false;
        }

        /*
         * An exception thrown by the Visitor on unexpected errors.
         * Contains the error message and the node that caused the error.
         */
        public class VisitorException : Exception
        {
            public VisitorException(string message, ASTNode node = null) : base(message)
            {
                this.node = node;
            }

            public readonly ASTNode node;
        }

        private string name; // Visitor name
        public delegate ASTNode VisitorFunction(ASTNode ast); // type of the Visit functions
        public bool errored { get; protected set; } = false;
        protected ASTNode ast;
        protected Dictionary<string /*identifier*/, Variable> variables = new Dictionary<string, Variable>();
        protected Dictionary<ASTNodeType, VisitorFunction> visitorFunctions = new Dictionary<ASTNodeType, VisitorFunction>();
        protected IO io { get; }
    }
}
