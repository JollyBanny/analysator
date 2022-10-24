namespace LexicalAnalyzer
{
    class LexemException : Exception
    {
        public LexemException(Position pos, string message)
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