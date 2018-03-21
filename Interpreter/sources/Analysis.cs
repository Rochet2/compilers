namespace Interpreter
{
    public class Analysis : Visitor
    {
        public Analysis(ASTNode ast, IO io) : base("Semantic analysis", ast, io)
        {
            this.visitorFunctions.Add(ASTNodeType.NUMBER, x => x);
            this.visitorFunctions.Add(ASTNodeType.STRING, x => x);
            this.visitorFunctions.Add(ASTNodeType.BOOLEAN, x => x);
            this.visitorFunctions.Add(ASTNodeType.TYPENAME, x => x);
            this.visitorFunctions.Add(ASTNodeType.UNARYOP, x =>
            {
                var op = As<UnaryOperator>(x, ASTNodeType.UNARYOP);
                var v = ExpectNotNull(Visit(op.operand), op.operand);
                if (Is<Boolean>(v))
                {
                    switch (op.unaryOperator)
                    {
                        case "!":
                            return new Boolean(false);
                    }
                    throw new VisitorException(string.Format("unrecognized boolean unary operator {0}", op.unaryOperator), op);
                }
                throw new VisitorException(string.Format("unrecognized unary operator {0} for operand type {1}", op.unaryOperator, v.type), op);
            });
            this.visitorFunctions.Add(ASTNodeType.EXPRESSION, x =>
            {
                var exp = As<Expression>(x, ASTNodeType.EXPRESSION);
                if (exp.expressionTail == null)
                    return ExpectNotNull(Visit(exp.leftOperand), exp.leftOperand);
                var rtail = As<BinaryOperator>(exp.expressionTail, ASTNodeType.BINARYOP);
                var opl = ExpectNotNull(Visit(exp.leftOperand), exp.leftOperand);
                var opr = ExpectNotNull(Visit(rtail.rightOperand), rtail.rightOperand);
                if (Is<Number>(opl) && Is<Number>(opr))
                {
                    switch (rtail.binaryOperator)
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
                    throw new VisitorException(string.Format("unknown integer binary operator {0}", rtail.binaryOperator), rtail);
                }
                if (Is<String>(opl) && Is<String>(opr))
                {
                    switch (rtail.binaryOperator)
                    {
                        case "+":
                            return new String("");
                        case "=":
                        case "<":
                            return new Boolean(false);
                    }
                    throw new VisitorException(string.Format("unknown string binary operator {0}", rtail.binaryOperator), rtail);
                }
                if (Is<Boolean>(opl) && Is<Boolean>(opr))
                {
                    switch (rtail.binaryOperator)
                    {
                        case "&":
                        case "=":
                        case "<":
                            return new Boolean(false);
                    }
                    throw new VisitorException(string.Format("unknown boolean binary operator {0}", rtail.binaryOperator), rtail);
                }
                throw new VisitorException(string.Format("unknown binary operator {0} for operand types left: {1}, right: {2}", rtail.binaryOperator, Visit(opl).type, Visit(opr).type), rtail);
            });
            this.visitorFunctions.Add(ASTNodeType.PRINT, x =>
            {
                var f = As<Print>(x, ASTNodeType.PRINT);
                Expect<ASTVariable>(Visit(f.printedValue), ASTNodeType.VARIABLE, f.printedValue);
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.STATEMENTS, x =>
            {
                var v = As<Statements>(x, ASTNodeType.STATEMENTS);
                try
                {
                    ExpectNull(Visit(v.statement), v.statement);
                }
                catch (VisitorException e)
                {
                    PrintError(e);
                }
                if (v.statementstail != null)
                {
                    try
                    {
                        ExpectNull(Visit(v.statementstail), v.statementstail);
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
                var f = As<Assert>(x, ASTNodeType.ASSERT);
                Expect<Boolean>(Visit(f.condition), ASTNodeType.BOOLEAN, f.condition);
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.READ, x =>
            {
                var f = As<Read>(x, ASTNodeType.READ);
                var ident = As<Identifier>(f.identifierToRead, ASTNodeType.IDENTIFIER);
                var variable = ExpectMutable(ident, f.identifierToRead);
                var type = variable.value.type;
                switch (type)
                {
                    case ASTNodeType.NUMBER:
                        variables[ident.name] = new Variable(new Number(0));
                        break;
                    case ASTNodeType.STRING:
                        variables[ident.name] = new Variable(new String(""));
                        break;
                    default:
                        throw new VisitorException(string.Format("variable {0} has unsupported type {1} to read from input", ident.name, type), f.identifierToRead);
                }
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.IDENTIFIER, x =>
            {
                var v = As<Identifier>(x, ASTNodeType.IDENTIFIER);
                return GetVar(v).value;
            });
            this.visitorFunctions.Add(ASTNodeType.DECLARATION, x =>
            {
                var v = As<Declaration>(x, ASTNodeType.DECLARATION);
                var type = As<TypeName>(v.identifierType, ASTNodeType.TYPENAME);
                var ident = As<Identifier>(v.identifier, ASTNodeType.IDENTIFIER);
                ASTNodeType typeasttype;
                switch (type.typeName)
                {
                    case "int":
                        typeasttype = ASTNodeType.NUMBER;
                        break;
                    case "string":
                        typeasttype = ASTNodeType.STRING;
                        break;
                    case "bool":
                        typeasttype = ASTNodeType.BOOLEAN;
                        break;
                    default:
                        throw new VisitorException(string.Format("unknown identifier type name {0}", type.typeName), v.identifierType);
                }
                if (variables.ContainsKey(ident.name))
                    throw new VisitorException(string.Format("variable {0} already defined", ident.name), v.identifier);
                if (v.identifierValue == null)
                {
                    switch (type.typeName)
                    {
                        case "int":
                            variables[ident.name] = new Variable(new Number(0));
                            break;
                        case "string":
                            variables[ident.name] = new Variable(new String(""));
                            break;
                        case "bool":
                            variables[ident.name] = new Variable(new Boolean(false));
                            break;
                    }
                }
                else
                {
                    var value = Visit(v.identifierValue);
                    if (value == null || value.type != typeasttype)
                        throw new VisitorException(string.Format("variable {0} type {1} does not match value type {2}", ident.name, typeasttype, value == null ? "null" : value.type.ToString()), ident);
                    variables[ident.name] = new Variable(As<ASTVariable>(value, ASTNodeType.VARIABLE));
                }
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.FORLOOP, x =>
            {
                var v = As<ForLoop>(x, ASTNodeType.FORLOOP);
                var ident = As<Identifier>(v.loopVariableIdentifier, ASTNodeType.IDENTIFIER);
                var control = ExpectMutable(ident, v.loopVariableIdentifier);
                Expect<Number>(control.value, ASTNodeType.NUMBER, v.loopVariableIdentifier);
                Expect<Number>(Visit(v.beginValue), ASTNodeType.NUMBER, v.beginValue);
                Expect<Number>(Visit(v.endValue), ASTNodeType.NUMBER, v.endValue);
                control.immutable = true;
                ExpectNull(Visit(v.statements), v.statements);
                control.immutable = false;
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.ASSIGN, x =>
            {
                var v = As<Assignment>(x, ASTNodeType.ASSIGN);
                var ident = As<Identifier>(v.identifier, ASTNodeType.IDENTIFIER);
                var variable = ExpectMutable(ident, v.identifier);
                var value = Visit(v.value);
                if (value == null || value.type != variable.value.type)
                    throw new VisitorException(string.Format("variable {0} type {1} does not match value type {2}", ident.name, variable.value.type, value == null ? "null" : value.type.ToString()), x);
                variables[ident.name] = new Variable(As<ASTVariable>(value, ASTNodeType.VARIABLE));
                return null;
            });
        }
    }
}
