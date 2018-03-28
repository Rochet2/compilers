using System;
using System.Collections.Generic;

namespace Interpreter
{
    /*
     * Types of all of the tokens accepted
     */
    public enum TokenType
    {
        KEYWORD,
        IDENTIFIER,
        SEPARATOR,
        OPERATOR,
        NUMBER,
        STRING,
        BOOLEAN,
        COMMENT,
        BLOCKCOMMENT,
    }

    /*
     * A class representing a lexeme with type, token and position.
     */
    public class Lexeme
    {
        public TokenType type;
        public string token;
        public InputBuffer.Position position;

        public Lexeme(TokenType type, InputBuffer.Position position, string token)
        {
            this.type = type;
            this.token = token;
            this.position = position;
        }

        /*
         * Returns the a string containing lexeme type and token.
         */
        public override string ToString()
        {
            return string.Format("{{{0} \"{1}\"}}", type, token);
        }
    }

    /*
     * Lexer, or Scanner, implements the tokenizing of the input.
     * Reads the input and outputs Lexemes.
     */
    public class Lexer
    {
        /*
         * An exception thrown by the Lexer on unexpected input.
         * Contains the error message and input position of the error.
         */
        class LexerException : Exception
        {
            public LexerException(string message, InputBuffer.Position position) : base(message)
            {
                this.position = position;
            }

            public InputBuffer.Position position { get; }

            public override string ToString()
            {
                return this.Message;
            }
        }

        public Lexer(InputBuffer input, IO io)
        {
            this.input = input;
            this.io = io;
        }

        /*
         * A list of keywords used to differentiate indentifiers
         * from keywords.
         */
        public static List<string> keywords { get; } = new List<string> {
            "var",
            "for",
            "end",
            "in",
            "do",
            "read",
            "print",
            "int",
            "string",
            "bool",
            "assert",
        };

        /*
         * Consumes input and matches the input characters to given token.
         * Returns true if whole token is matched.
         * Returns false at the first occurrence of the token not matching.
         */
        public bool ExpectToken(string token)
        {
            if (token.Length <= 0)
                return true;
            if (input.PeekCharacter() < 0)
                return false;
            int i = 0;
            while (i < token.Length)
            {
                if (token[i] != input.PeekCharacter())
                    return false;
                ++i;
                if (!input.Next())
                    return i >= token.Length;
            }
            return true;
        }

        /*
         * Expect functions return Lexemes of the tokens they expect.
         * If the input contains any unexpected characters,
         * an exception is thrown.
         * 
         * Expected regex: %d+
         */
        public Lexeme ExpectNumber()
        {
            InputBuffer.Position pos = input.GetPosition();
            System.String s = "";
            if (input.HasCharacter())
            {
                do
                {
                    // Match any numbers
                    if (!Char.IsDigit(input.PeekCharacter()))
                        break;
                    s += input.PeekCharacter();
                } while (input.Next());
                if (s.Length > 0)
                {
                    // If matched one or more numbers..
                    int parsed = 0;
                    // make sure the number is convertible to integer
                    if (!int.TryParse(s, out parsed))
                        throw new LexerException("too high constant value", pos);
                    return new Lexeme(TokenType.NUMBER, pos, s);
                }
            }
            // Did not match any number
            throw new LexerException("number expected", pos);
        }

        /*
         * Expect functions return Lexemes of the tokens they expect.
         * If the input contains any unexpected characters,
         * an exception is thrown.
         * 
         * Expected regex: //[^\n]*
         */
        public Lexeme ExpectComment()
        {
            InputBuffer.Position pos = input.GetPosition();
            if (!ExpectToken("//")) // Expect comment start
                throw new LexerException("comment expected", pos);
            string s = "";
            if (input.HasCharacter())
            {
                do
                {
                    // Match anything until a new line or end of file reached
                    if (input.PeekCharacter() == '\n')
                        break;
                    s += input.PeekCharacter();
                } while (input.Next());
            }
            return new Lexeme(TokenType.COMMENT, pos, s);
        }

