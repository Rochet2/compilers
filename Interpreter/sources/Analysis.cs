namespace Interpreter
{
    /*
     * A visitor class for semantic analysis on the program.
     */
    public class Analysis : Visitor
    {
        /*
         * Initializes the visitor with visitor functions.
         * The visitors mainly check that their children return correct value type
         * and then they return a value type as an ASTNode or null for no return value.
         */
        public Analysis(ASTNode ast, IO io) : base("Semantic analysis", ast, io)
        {
            this.visitorFunctions.Add(ASTNodeType.NUMBER, x => x);
            this.visitorFunctions.Add(ASTNodeType.STRING, x => x);
            this.visitorFunctions.Add(ASTNodeType.BOOLEAN, x => x);
            this.visitorFunctions.Add(ASTNodeType.TYPENAME, x => x);
            this.visitorFunctions.Add(ASTNodeType.UNARYOPERATOR, x =>
            {
                var unaryOperation = As<UnaryOperator>(x, ASTNodeType.UNARYOPERATOR);
                var operand = ExpectNotNull(Visit(unaryOperation.operand), unaryOperation.operand);
                if (Is<Boolean>(operand))
                {
                    switch (unaryOperation.unaryOperator)
                    {
                        case "!":
                            return new Boolean(false);
                    }
                    throw new VisitorException(string.Format("unrecognized boolean unary operator {0}", unaryOperation.unaryOperator), unaryOperation);
                }
                throw new VisitorException(string.Format("unrecognized unary operator {0} for operand type {1}", unaryOperation.unaryOperator, operand.type), unaryOperation);
            });
            this.visitorFunctions.Add(ASTNodeType.EXPRESSION, x =>
            {
                var expression = As<Expression>(x, ASTNodeType.EXPRESSION);
                if (expression.binaryOperator == null)
                    return ExpectNotNull(Visit(expression.leftOperand), expression.leftOperand);
                var binaryOperation = As<BinaryOperator>(expression.binaryOperator, ASTNodeType.BINARYOPERATOR);
                var leftOperand = ExpectNotNull(Visit(expression.leftOperand), expression.leftOperand);
                var rightOperand = ExpectNotNull(Visit(binaryOperation.rightOperand), binaryOperation.rightOperand);
                if (Is<Number>(leftOperand) && Is<Number>(rightOperand))
                {
                    switch (binaryOperation.binaryOperator)
                    {
                        case "+":
                        case "-":
                        case "*":
                        case "/":
                            return new Number(0);
                        case "=":
                        case "<":
                            return new Boolean(false);
                    }
                    throw new VisitorException(string.Format("unknown integer binary operator {0}", binaryOperation.binaryOperator), binaryOperation);
                }
                if (Is<String>(leftOperand) && Is<String>(rightOperand))
                {
                    switch (binaryOperation.binaryOperator)
                    {
                        case "+":
                            return new String("");
                        case "=":
                        case "<":
                            return new Boolean(false);
                    }
                    throw new VisitorException(string.Format("unknown string binary operator {0}", binaryOperation.binaryOperator), binaryOperation);
                }
                if (Is<Boolean>(leftOperand) && Is<Boolean>(rightOperand))
                {
                    switch (binaryOperation.binaryOperator)
                    {
                        case "&":
                        case "=":
                        case "<":
                            return new Boolean(false);
                    }
                    throw new VisitorException(string.Format("unknown boolean binary operator {0}", binaryOperation.binaryOperator), binaryOperation);
                }
                throw new VisitorException(string.Format("unknown binary operator {0} for operand types left: {1}, right: {2}", binaryOperation.binaryOperator, Visit(leftOperand).type, Visit(rightOperand).type), binaryOperation);
            });
            this.visitorFunctions.Add(ASTNodeType.PRINT, x =>
            {
                var printNode = As<Print>(x, ASTNodeType.PRINT);
                Expect<ASTVariableNode>(Visit(printNode.printedValue), ASTNodeType.VARIABLE, printNode.printedValue);
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.STATEMENT, x =>
            {
                var statements = As<Statements>(x, ASTNodeType.STATEMENT);

                // Here we visit a statement and if it has errors we print them
                try
                {
                    ExpectNull(Visit(statements.statement), statements.statement);
                }
                catch (VisitorException e)
                {
                    PrintError(e);
                }

                // then we move to the next statement if any and print errors from it if any.
                // this allows us to visit all statements regardless of errors as we
                // will keep visiting the next statement even if previous has errors.
                if (statements.statementtail != null)
                {
                    try
                    {
                        ExpectNull(Visit(statements.statementtail), statements.statementtail);
                    }
                    catch (VisitorException e)
                    {
                        PrintError(e);
                    }
                }
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.ASSERT, x =>
            {
                var assertNode = As<Assert>(x, ASTNodeType.ASSERT);
                Expect<Boolean>(Visit(assertNode.condition), ASTNodeType.BOOLEAN, assertNode.condition);
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.READ, x =>
            {
                var readNode = As<Read>(x, ASTNodeType.READ);
                var identifier = As<Identifier>(readNode.identifierToRead, ASTNodeType.IDENTIFIER);
                var variable = ExpectMutable(identifier, readNode.identifierToRead);
                var variableType = variable.value.type;
                switch (variableType)
                {
                    case ASTNodeType.NUMBER:
                        variables[identifier.name] = new Variable(new Number(0));
                        break;
                    case ASTNodeType.STRING:
                        variables[identifier.name] = new Variable(new String(""));
                        break;
                    default:
                        throw new VisitorException(string.Format("variable {0} has unsupported type {1} to read from input", identifier.name, variableType), readNode.identifierToRead);
                }
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.IDENTIFIER, x =>
            {
                var identifier = As<Identifier>(x, ASTNodeType.IDENTIFIER);
                return GetVariable(identifier).value;
            });
            this.visitorFunctions.Add(ASTNodeType.DECLARATION, x =>
            {
                var declarationNode = As<Declaration>(x, ASTNodeType.DECLARATION);
                var typeNameNode = As<TypeName>(declarationNode.identifierType, ASTNodeType.TYPENAME);
                var identifier = As<Identifier>(declarationNode.identifier, ASTNodeType.IDENTIFIER);

                // get the real variable type
                ASTNodeType variableType;
                switch (typeNameNode.typeName)
                {
                    case "int":
                        variableType = ASTNodeType.NUMBER;
                        break;
                    case "string":
                        variableType = ASTNodeType.STRING;
                        break;
                    case "bool":
                        variableType = ASTNodeType.BOOLEAN;
                        break;
                    default:
                        throw new VisitorException(string.Format("unknown identifier type name {0}", typeNameNode.typeName), declarationNode.identifierType);
                }

                // make sure identifier is not taken
                if (variables.ContainsKey(identifier.name))
                    throw new VisitorException(string.Format("variable {0} already defined", identifier.name), declarationNode.identifier);
                
                if (declarationNode.identifierValue == null)
                {
                    // initialize with default value if one not given
                    switch (typeNameNode.typeName)
                    {
                        case "int":
                            variables[identifier.name] = new Variable(new Number(0));
                            break;
                        case "string":
                            variables[identifier.name] = new Variable(new String(""));
                            break;
                        case "bool":
                            variables[identifier.name] = new Variable(new Boolean(false));
                            break;
                    }
                }
                else
                {
                    // check that given value is correct type and do assignment
                    var value = Visit(declarationNode.identifierValue);
                    if (value == null || value.type != variableType)
                        throw new VisitorException(string.Format("variable {0} type {1} does not match value type {2}", identifier.name, variableType, value == null ? "null" : value.type.ToString()), identifier);
                    variables[identifier.name] = new Variable(As<ASTVariableNode>(value, ASTNodeType.VARIABLE));
                }
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.FORLOOP, x =>
            {
                var forLoopNode = As<ForLoop>(x, ASTNodeType.FORLOOP);
                var identifier = As<Identifier>(forLoopNode.loopVariableIdentifier, ASTNodeType.IDENTIFIER);
                var control = ExpectMutable(identifier, forLoopNode.loopVariableIdentifier);
                Expect<Number>(control.value, ASTNodeType.NUMBER, forLoopNode.loopVariableIdentifier);
                Expect<Number>(Visit(forLoopNode.beginValue), ASTNodeType.NUMBER, forLoopNode.beginValue);
                Expect<Number>(Visit(forLoopNode.endValue), ASTNodeType.NUMBER, forLoopNode.endValue);

                // we set the control variable immutable here to check
                // any errors regarding changing the control variable in for loop.
                control.immutable = true;
                ExpectNull(Visit(forLoopNode.statements), forLoopNode.statements);
                control.immutable = false;
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.ASSIGNMENT, x =>
            {
                var assignmentNode = As<Assignment>(x, ASTNodeType.ASSIGNMENT);
                var identifier = As<Identifier>(assignmentNode.identifier, ASTNodeType.IDENTIFIER);
                var variable = ExpectMutable(identifier, assignmentNode.identifier);
                var value = Visit(assignmentNode.value);
                if (value == null || value.type != variable.value.type)
                    throw new VisitorException(string.Format("variable {0} type {1} does not match value type {2}", identifier.name, variable.value.type, value == null ? "null" : value.type.ToString()), x);
                variables[identifier.name] = new Variable(As<ASTVariableNode>(value, ASTNodeType.VARIABLE));
                return null;
            });
        }
    }
}
