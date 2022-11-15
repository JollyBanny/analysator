using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.Exceptions
{
    class LexicalException : Exception
    {
        public LexicalException(Position pos, string message)
            : base($"{pos.Line}\t{pos.Ch}\t{message}")
        { }

        public LexicalException(LexicalException exception)
            : base(string.Empty, exception)
        {
            throw new Exception(exception.Message);
        }
    }

    class LexemeOverflowException : OverflowException
    {
        public LexemeOverflowException(Position pos)
            : base($"{pos.Line}\t{pos.Ch}\tType overflow")
        { }
    }

    class SyntaxException : Exception
    {
        public SyntaxException(Position pos, string message)
            : base($"{pos.Line}\t{pos.Ch}\t{message}")
        { }

        public SyntaxException(SyntaxException exception)
            : base(string.Empty, exception)
        {
            throw new Exception(exception.Message);
        }
    }
}