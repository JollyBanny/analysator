using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.Exceptions
{
    class LexemeException : Exception
    {
        public LexemeException(Position pos, string message)
            : base($"{pos.Line}\t{pos.Ch}\t{message}")
        { }
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
    }
}