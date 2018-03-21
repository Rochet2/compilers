namespace Interpreter
{
    public class ExpressionPrinter : Visitor
    {
        private ASTNode PrintToken(ASTNode node)
        {
            if (node != null && node.lexeme != null && node.lexeme.token != null)
                io.Write(node.lexeme.token);
            return null;
        }

        public ExpressionPrinter(ASTNode ast, IO io) : base("Printer", ast, io)
        {
            this.visitorFunctions.Add(ASTNodeType.NUMBER, PrintToken);
            this.visitorFunctions.Add(ASTNodeType.STRING, PrintToken);
            this.visitorFunctions.Add(ASTNodeType.BOOLEAN, PrintToken);
            this.visitorFunctions.Add(ASTNodeType.UNARYOPERATOR, x =>
            {
                var op = As<UnaryOperator>(x, ASTNodeType.UNARYOPERATOR);
                PrintToken(op);
                PrintToken(op.operand);
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.EXPRESSION, x =>
            {
                var exp = As<Expression>(x, ASTNodeType.EXPRESSION);
                if (exp.expressionTail == null)
                    return Visit(exp.leftOperand);
                var rtail = As<BinaryOperator>(exp.expressionTail, ASTNodeType.BINARYOPERATOR);
                io.Write("(");
                Visit(exp.leftOperand);
                PrintToken(rtail);
                Visit(rtail.rightOperand);
                io.Write(")");
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.IDENTIFIER, PrintToken);
        }
    }
}
