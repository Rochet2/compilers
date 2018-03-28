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
    public abstract class ASTVariableNode : ASTNode
    {
        public ASTVariableNode(ASTNodeType type) : base(type)
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

    public class ExpressionNode : ASTNode
    {
        public ExpressionNode() :
            base(ASTNodeType.EXPRESSION)
        {
        }

        public ASTNode leftOperand;
        public ASTNode binaryOperator;
    }

    public class BinaryOperatorNode : ASTNode
    {
        public BinaryOperatorNode() :
            base(ASTNodeType.BINARYOPERATOR)
        {
        }

        public string binaryOperator;
        public ASTNode rightOperand;
    }

    public class NumberNode : ASTVariableNode
    {
        public NumberNode(string value) :
            base(ASTNodeType.NUMBER)
        {
            this.value = int.Parse(value);
        }

        public NumberNode(int value) :
            base(ASTNodeType.NUMBER)
        {
            this.value = value;
        }

        public NumberNode(string value, Lexeme lexeme) :
            base(ASTNodeType.NUMBER)
        {
            this.value = int.Parse(value);
            this.lexeme = lexeme;
        }

        public NumberNode(int value, Lexeme lexeme) :
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

    public class StringNode : ASTVariableNode
    {
        public StringNode(string value) :
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

    public class BooleanNode : ASTVariableNode
    {
        public BooleanNode(bool value) :
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

    public class IdentifierNode : ASTNode
    {
        public IdentifierNode(string name) :
            base(ASTNodeType.IDENTIFIER)
        {
            this.name = name;
        }

        public string name;
    }

    public class PrintNode : ASTNode
    {
        public PrintNode() :
            base(ASTNodeType.PRINT)
        {
        }

        public ASTNode printedValue;
    }

    public class ReadNode : ASTNode
    {
        public ReadNode() :
            base(ASTNodeType.READ)
        {
        }

        public ASTNode identifierToRead;
    }

    public class AssertNode : ASTNode
    {
        public AssertNode() :
            base(ASTNodeType.ASSERT)
        {
        }

        public ASTNode condition;
    }

    public class UnaryOperatorNode : ASTNode
    {
        public UnaryOperatorNode() :
            base(ASTNodeType.UNARYOPERATOR)
        {
        }

        public string unaryOperator;
        public ASTNode operand;
    }

    public class TypeNameNode : ASTNode
    {
        public TypeNameNode(string typeName) :
            base(ASTNodeType.TYPENAME)
        {
            this.typeName = typeName;
        }

        public string typeName;
    }

    public class StatementsNode : ASTNode
    {
        public StatementsNode() :
            base(ASTNodeType.STATEMENT)
        {
        }

        public ASTNode statement;
        public ASTNode statementtail;
    }

    public class DeclarationNode : ASTNode
    {
        public DeclarationNode() :
            base(ASTNodeType.DECLARATION)
        {
        }

        public ASTNode identifier;
        public ASTNode identifierType;
        public ASTNode identifierValue = null;
    }

    public class AssignmentNode : ASTNode
    {
        public AssignmentNode() :
            base(ASTNodeType.ASSIGNMENT)
        {
        }

        public ASTNode identifier;
        public ASTNode value;
    }

    public class ForLoopNode : ASTNode
    {
        public ForLoopNode() :
            base(ASTNodeType.FORLOOP)
        {
        }

        public ASTNode loopVariableIdentifier;
        public ASTNode beginValue, endValue, statements;
    }
}
