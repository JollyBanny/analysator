using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer
{
    abstract class SyntaxNode
    {
        public SyntaxNode(Lexeme lexeme)
        {
            Lexeme = lexeme;
        }

        public Lexeme Lexeme { get; }

        virtual public void PrintTree(int depth = 0, string bridges = "") { }

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

        public override void PrintTree(int depth, string bridges)
        {
            var padding = depth > 0 ? 4 : 0;
            Console.WriteLine(
                $"".PadLeft(padding, '─') + (depth > 0 ? $" {this}" : this));

            if (Left is not null && Right is not null)
            {
                var prefix = "";
                for (int i = 0; i < depth; i++)
                    prefix += (bridges.Contains(i.ToString()) ? "│" : "").PadRight(6, ' ');

                var _ = Left.HasChildren() || Right.HasChildren() ?
                    bridges + depth.ToString() : bridges;

                Console.Write(prefix + "├");
                Left.PrintTree(depth + 1, _);

                Console.Write(prefix + "└");
                Right.PrintTree(depth + 1, bridges);
            }
        }

        override public string ToString() => Lexeme.Source;

        public override bool HasChildren() =>
            Left is not null && Right is not null;
    }

    class NumberNode : SyntaxNode
    {
        public NumberNode(Lexeme lexeme) : base(lexeme) { }

        public override void PrintTree(int depth, string bridges)
        {
            var prefix = depth > 0 ? $" ".PadLeft(4, '─') : "";
            Console.WriteLine(prefix + this);
        }

        override public string ToString() => $"{Lexeme.Value}";

        public override bool HasChildren()
        {
            return false;
        }
    }

    class IdentifireNode : SyntaxNode
    {
        public IdentifireNode(Lexeme lexeme) : base(lexeme) { }

        public override void PrintTree(int depth, string bridges)
        {
            var prefix = depth > 0 ? $" ".PadLeft(4, '─') : "";
            Console.WriteLine(prefix + this);
        }

        override public string ToString() => $"{Lexeme.Value}";

        public override bool HasChildren()
        {
            return false;
        }
    }

}