using System;

namespace Interpreter
{
    /*
     * A class representing the Parser, which generates an AST from Lexemes.
     */
    public class Parser
    {
        /*
         * An exception thrown by the Parser on unexpected Lexeme.
         * Contains the error message and the current lexeme.
         */
        class ParserException : Exception
        {
            public ParserException(string message, Lexeme lexeme) : base(message)
            {
                this.lexeme = lexeme;
            }

            public Lexeme lexeme { get; }
        }

        public Parser(Lexer lexer, IO io)
        {
            this.lexer = lexer;
            this.io = io;
        }

        /*
         * Gets next lexeme and sets it as the currentLexeme.
         */
        void NextLexeme()
        {
            currentLexeme = lexer.NextLexeme();
        }

        /*
         * Consumes a lexeme of given type or throws ParserException.
         * Returns the consumed lexeme.
         * Moves to the next lexeme.
         */
        Lexeme Consume(TokenType t)
        {
            var lexeme = currentLexeme;
            if (lexeme == null)
                throw new ParserException(string.Format("expected token of type {0}", t), null);
            if (lexeme.type != t)
                throw new ParserException(string.Format("expected token of type {0}, got {1}", t, lexeme == null ? "null" : lexeme.ToString()), lexeme);
            NextLexeme();
            return lexeme;
        }

        /*
         * Consumes a lexeme of given type containing given token
         * or throws ParserException.
         * Returns the consumed lexeme.
         * Moves to the next lexeme.
         */
        Lexeme Consume(string s, TokenType t)
        {
            var lexeme = currentLexeme;
            if (lexeme == null)
                throw new ParserException(string.Format("expected token {{{0}, \"{1}\"}}", t, s), null);
            if (lexeme.token != s || lexeme.type != t)
                throw new ParserException(string.Format("expected token {{{0}, \"{1}\"}}, got {2}", t, s, lexeme == null ? "null" : lexeme.ToString()), lexeme);
            NextLexeme();
            return lexeme;
        }

        /*
         * Below are functions that check if the contents of the Lexeme
         * match the given contents and return true if they do and false otherwise.
         */

        bool LexemeContains(TokenType t)
        {
            return currentLexeme != null && currentLexeme.type == t;
        }

        bool LexemeHasToken(string s)
        {
            return currentLexeme != null && currentLexeme.token == s;
        }

        bool LexemeContains(string s, TokenType t)
        {
            return LexemeHasToken(s) && LexemeContains(t);
        }

        /*
         * Below are the functions used to generate AST nodes.
         * They can throw ParserException to indicate of an error.
         * They can return null to indicate that lexeme did not match.
         * Otherwise they return an AST node.
         * They consume Lexemes.
         * 
         * The function names reflect the ones used in the grammar.
         */

        ASTNode NUM()
        {
            var node = new Number(currentLexeme.token);
            node.lexeme = Consume(TokenType.NUMBER);
            return node;
        }

        ASTNode STR()
        {
            var node = new String(currentLexeme.token);
            node.lexeme = Consume(TokenType.STRING);
            return node;
        }

        Identifier IDENT()
        {
            var node = new Identifier(currentLexeme.token);
            node.lexeme = Consume(TokenType.IDENTIFIER);
            return node;
        }

        ASTNode TYPE()
        {
            var node = new TypeName(currentLexeme.token);
            node.lexeme = Consume(TokenType.KEYWORD);
            return node;
        }

        ASTNode UNARY()
        {
            var node = new UnaryOperator();
            node.lexeme = Consume(TokenType.OPERATOR);
            node.unaryOperator = node.lexeme.token;
            node.operand = OPND();
            return node;
        }

        ASTNode BINOP()
        {
            var node = new BinaryOperator();
            node.lexeme = Consume(TokenType.OPERATOR);
            node.binaryOperator = node.lexeme.token;
            node.rightOperand = OPND();
            return node;
        }

        ASTNode OPND()
        {
            switch (currentLexeme.type)
            {
                case TokenType.SEPARATOR:
                    {
                        Consume("(", TokenType.SEPARATOR);
                        var node = EXPR();
                        Consume(")", TokenType.SEPARATOR);
                        return node;
                    }
                case TokenType.NUMBER:
                    return NUM();
                case TokenType.STRING:
                    return STR();
                case TokenType.IDENTIFIER:
                    return IDENT();
            }
            throw new ParserException("operand expected", currentLexeme);
        }

        ASTNode EXPRTAIL()
        {
            if (LexemeContains(TokenType.OPERATOR))
                return BINOP();
            return null;
        }

        ASTNode EXPR()
        {
            if (LexemeContains(TokenType.OPERATOR))
                return UNARY();
            var node = new Expression();
            node.lexeme = currentLexeme;
            node.leftOperand = OPND();
            node.expressionTail = EXPRTAIL();
            return node;
        }

        ASTNode PRINT()
        {
            var node = new Print();
            node.lexeme = Consume("print", TokenType.KEYWORD);
            node.printedValue = EXPR();
            return node;
        }

