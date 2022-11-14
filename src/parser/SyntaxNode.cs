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

        virtual public void PrintTree(int depth, List<int> bridge) { }
    }

    class BinOperNode : SyntaxNode
    {
        public BinOperNode(Lexeme lexeme, SyntaxNode left, SyntaxNode right)
            : base(lexeme, left, right) { }


        public override void PrintTree(int depth, List<int> bridge)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            int padding = 4 * depth == 0 ? 0 : 4;

            Console.WriteLine($"".PadLeft(padding, '─') + (depth > 0 ? $" {this}" : this));
            if (Left is not null && Right is not null)
            {
                string prefix = "";
                for (int i = 0; i < depth; i++)
                    prefix += (bridge.Contains(i) ? "│" : "").PadRight(6, ' ');

                var _ = new List<int>(bridge);
                if (Left.Left is not null)
                    _.Add(depth);

                Console.Write(prefix + "├");
                Left.PrintTree(depth + 1, _);

                Console.Write(prefix + "└");
                Right.PrintTree(depth + 1, bridge);
            }
        }

        override public string ToString() => Lexeme.Source;
        // override public string ToString() =>
        //     $"{Lexeme.Value} ( {Left?.ToString()}, {Right?.ToString()} )";
    }

    class NumberNode : SyntaxNode
    {
        public NumberNode(Lexeme lexeme) : base(lexeme, null, null) { }

        public override void PrintTree(int depth, List<int> bridge)
        {
            int padding = 4 * (depth - 1) == 0 ? 0 : 4 * (depth - 1);
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine($"".PadLeft(4, '─') + $" {this}");
        }

        override public string ToString() => $"{Lexeme.Value}";
    }

    class IdentifireNode : SyntaxNode
    {
        public IdentifireNode(Lexeme lexeme) : base(lexeme, null, null) { }

        public override void PrintTree(int depth, List<int> bridge)
        {
            int padding = 4 * (depth - 1) == 0 ? 0 : 4 * (depth - 1);
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine($"".PadLeft(4, '─') + $" {this}");
        }

        override public string ToString() => $"{Lexeme.Value}";
    }

}