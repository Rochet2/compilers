namespace Interpreter
{
    /*
     * A visitor class for printing the tokens of given expression with given IO.
     * Expects the given AST to consist only of ExpressionNode or any possible child node to it.
     */
    public class ExpressionPrinter : Visitor
    {
        /*
         * Prints the token of the given node if it has one.
         * Returns null always.
         */
        private ASTNode PrintToken(ASTNode node)
        {
            if (node != null && node.lexeme != null && node.lexeme.token != null)
                io.Write(node.lexeme.token);
            return null;
        }

        /*
         * Initializes the visitor with visitor functions
         */
        public ExpressionPrinter(ASTNode ast, IO io) : base("Printer", ast, io)
        {
            this.visitorFunctions.Add(ASTNodeType.NUMBER, PrintToken);
            this.visitorFunctions.Add(ASTNodeType.STRING, PrintToken);
            this.visitorFunctions.Add(ASTNodeType.BOOLEAN, PrintToken);
            this.visitorFunctions.Add(ASTNodeType.UNARYOPERATOR, x =>
            {
                var unaryOperator = As<UnaryOperatorNode>(x, ASTNodeType.UNARYOPERATOR);
                PrintToken(unaryOperator);
                PrintToken(unaryOperator.operand);
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.EXPRESSION, x =>
            {
                var expression = As<ExpressionNode>(x, ASTNodeType.EXPRESSION);
                if (expression.binaryOperator == null)
                    return Visit(expression.leftOperand); // was not binary operation
                // is a binary opration
                var binaryOperator = As<BinaryOperatorNode>(expression.binaryOperator, ASTNodeType.BINARYOPERATOR);
                io.Write("(");
                Visit(expression.leftOperand);
                PrintToken(binaryOperator);
                Visit(binaryOperator.rightOperand);
                io.Write(")");
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.IDENTIFIER, PrintToken);
        }
    }
}
