using System;

namespace Interpreter
{
    /*
     * A visitor class for interpreting the given AST.
     * The visitor will try to run the AST as a program.
     * Any error halts the execution.
     */
    public class Interpreter : Visitor
    {
        /*
         * Initializes the visitor with visitor functions.
         * The visitor functions evaluate the node's children, execute the node's action and
         * return a new ASTNode which contains the return value or null.
         */
        public Interpreter(ASTNode ast, IO io) : base("Interpreter", ast, io)
        {
            this.visitorFunctions.Add(ASTNodeType.NUMBER, x => x);
            this.visitorFunctions.Add(ASTNodeType.STRING, x => x);
            this.visitorFunctions.Add(ASTNodeType.BOOLEAN, x => x);
            this.visitorFunctions.Add(ASTNodeType.TYPENAME, x => x);
            this.visitorFunctions.Add(ASTNodeType.UNARYOPERATOR, x =>
            {
                var unaryOperation = As<UnaryOperatorNode>(x, ASTNodeType.UNARYOPERATOR);
                var operand = ExpectNotNull(Visit(unaryOperation.operand), unaryOperation.operand);
                // Make sure the operand is boolean before checking boolean operators
                if (Is<BooleanNode>(operand))
                {
                    switch (unaryOperation.unaryOperator)
                    {
                        case "!":
                            return new BooleanNode(!As<BooleanNode>(operand, ASTNodeType.BOOLEAN, unaryOperation.operand).value);
                    }
                    throw new VisitorException(string.Format("unrecognized boolean unary operator {0}", unaryOperation.unaryOperator), unaryOperation);
                }
                throw new VisitorException(string.Format("unrecognized unary operator {0} for operand type {1}", unaryOperation.unaryOperator, operand.type), unaryOperation);
            });
            this.visitorFunctions.Add(ASTNodeType.EXPRESSION, x =>
            {
                var expression = As<ExpressionNode>(x, ASTNodeType.EXPRESSION);
                if (expression.binaryOperator == null)
                    return ExpectNotNull(Visit(expression.leftOperand), expression.leftOperand); // is not a binary operation
                // is a binary operation
                // fetch all values
                var binaryOperation = As<BinaryOperatorNode>(expression.binaryOperator, ASTNodeType.BINARYOPERATOR);
                var operandLeft = ExpectNotNull(Visit(expression.leftOperand), expression.leftOperand);
                var operandRight = ExpectNotNull(Visit(binaryOperation.rightOperand), binaryOperation.rightOperand);
                // Do number operations
                if (Is<NumberNode>(operandLeft) && Is<NumberNode>(operandRight))
                {
                    var l = As<NumberNode>(operandLeft, ASTNodeType.NUMBER, expression.leftOperand);
                    var r = As<NumberNode>(operandRight, ASTNodeType.NUMBER, binaryOperation.rightOperand);
                    switch (binaryOperation.binaryOperator)
                    {
                        case "+":
                            return new NumberNode(l.value + r.value);
                        case "-":
                            return new NumberNode(l.value - r.value);
                        case "*":
                            return new NumberNode(l.value * r.value);
                        case "/":
                            return new NumberNode(l.value / r.value);
                        case "=":
                            return new BooleanNode(l.value == r.value);
                        case "<":
                            return new BooleanNode(l.value < r.value);
                    }
                    throw new VisitorException(string.Format("unknown integer binary operator {0}", binaryOperation.binaryOperator), binaryOperation);
                }
                // do string operations
                if (Is<StringNode>(operandLeft) && Is<StringNode>(operandRight))
                {
                    var l = As<StringNode>(operandLeft, ASTNodeType.STRING, expression.leftOperand);
                    var r = As<StringNode>(operandRight, ASTNodeType.STRING, binaryOperation.rightOperand);
                    switch (binaryOperation.binaryOperator)
                    {
                        case "+":
                            return new StringNode(l.value + r.value);
                        case "=":
                            return new BooleanNode(l.value == r.value);
                        case "<":
                            return new BooleanNode(string.Compare(l.value, r.value) < 0);
                    }
                    throw new VisitorException(string.Format("unknown string binary operator {0}", binaryOperation.binaryOperator), binaryOperation);
                }
                // do boolean operations
                if (Is<BooleanNode>(operandLeft) && Is<BooleanNode>(operandRight))
                {
                    var l = As<BooleanNode>(operandLeft, ASTNodeType.BOOLEAN, expression.leftOperand);
                    var r = As<BooleanNode>(operandRight, ASTNodeType.BOOLEAN, binaryOperation.rightOperand);
                    switch (binaryOperation.binaryOperator)
                    {
                        case "&":
                            return new BooleanNode(l.value && r.value);
                        case "=":
                            return new BooleanNode(l.value == r.value);
                        case "<":
                            return new BooleanNode(!l.value && r.value);
                    }
                    throw new VisitorException(string.Format("unknown boolean binary operator {0}", binaryOperation.binaryOperator), binaryOperation);
                }
                throw new VisitorException(string.Format("unknown binary operator {0} for operand types left: {1}, right: {2}", binaryOperation.binaryOperator, Visit(operandLeft).type, Visit(operandRight).type), binaryOperation);
            });
            this.visitorFunctions.Add(ASTNodeType.PRINT, x =>
            {
                var printNode = As<PrintNode>(x, ASTNodeType.PRINT);
                var printed = As<ASTVariableNode>(Visit(printNode.printedValue), ASTNodeType.VARIABLE, printNode.printedValue);
                io.Write("{0}", printed.Value());
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.STATEMENT, x =>
            {
                var statements = As<StatementsNode>(x, ASTNodeType.STATEMENT);
                ExpectNull(Visit(statements.statement), statements.statement);
                if (statements.statementtail != null) // statement has another statement after it
                    ExpectNull(Visit(statements.statementtail), statements.statementtail);
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.ASSERT, x =>
            {
                var assertNode = As<AssertNode>(x, ASTNodeType.ASSERT);
                var condition = As<BooleanNode>(Visit(assertNode.condition), ASTNodeType.BOOLEAN, assertNode.condition);
                if (!condition.value)
                {
                    // assertion failed.
                    // visit the assertion condition with a printer
                    // and collect the output to string IO.
                    var stringIO = new StringIO();
                    var expressionPrinter = new ExpressionPrinter(assertNode.condition, stringIO);
                    expressionPrinter.Visit();

                    // stop execution by throwing an exception with the assertion error message.
                    // error message contains the asserted expression if visitor did not fail.
                    throw new VisitorException(
                        expressionPrinter.errored ?
                        "assertion failed" :
                        string.Format("assertion failed with condition {0}", stringIO.output),
                        assertNode
                    );
                }
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.READ, x =>
            {
                var readNode = As<ReadNode>(x, ASTNodeType.READ);
                var identifier = As<IdentifierNode>(readNode.identifierToRead, ASTNodeType.IDENTIFIER);
                var variable = ExpectMutable(identifier, readNode.identifierToRead);
                var variableType = variable.value.type;
                switch (variableType)
                {
                    case ASTNodeType.NUMBER:
                        while (true)
                        {
                            // keep trying to read input until a number is read successfully
                            try
                            {

                                variables[identifier.name] = new Variable(new NumberNode(ReadInput()));
                                break;
                            }
                            catch (System.FormatException /*e*/)
                            {
                                // Occurs when user inputs non number
                                // Do nothing
                            }
                        }
                        break;
                    case ASTNodeType.STRING:
                        // read a string from input
                        variables[identifier.name] = new Variable(new StringNode(ReadInput()));
                        break;
                    default:
                        throw new VisitorException(string.Format("variable {0} has unsupported type {1} to read from input", identifier.name, variableType), readNode.identifierToRead);
                }
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.IDENTIFIER, x =>
            {
                var identifier = As<IdentifierNode>(x, ASTNodeType.IDENTIFIER);
                return GetVariable(identifier).value;
            });
            this.visitorFunctions.Add(ASTNodeType.DECLARATION, x =>
            {
                var declarationNode = As<DeclarationNode>(x, ASTNodeType.DECLARATION);
                var typeNameNode = As<TypeNameNode>(declarationNode.identifierType, ASTNodeType.TYPENAME);
                var identifier = As<IdentifierNode>(declarationNode.identifier, ASTNodeType.IDENTIFIER);

                // get the real type from string type
                // error on unknown type
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
                    // if there is no value given, then lets initialize with appropriate default value
                    switch (typeNameNode.typeName)
                    {
                        case "int":
		                    variables[identifier.name] = new Variable(new NumberNode(0));
                            break;
                        case "string":
                            variables[identifier.name] = new Variable(new StringNode(""));
                            break;
                        case "bool":
                            variables[identifier.name] = new Variable(new BooleanNode(false));
                            break;
                    }
                }
                else
                {
                    // make sure variable type and value type match and then do the assignment
                    var value = Visit(declarationNode.identifierValue);
                    if (value == null || value.type != variableType)
                        throw new VisitorException(string.Format("variable {0} type {1} does not match value type {2}", identifier.name, variableType, value == null ? "null" : value.type.ToString()), identifier);
                    variables[identifier.name] = new Variable(As<ASTVariableNode>(value, ASTNodeType.VARIABLE));
                }
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.FORLOOP, x =>
            {
                var forLoopNode = As<ForLoopNode>(x, ASTNodeType.FORLOOP);

                // get control variable and make sure it is correct type and is mutable
                var identifier = As<IdentifierNode>(forLoopNode.loopVariableIdentifier, ASTNodeType.IDENTIFIER);
                var controlVariable = ExpectMutable(identifier, forLoopNode.loopVariableIdentifier);
                Expect<NumberNode>(controlVariable.value, ASTNodeType.NUMBER, forLoopNode.loopVariableIdentifier);

                // make sure begin and end are numbers
                var begin = As<NumberNode>(Visit(forLoopNode.beginValue), ASTNodeType.NUMBER, forLoopNode.beginValue);
                var end = As<NumberNode>(Visit(forLoopNode.endValue), ASTNodeType.NUMBER, forLoopNode.endValue);

                // set control variable immutable
                controlVariable.immutable = true;

                // do the for-loop
                int i = begin.value;
                for (; i <= end.value; ++i)
                {
                    // update the control variable
                    variables[identifier.name] = new Variable(new NumberNode(i, identifier.lexeme), true);
                    // execute statements
                    ExpectNull(Visit(forLoopNode.statements), forLoopNode.statements);
                }

                // A small quirk that seemed to be a part of an example program
                // the quirk is that after for loop ends, the control variable is one over the end
                variables[identifier.name] = new Variable(new NumberNode(i, identifier.lexeme));

                // make control variable mutable again
                controlVariable.immutable = false;
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.ASSIGNMENT, x =>
            {
                var assignmentNode = As<AssignmentNode>(x, ASTNodeType.ASSIGNMENT);
                // get variable and assigned value
                var identifier = As<IdentifierNode>(assignmentNode.identifier, ASTNodeType.IDENTIFIER);
                var variable = ExpectMutable(identifier, assignmentNode.identifier);
                var value = Visit(assignmentNode.value);
                // make sure value matches variable type
                if (value == null || value.type != variable.value.type)
                    throw new VisitorException(string.Format("variable {0} type {1} does not match value type {2}", identifier.name, variable.value.type, value == null ? "null" : value.type.ToString()), x);
                // do the assignment
                variables[identifier.name] = new Variable(As<ASTVariableNode>(value, ASTNodeType.VARIABLE));
                return null;
            });
        }

        /*
         * An utility function for reading a string from the input.
         * The string is terminated when input ends or a whitespace is read.
         */
        private string ReadInput()
        {
            int readCharacter = io.Read();
            string resultString = "";
            while (readCharacter >= 0 && !Char.IsWhiteSpace((char)readCharacter))
            {
                resultString += (char)readCharacter;
                readCharacter = io.Read();
            }
            return resultString;
        }
    }
}

