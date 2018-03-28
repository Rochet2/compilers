using System;
using System.Collections.Generic;

namespace Interpreter
{
    /*
     * An abstract visitor class for walking an AST tree.
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
        protected ASTNode Visit(ASTNode node)
        {
            if (node == null)
                throw new VisitorException("trying to visit a null node");
            return visitorFunctions[node.type](node);
        }

        /*
         * Converts convertedNode to the type T and returns the result.
         * If conversion fails then throws a VisitorException.
         * The exception contains an error message and attaches errorNode or
         * if not given then convertedNode as the source of the error.
         */
        protected T As<T>(ASTNode convertedNode, ASTNodeType expectedtype, ASTNode errorNode = null) where T : ASTNode
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
        protected void Expect<T>(ASTNode node, ASTNodeType expectedtype, ASTNode errorNode = null) where T : ASTNode
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
        protected void ExpectNull(ASTNode node, ASTNode errorNode = null)
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
        protected ASTNode ExpectNotNull(ASTNode node, ASTNode errorNode = null)
        {
            if (node != null)
                return node;
            throw new VisitorException("return value expected not to be null", errorNode ?? node);
        }

        /*
         * Returns true if the node is of the given type T.
         * Returns false otherwise.
         */
        protected bool Is<T>(ASTNode node) where T : ASTNode
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
        protected Variable GetVariable(IdentifierNode identifier, ASTNode errorNode = null)
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
        protected Variable ExpectMutable(IdentifierNode identifier, ASTNode errorNode = null)
        {
            var variable = GetVariable(identifier, errorNode);
            if (!variable.immutable)
                return variable;
            throw new VisitorException(string.Format("trying to change immutable variable {0}", identifier.name), errorNode ?? identifier);
        }

        /*
         * Handles a VisitorException.
         * Sets error indicator to true,
         * prints the error message.
         */
        protected void PrintError(VisitorException e)
        {
            errored = true;
            string errorMessage = string.Format("{0} error", name);
            if (e.node != null && e.node.lexeme != null)
            {
                errorMessage = string.Format(
                    "{0} at {1} token {2}",
                    errorMessage,
                    e.node.lexeme.position,
                    e.node.lexeme
                );
            }
            else
            {
                errorMessage = string.Format(
                    "{0} at {1}",
                    errorMessage,
                    "<runtime generated code>"
                );
            }
            errorMessage = string.Format(
                "{0} in node {1}",
                errorMessage,
                e.node.type
            );
            errorMessage = string.Format(
                "{0}:",
                errorMessage
            );
            io.WriteLine(errorMessage);
            io.WriteLine(e.Message);
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
        protected class Variable
        {
            public Variable(ASTVariableNode variable, bool immutable = false)
            {
                this.value = variable;
                this.immutable = immutable;
            }

            public readonly ASTVariableNode value;
            public bool immutable = false;
        }

        /*
         * An exception thrown by the Visitor on unexpected errors.
         * Contains the error message and the node that caused the error.
         */
        protected class VisitorException : Exception
        {
            public VisitorException(string message, ASTNode node = null) : base(message)
            {
                this.node = node;
            }

            public readonly ASTNode node;
        }

        private string name; // Visitor name
        protected delegate ASTNode VisitorFunction(ASTNode ast); // type of the Visit functions
        public bool errored { get; protected set; } = false;
        protected ASTNode ast;
        protected Dictionary<string /*identifier*/, Variable> variables = new Dictionary<string, Variable>();
        protected Dictionary<ASTNodeType, VisitorFunction> visitorFunctions = new Dictionary<ASTNodeType, VisitorFunction>();
        protected IO io { get; }
    }
}
