using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SimpleSyntaxAnalyzer
{
    abstract class SyntaxNode
    {
        public SyntaxNode(Lexeme lexeme)
        {
            Lexeme = lexeme;
        }

        public Lexeme Lexeme { get; }

        abstract public void PrintTree(int depth = 0, string indent = "");

        abstract public bool HasChildren();
    }

    class BinOperNode : SyntaxNode
    {
        public BinOperNode(Lexeme lexeme, SyntaxNode left, SyntaxNode right)
            : base(lexeme)
        {
            Left = left;
            Right = right;
        }

        public SyntaxNode Left { get; }
        public SyntaxNode Right { get; }

        public override void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);
            if (Left is not null && Right is not null)
            {
                Console.Write(indent + "├──── ");
                Left.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

                Console.Write(indent + "└──── ");
                Right.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
            }
        }

        override public string ToString() => Lexeme.Source;

        public override bool HasChildren() =>
            Left is not null && Right is not null;
    }

    class NumberNode : SyntaxNode
    {
        public NumberNode(Lexeme lexeme) : base(lexeme) { }

        public override void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        override public string ToString() => $"{Lexeme.Value}";

        public override bool HasChildren()
        {
            return false;
        }
    }

    class IdentifireNode : SyntaxNode
    {
        public IdentifireNode(Lexeme lexeme) : base(lexeme) { }

        public override void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        override public string ToString() => $"{Lexeme.Value}";

        public override bool HasChildren()
        {
            return false;
        }
    }

}