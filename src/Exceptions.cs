namespace LexicalAnalyzer.Exceptions
{
    class LexemeException : Exception
    {
        public LexemeException(Position pos, string message)
            : base($"{pos.line} \t {pos.ch} \t {message}")
        { }
    }

    class LexemeOverflowException : System.OverflowException
    {
        public LexemeOverflowException(Position pos)
            : base($"{pos.line} \t {pos.ch} \t Type overflow")
        { }
    }
}