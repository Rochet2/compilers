using System;
using System.IO;

namespace Interpreter
{
    /*
     * Class that handles buffering input stream.
     * The buffer holds only given amount of input characters at a time.
     */
    public class InputBuffer
    {
        /*
         * Utility function to convert string to a Stream
         */
        private static MemoryStream ToStream(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /*
         * Utility function to convert a string to an InputBuffer
         * where bufferSize is the size of the buffer.
         */
        public static InputBuffer ToInputBuffer(string str, int bufferSize = 2)
        {
            return new InputBuffer(ToStream(str), bufferSize);
        }

        /*
         * Creates new input buffer that uses the stream as the source of
         * input and the bufferSize as the size of the buffer
         */
        public InputBuffer(Stream stream, int bufferSize)
        {
            this.stream = stream;
            buffer = new int[bufferSize];
            for (int i = 0; i < buffer.Length; ++i)
                Next();
            position = new Position();
        }

        /*
         * Returns true if the buffer contains a valid current character,
         * false otherwise.
         */
        public bool HasCharacter()
        {
            return buffer[0] >= 0;
        }

        /*
         * Returns true if the buffer contains a valid next character,
         * false otherwise.
         */
        public bool HasNextCharacter()
        {
            return buffer[1] >= 0;
        }

        /*
         * Reads the next character from input stream
         * and updates the buffer and position information.
         * Returns true if the buffer contains a valid current character,
         * false otherwise.
         */
        public bool Next()
        {
            int previous = buffer[0];
            for (int i = 1; i < buffer.Length; ++i)
                buffer[i - 1] = buffer[i];
            buffer[buffer.Length - 1] = stream.ReadByte();

            ++position.column;
            if (previous == '\n')
            {
                ++position.line;
                position.column = 1;
            }
            return HasCharacter();
        }

        /*
         * Peeks the character in the buffer at given index.
         * Throws IndexOutOfRangeException if invalid index given.
         * Throws OverFlowException if value at the index is not convertible
         * to a char.
         */
        public char PeekCharacter(int index = 0)
        {
            if (index >= buffer.Length)
                throw new IndexOutOfRangeException("invalid peek index");
            return Convert.ToChar(buffer[index]);
        }

        /*
         * Utility function to peek the next character.
         */
        public char PeekNext()
        {
            return PeekCharacter(1);
        }

        /*
         * Returns a copy of the current position information.
         */
        public Position GetPosition()
        {
            return position.Copy();
        }

        /*
         * Position information class for the buffer
         */
        public class Position
        {
            public int line = 1;
            public int column = 1;

            /*
             * Returns a copy of the Position.
             */
            public Position Copy()
            {
                // Shallow copy.
                return (Position)this.MemberwiseClone();
            }

            /*
             * Returns the position in string format containing
             * the line and column.
             */
            public override string ToString()
            {
                return string.Format("{0}:{1}", line, column);
            }
        }

        private Position position = new Position();
        private int[] buffer;
        private Stream stream;
    }
}