        /*
         * Expect functions return Lexemes of the tokens they expect.
         * If the input contains any unexpected characters,
         * an exception is thrown.
         * 
         * Expected regex: "(\\[^\n]|[^"\n])*"
         */
        public Lexeme ExpectString()
        {
            InputBuffer.Position pos = input.GetPosition();
            if (!ExpectToken("\"")) // Expect string starting quote
                throw new LexerException("string expected", pos);
            string s = "";
            if (input.HasCharacter())
            {
                do
                {
                    if (input.PeekCharacter() == '\n') // unexpected newline in middle of string
                    {
                        break;
                    }
                    if (input.PeekCharacter() == '\\') // escaped character
                    {
                        // skip escape character
                        if (!input.Next())
                            break; // end of input after escape character
                        // handle escaped character
                        switch (input.PeekCharacter())
                        {
                            case 'n': s += '\n'; break;
                            case 't': s += '\t'; break;
                            case '"': s += '"'; break;
                            default:
                                throw new LexerException(
                                    string.Format("unrecognized escape character {0}", input.PeekCharacter()),
                                    input.GetPosition()
                                );
                        }
                        continue;
                    }
                    if (input.PeekCharacter() == '"') // string end quote
                    {
                        input.Next();
                        return new Lexeme(TokenType.STRING, pos, s);
                    }
                    s += input.PeekCharacter();
                } while (input.Next());
            }
            throw new LexerException(string.Format("unexpected end of string starting at {0}", pos.ToString()), input.GetPosition());
        }

        /*
         * Expect functions return Lexemes of the tokens they expect.
         * If the input contains any unexpected characters,
         * an exception is thrown.
         * 
         * Expected regular definition: comment
         */
        // comment := "/*" commentend
        // commentend := ([^/][^*] | comment)* "*/"
        public Lexeme ExpectBlockComment()
        {
            InputBuffer.Position pos = input.GetPosition();
            if (!ExpectToken("/*")) // block comment start
                throw new LexerException("blockcomment expected", pos);
            string s = "";
            int nestedComments = 0; // counter for level of comment nesting
            if (input.HasCharacter())
            {
                do
                {
                    if (input.HasNextCharacter() && input.PeekCharacter() == '/' && input.PeekNext() == '*')
                    {
                        // nested comment start found
                        // read /*
                        s += input.PeekCharacter();
                        if (input.Next())
                            s += input.PeekCharacter();
                        ++nestedComments;
                        continue;
                    }
                    if (input.HasNextCharacter() && input.PeekCharacter() == '*' && input.PeekNext() == '/')
                    {
                        // comment end found
                        if (nestedComments <= 0)
                        {
                            // the end is final comment end
                            // skip */
                            input.Next();
                            input.Next();
                            return new Lexeme(TokenType.BLOCKCOMMENT, pos, s);
                        }
                        // the end is nested
                        // read */
                        s += input.PeekCharacter();
                        if (input.Next())
                            s += input.PeekCharacter();
                        --nestedComments;
                        continue;
                    }
                    s += input.PeekCharacter();
                } while (input.Next());
            }
            throw new LexerException(string.Format("unexpected end of blockcomment starting at {0}", pos.ToString()), input.GetPosition());
        }

        /*
         * Expect functions return Lexemes of the types they expect.
         * If the input contains any unexpected characters,
         * an exception is thrown.
         * 
         * Expected regex: (\a|_)(\a|\d|_)*
         */
        public Lexeme ExpectIdentifierOrKeyword()
        {
            InputBuffer.Position pos = input.GetPosition();
            if (!input.HasCharacter()) // check end of file
                throw new LexerException("identifier or keyword expected", pos);
            if (!(Char.IsLetter(input.PeekCharacter()))) // identifier starts with a letter
                throw new LexerException("identifier or keyword must start with a letter", pos);
            string s = "";
            do
            {
                // read any letter, digit or underscore
                char c = input.PeekCharacter();
                if (!(Char.IsLetterOrDigit(c) || c == '_'))
                    break;
                s += c;
            } while (input.Next());
            return new Lexeme(keywords.Contains(s) ? TokenType.KEYWORD : TokenType.IDENTIFIER, pos, s);
        }