        ASTNode ASSERT()
        {
            var node = new Assert();
            node.lexeme = Consume("assert", TokenType.KEYWORD);
            Consume("(", TokenType.SEPARATOR);
            node.condition = EXPR();
            Consume(")", TokenType.SEPARATOR);
            return node;
        }

        ASTNode VARTAIL()
        {
            if (LexemeContains(":=", TokenType.SEPARATOR))
            {
                Consume(":=", TokenType.SEPARATOR);
                return EXPR();
            }
            return null;
        }

        ASTNode VAR()
        {
            var node = new Declaration();
            node.lexeme = Consume("var", TokenType.KEYWORD);
            node.identifier = IDENT();
            Consume(":", TokenType.SEPARATOR);
            node.identifierType = TYPE();
            node.identifierValue = VARTAIL();
            return node;
        }

        ASTNode ASSIGN()
        {
            var node = new Assignment();
            node.identifier = IDENT();
            node.lexeme = Consume(":=", TokenType.SEPARATOR);
            node.value = EXPR();
            return node;
        }

        ASTNode READ()
        {
            var node = new Read();
            node.lexeme = Consume("read", TokenType.KEYWORD);
            node.identifierToRead = IDENT();
            return node;
        }

        ASTNode FORLOOP()
        {
            var node = new ForLoop();
            node.lexeme = Consume("for", TokenType.KEYWORD);
            node.loopVariableIdentifier = IDENT();
            Consume("in", TokenType.KEYWORD);
            node.beginValue = EXPR();
            Consume("..", TokenType.SEPARATOR);
            node.endValue = EXPR();
            Consume("do", TokenType.KEYWORD);
            node.statements = STMTS();
            if (node.statements == null) // no statements found inside for loop
                throw new ParserException(string.Format("statement expected in for loop"), currentLexeme);
            Consume("end", TokenType.KEYWORD);
            Consume("for", TokenType.KEYWORD);
            return node;
        }

        ASTNode STMT()
        {
            if (LexemeContains(TokenType.KEYWORD))
            {
                switch (currentLexeme.token)
                {
                    case "read":
                        return READ();
                    case "assert":
                        return ASSERT();
                    case "print":
                        return PRINT();
                    case "var":
                        return VAR();
                    case "for":
                        return FORLOOP();
                }
            }
            if (LexemeContains(TokenType.IDENTIFIER))
                return ASSIGN();
            return null; // was not a statement
        }

        ASTNode STMTSTAIL()
        {
            var node = new Statements();
            node.statement = STMT();
            if (node.statement == null)
                return null; // no more statements found
            Consume(";", TokenType.SEPARATOR);
            node.statementstail = STMTSTAIL();
            return node;
        }

        ASTNode STMTS()
        {
            var node = new Statements();
            node.statement = STMT();
            if (node.statement == null)
                return null; // not even a single statement found
            Consume(";", TokenType.SEPARATOR);
            node.statementstail = STMTSTAIL();
            return node;
        }

        /*
         * Generate root AST node of the program.
         */
        ASTNode PROG()
        {
            NextLexeme(); // get first lexeme
            ASTNode ast = STMTS();
            if (ast == null) // found no statements
                throw new ParserException(string.Format("statement expected"), currentLexeme);
            if (currentLexeme != null) // tokens exist after statements
                throw new ParserException("unexpected token after program statements", currentLexeme);
            return ast;
        }

        /*
         * Called after an error occurs.
         * Attempts to read more statements.
         * Prints any subsequent error and calls itself again.
         */
        void ERRORTAIL()
        {
            try
            {
                STMTSTAIL();
            }
            catch (ParserException e)
            {
                Error(e);
                ERRORTAIL();
            }
        }

        /*
         * Parses the token stream to generate an AST.
         * Returns ASTNode if eerything was successful and
         * null otherwise.
         */
        public ASTNode Parse()
        {
            try
            {
                return PROG();
            }
            catch (ParserException e)
            {
                Error(e);
                ERRORTAIL();
            }
            return null;
        }

        /*
         * Handles a ParserException.
         * Sets error indicator to true,
         * prints the error message and
         * skips to the beginning of the next statement.
         */
        void Error(ParserException e)
        {
            errored = true;
            if (e.lexeme == null)
                io.WriteLine("Parser error at <end of file>: {0}, current token: {1}", e.Message, e.lexeme.ToString());
            else
                io.WriteLine("Parser error at {0}: {1}, current token: {2}", e.lexeme.position, e.Message, e.lexeme.ToString());
            SkipToNextStatement();
        }

        /*
         * Skips to the beginning of the next statement.
         */
        void SkipToNextStatement()
        {
            while (currentLexeme != null && !LexemeContains(";", TokenType.SEPARATOR))
                NextLexeme();
            NextLexeme();
        }

        private Lexer lexer;
        private Lexeme currentLexeme = null;
        public bool errored { get; private set; } = false;
        private IO io;
    }
}
