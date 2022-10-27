namespace LexicalAnalyzer
{
    class LexemeException : Exception
    {
        public LexemeException(Position pos, string message)
            : base($"{pos.line} \t {pos.ch} \t {message}")
        { }
    }

    class NumberException : OverflowException
    {
        public NumberException(Position pos)
            : base($"{pos.line} \t {pos.ch} \t Type overflow")
        { }
    }
}