using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer
{
    class SyntaxNode
    {
        public SyntaxNode(Lexeme lexeme) { Lexeme = lexeme; }

        protected Lexeme Lexeme { get; }
    }

    class BinOperNode : SyntaxNode
    {
        public BinOperNode(Lexeme lexeme, SyntaxNode left, SyntaxNode right) : base(lexeme)
        {
            _Left = left;
            _Right = right;
        }

        private SyntaxNode? _Left { get; }
        private SyntaxNode? _Right { get; }

        override public string ToString() =>
            $"{Lexeme.Value} ( {_Left?.ToString()}, {_Right?.ToString()} )";
    }

    class NumberNode : SyntaxNode
    {
        public NumberNode(Lexeme lexeme) : base(lexeme) { }

        override public string ToString() => $"{Lexeme.Value}";
    }

    class IdentifireNode : SyntaxNode
    {
        public IdentifireNode(Lexeme lexeme) : base(lexeme) { }

        override public string ToString() => $"{Lexeme.Value}";
    }

}