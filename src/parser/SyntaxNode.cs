using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer
{
    class SyntaxNode
    {
        public SyntaxNode(Lexeme lexeme, SyntaxNode? left, SyntaxNode? right)
        {
            Lexeme = lexeme;
            Left = left;
            Right = right;
        }

        public Lexeme Lexeme { get; }
        public SyntaxNode? Left { get; }
        public SyntaxNode? Right { get; }

        virtual public void PrintTree(int depth = 0, string bridges = "") { }
    }

    class BinOperNode : SyntaxNode
    {
        public BinOperNode(Lexeme lexeme, SyntaxNode left, SyntaxNode right)
            : base(lexeme, left, right) { }

        public override void PrintTree(int depth, string bridges)
        {
            int padding = 4 * depth == 0 ? 0 : 4;
            Console.WriteLine(
                $"".PadLeft(padding, '─') + (depth > 0 ? $" {this}" : this));

            if (Left is not null && Right is not null)
            {
                string prefix = "";
                for (int i = 0; i < depth; i++)
                    prefix += (bridges.Contains(i.ToString()) ? "│" : "").PadRight(6, ' ');

                var _ = Left.Left is not null ? bridges + depth.ToString() : bridges;

                Console.Write(prefix + "├");
                Left.PrintTree(depth + 1, _);

                Console.Write(prefix + "└");
                Right.PrintTree(depth + 1, bridges);
            }
        }

        override public string ToString() => Lexeme.Source;
    }

    class NumberNode : SyntaxNode
    {
        public NumberNode(Lexeme lexeme) : base(lexeme, null, null) { }

        public override void PrintTree(int depth, string bridges)
        {
            var prefix = depth > 0 ? $" ".PadLeft(4, '─') : "";
            Console.WriteLine(prefix + this);
        }

        override public string ToString() => $"{Lexeme.Value}";
    }

    class IdentifireNode : SyntaxNode
    {
        public IdentifireNode(Lexeme lexeme) : base(lexeme, null, null) { }

        public override void PrintTree(int depth, string bridges)
        {
            var prefix = depth > 0 ? $" ".PadLeft(4, '─') : "";
            Console.WriteLine(prefix + this);
        }

        override public string ToString() => $"{Lexeme.Value}";
    }

}