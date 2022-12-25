using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.Exceptions
{
    class LexicalException : Exception
    {
        public LexicalException(Position pos, string message)
            : base($"[Line: {pos.Line}; Column: {pos.Ch}]\t{message}")
        {
        }
    }

    class LexemeOverflowException : OverflowException
    {
        public LexemeOverflowException(Position pos)
            : base($"[Line: {pos.Line}; Column: {pos.Ch}]\tType overflow")
        {
        }
    }

    class SyntaxException : Exception
    {
        public SyntaxException(Position pos, string message)
            : base($"[Line: {pos.Line}; Column: {pos.Ch}]\t{message}")
        {
        }

        public SyntaxException(string message) : base(message)
        {
        }
    }

    class SemanticException : Exception
    {
        public SemanticException(Position pos, string message)
            : base($"[Line: {pos.Line}; Column: {pos.Ch}]\t{message}")
        {
        }

        public SemanticException(string message) : base(message)
        {
        }
    }
}