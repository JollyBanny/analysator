using LexicalAnalyzer;

namespace PascalCompiler.Exceptions
{
    class LexemeException : Exception
    {
        public LexemeException(Position pos, string message)
            : base($"{pos.Line}\t{pos.Ch}\t{message}")
        { }
    }

    class LexemeOverflowException : System.OverflowException
    {
        public LexemeOverflowException(Position pos)
            : base($"{pos.Line}\t{pos.Ch}\tType overflow")
        { }
    }
}