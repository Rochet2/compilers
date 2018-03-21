using System;

namespace Interpreter
{
    public class Interpreter : Visitor
    {
        public Interpreter(ASTNode ast, IO io) : base("Interpreter", ast, io)
        {
            this.visitorFunctions.Add(ASTNodeType.NUMBER, x => x);
            this.visitorFunctions.Add(ASTNodeType.STRING, x => x);
            this.visitorFunctions.Add(ASTNodeType.BOOLEAN, x => x);
            this.visitorFunctions.Add(ASTNodeType.TYPENAME, x => x);
            this.visitorFunctions.Add(ASTNodeType.UNARYOPERATOR, x =>
            {
                var op = As<UnaryOperator>(x, ASTNodeType.UNARYOPERATOR);
                var v = ExpectNotNull(Visit(op.operand), op.operand);
                if (Is<Boolean>(v))
                {
                    switch (op.unaryOperator)
                    {
                        case "!":
                            return new Boolean(!As<Boolean>(v, ASTNodeType.BOOLEAN, op.operand).value);
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
                var rtail = As<BinaryOperator>(exp.expressionTail, ASTNodeType.BINARYOPERATOR);
                var opl = ExpectNotNull(Visit(exp.leftOperand), exp.leftOperand);
                var opr = ExpectNotNull(Visit(rtail.rightOperand), rtail.rightOperand);
                if (Is<Number>(opl) && Is<Number>(opr))
                {
                    var l = As<Number>(opl, ASTNodeType.NUMBER, exp.leftOperand);
                    var r = As<Number>(opr, ASTNodeType.NUMBER, rtail.rightOperand);
                    switch (rtail.binaryOperator)
                    {
                        case "+":
                            return new Number(l.value + r.value);
                        case "-":
                            return new Number(l.value - r.value);
                        case "*":
                            return new Number(l.value * r.value);
                        case "/":
                            return new Number(l.value / r.value);
                        case "=":
                            return new Boolean(l.value == r.value);
                        case "<":
                            return new Boolean(l.value < r.value);
                    }
                    throw new VisitorException(string.Format("unknown integer binary operator {0}", rtail.binaryOperator), rtail);
                }
                if (Is<String>(opl) && Is<String>(opr))
                {
                    var l = As<String>(opl, ASTNodeType.STRING, exp.leftOperand);
                    var r = As<String>(opr, ASTNodeType.STRING, rtail.rightOperand);
                    switch (rtail.binaryOperator)
                    {
                        case "+":
                            return new String(l.value + r.value);
                        case "=":
                            return new Boolean(l.value == r.value);
                        case "<":
                            return new Boolean(string.Compare(l.value, r.value) < 0);
                    }
                    throw new VisitorException(string.Format("unknown string binary operator {0}", rtail.binaryOperator), rtail);
                }
                if (Is<Boolean>(opl) && Is<Boolean>(opr))
                {
                    var l = As<Boolean>(opl, ASTNodeType.BOOLEAN, exp.leftOperand);
                    var r = As<Boolean>(opr, ASTNodeType.BOOLEAN, rtail.rightOperand);
                    switch (rtail.binaryOperator)
                    {
                        case "&":
                            return new Boolean(l.value && r.value);
                        case "=":
                            return new Boolean(l.value == r.value);
                        case "<":
                            return new Boolean(!l.value && r.value);
                    }
                    throw new VisitorException(string.Format("unknown boolean binary operator {0}", rtail.binaryOperator), rtail);
                }
                throw new VisitorException(string.Format("unknown binary operator {0} for operand types left: {1}, right: {2}", rtail.binaryOperator, Visit(opl).type, Visit(opr).type), rtail);
            });
            this.visitorFunctions.Add(ASTNodeType.PRINT, x =>
            {
                var f = As<Print>(x, ASTNodeType.PRINT);
                var v = As<ASTVariable>(Visit(f.printedValue), ASTNodeType.VARIABLE, f.printedValue);
                io.Write("{0}", v.Value());
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.STATEMENT, x =>
            {
                var v = As<Statements>(x, ASTNodeType.STATEMENT);
                ExpectNull(Visit(v.statement), v.statement);
                if (v.statementstail != null)
                    ExpectNull(Visit(v.statementstail), v.statementstail);
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.ASSERT, x =>
            {
                var f = As<Assert>(x, ASTNodeType.ASSERT);
                var c = As<Boolean>(Visit(f.condition), ASTNodeType.BOOLEAN, f.condition);
                if (!c.value)
                {
                    var sio = new StringIO();
                    var exprVisitor = new ExpressionPrinter(f.condition, sio);
                    exprVisitor.Visit();
                    throw new VisitorException(string.Format("assertion failed with condition {0}", sio.output), f);
                }
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
                        while (true)
                        {
                            try
                            {
                                variables[ident.name] = new Variable(new Number(read()));
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
                        variables[ident.name] = new Variable(new String(read()));
                        break;
                    default:
                        throw new VisitorException(string.Format("variable {0} has unsupported type {1} to read from input", ident.name, type), f.identifierToRead);
                }
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.IDENTIFIER, x =>
            {
                var v = As<Identifier>(x, ASTNodeType.IDENTIFIER);
                return GetVariable(v).value;
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
                var begin = As<Number>(Visit(v.beginValue), ASTNodeType.NUMBER, v.beginValue);
                var end = As<Number>(Visit(v.endValue), ASTNodeType.NUMBER, v.endValue);
                control.immutable = true;
                int i = begin.value;
                for (; i <= end.value; ++i)
                {
                    variables[ident.name] = new Variable(new Number(i, ident.lexeme), true);
                    ExpectNull(Visit(v.statements), v.statements);
                }
                variables[ident.name] = new Variable(new Number(i, ident.lexeme));
                control.immutable = false;
                return null;
            });
            this.visitorFunctions.Add(ASTNodeType.ASSIGNMENT, x =>
            {
                var v = As<Assignment>(x, ASTNodeType.ASSIGNMENT);
                var ident = As<Identifier>(v.identifier, ASTNodeType.IDENTIFIER);
                var variable = ExpectMutable(ident, v.identifier);
                var value = Visit(v.value);
                if (value == null || value.type != variable.value.type)
                    throw new VisitorException(string.Format("variable {0} type {1} does not match value type {2}", ident.name, variable.value.type, value == null ? "null" : value.type.ToString()), x);
                variables[ident.name] = new Variable(As<ASTVariable>(value, ASTNodeType.VARIABLE));
                return null;
            });
        }

        string read()
        {
            int c = io.Read();
            string s = "";
            while (c >= 0 && !Char.IsWhiteSpace((char)c))
            {
                s += (char)c;
                c = io.Read();
            }
            return s;
        }
    }
}