        /*
         * Tries to read the input to generate a new Lexeme.
         * Returns true if a new Lexeme was generated and false otherwise.
         * Can throw LexerException if Lexer encountered unexpected tokens.
         */
        public bool LexNext()
        {
            // Fetch current and next character from input
            if (!input.HasCharacter())
                return false;
            char current = input.PeekCharacter();
            char next = input.HasNextCharacter() ? input.PeekNext() : ' '; // Whitespace used as next character at end of file

            try
            {
                // Use conditional blocks as a kind of switch or lookup table
                if (current == '/' && next == '/')
                {
                    ExpectComment();
                    return LexNext();
                }
                else if (current == '/' && next == '*')
                {
                    ExpectBlockComment();
                    return LexNext();
                }
                else if (Char.IsLetter(current))
                {
                    lexemes.Add(ExpectIdentifierOrKeyword());
                }
                else if (
                  current == ':' && next == '=')
                {
                    lexemes.Add(new Lexeme(TokenType.SEPARATOR, input.GetPosition(), current.ToString() + next.ToString()));
                    input.Next();
                    input.Next();
                }
                else if (
                  current == '.' && next == '.')
                {
                    lexemes.Add(new Lexeme(TokenType.SEPARATOR, input.GetPosition(), current.ToString() + next.ToString()));
                    input.Next();
                    input.Next();
                }
                else if (
                  current == '(' ||
                  current == ')' ||
                  current == ':' ||
                  current == ';')
                {
                    lexemes.Add(new Lexeme(TokenType.SEPARATOR, input.GetPosition(), current.ToString()));
                    input.Next();
                }
                else if (
                  current == '<' ||
                  current == '=' ||
                  current == '!' ||
                  current == '&' ||
                  current == '+' ||
                  current == '-' ||
                  current == '/' ||
                  current == '*')
                {
                    lexemes.Add(new Lexeme(TokenType.OPERATOR, input.GetPosition(), current.ToString()));
                    input.Next();
                }
                else if (Char.IsDigit(current))
                {
                    lexemes.Add(ExpectNumber());
                }
                else if (current == '"')
                {
                    lexemes.Add(ExpectString());
                }
                else if (Char.IsWhiteSpace(current))
                {
                    // skip
                    input.Next();
                    return LexNext();
                }
                else
                {
                    InputBuffer.Position pos = input.GetPosition();
                    input.Next();
                    throw new LexerException(string.Format("unrecognized token beginning with {0} followed by {1}", current, next), pos);
                }
            }
            catch (LexerException e)
            {
                // an error occurred
                // set errored to true, print the error and
                //  continue on next line.
                errored = true;
                io.WriteLine("Lexical error at {0}: {1}", e.position, e.ToString());
                SkipToNextLine();
                return LexNext();
            }
            return true;
        }

        /*
         * Calls LexNext as long as we have input
         * and new Lexemes are being produced.
         */
        public void LexAll()
        {
            while (input.HasCharacter())
            {
                if (!LexNext())
                    break;
            }
        }

        /*
         * Returns true if the Lexer has a Lexeme it already read from input,
         * false otherwise.
         */
        public bool HasNext()
        {
            return lexemes.Count > 0;
        }

        /*
         * Returns the next Lexeme.
         * 
         * Calls LexNext to fill internal storage if it was empty.
         * Pops a Lexeme from the internal storage and returns it.
         * Returns null if no Lexeme can be produced.
         */
        public Lexeme NextLexeme()
        {
            if (!HasNext() && !LexNext())
                return null;
            Lexeme lexeme = lexemes[0];
            lexemes.RemoveAt(0);
            return lexeme;
        }

        /*
         * Consumes input until end of file or new line is found
         */
        private void SkipToNextLine()
        {
            while (input.HasCharacter() && input.PeekCharacter() != '\n')
                input.Next();
        }

        /*
         * List of Lexeme produced so far.
         */
        private List<Lexeme> lexemes = new List<Lexeme>();

        /*
         * Boolean value indicating if errors have occurred when producing
         * Lexemes.
         */
        public bool errored { get; private set; } = false;
        private InputBuffer input;
        private IO io;
    }
}
