/*
 * This file defines all possible abstract syntax tree (AST) node classes.
 */
namespace Interpreter
{
    /*
     * Possible types of an AST node
     */
    public enum ASTNodeType
    {
        NUMBER,
        STRING,
        BOOLEAN,
        IDENTIFIER,
        TYPENAME,
        PRINT,
        VARIABLE,
        ASSERT,
        READ,
        DECLARATION,
        ASSIGNMENT,
        FORLOOP,
        EXPRESSION,
        BINARYOPERATOR,
        UNARYOPERATOR,
        STATEMENT,
    }

    /*
     * Abstract class that all AST nodes inherit.
     */
    public abstract class ASTNode
    {
        public ASTNode(ASTNodeType type, Lexeme lexeme = null)
        {
            this.type = type;
            this.lexeme = lexeme;
        }

        public readonly ASTNodeType type;
        public Lexeme lexeme;
    }

    /*
     * Abstract class that all AST variable nodes inherit.
     */
    public abstract class ASTVariable : ASTNode
    {
        public ASTVariable(ASTNodeType type) : base(type)
        {
        }

        /*
         * Returns the value of the variable
         */
        public abstract object Value();
    }


    /*
     * Below are the AST node classes for each possible ASTNodeType.
     */

    public class Expression : ASTNode
    {
        public Expression() :
            base(ASTNodeType.EXPRESSION)
        {
        }

        public ASTNode leftOperand;
        public ASTNode expressionTail;
    }

    public class BinaryOperator : ASTNode
    {
        public BinaryOperator() :
            base(ASTNodeType.BINARYOPERATOR)
        {
        }

        public string binaryOperator;
        public ASTNode rightOperand;
    }

    public class Number : ASTVariable
    {
        public Number(string value) :
            base(ASTNodeType.NUMBER)
        {
            this.value = int.Parse(value);
        }

        public Number(int value) :
            base(ASTNodeType.NUMBER)
        {
            this.value = value;
        }

        public Number(string value, Lexeme lexeme) :
            base(ASTNodeType.NUMBER)
        {
            this.value = int.Parse(value);
            this.lexeme = lexeme;
        }

        public Number(int value, Lexeme lexeme) :
            base(ASTNodeType.NUMBER)
        {
            this.value = value;
            this.lexeme = lexeme;
        }

        public override object Value()
        {
            return value;
        }

        public int value;
    }

    public class String : ASTVariable
    {
        public String(string value) :
            base(ASTNodeType.STRING)
        {
            this.value = value;
        }

        public override object Value()
        {
            return value;
        }

        public string value;
    }

    public class Boolean : ASTVariable
    {
        public Boolean(bool value) :
            base(ASTNodeType.BOOLEAN)
        {
            this.value = value;
        }

        public override object Value()
        {
            return value;
        }

        public bool value;
    }

    public class Identifier : ASTNode
    {
        public Identifier(string name) :
            base(ASTNodeType.IDENTIFIER)
        {
            this.name = name;
        }

        public string name;
    }

    public class Print : ASTNode
    {
        public Print() :
            base(ASTNodeType.PRINT)
        {
        }

        public ASTNode printedValue;
    }

    public class Read : ASTNode
    {
        public Read() :
            base(ASTNodeType.READ)
        {
        }

        public ASTNode identifierToRead;
    }

    public class Assert : ASTNode
    {
        public Assert() :
            base(ASTNodeType.ASSERT)
        {
        }

        public ASTNode condition;
    }

    public class UnaryOperator : ASTNode
    {
        public UnaryOperator() :
            base(ASTNodeType.UNARYOPERATOR)
        {
        }

        public string unaryOperator;
        public ASTNode operand;
    }

    public class TypeName : ASTNode
    {
        public TypeName(string typeName) :
            base(ASTNodeType.TYPENAME)
        {
            this.typeName = typeName;
        }

        public string typeName;
    }

    public class Statements : ASTNode
    {
        public Statements() :
            base(ASTNodeType.STATEMENT)
        {
        }

        public ASTNode statement;
        public ASTNode statementstail;
    }

    public class Declaration : ASTNode
    {
        public Declaration() :
            base(ASTNodeType.DECLARATION)
        {
        }

        public ASTNode identifier;
        public ASTNode identifierType;
        public ASTNode identifierValue = null;
    }

    public class Assignment : ASTNode
    {
        public Assignment() :
            base(ASTNodeType.ASSIGNMENT)
        {
        }

        public ASTNode identifier;
        public ASTNode value;
    }

    public class ForLoop : ASTNode
    {
        public ForLoop() :
            base(ASTNodeType.FORLOOP)
        {
        }

        public ASTNode loopVariableIdentifier;
        public ASTNode beginValue, endValue, statements;
    }
}
