using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public class ExprNode : SyntaxNode
    {
        protected ExprNode(Lexeme? lexeme = null) : base(lexeme!) { }
        override public void PrintTree(int depth = 0, string indent = "") { }
    }

    public class BinOperNode : ExprNode
    {
        public BinOperNode(Lexeme lexeme, ExprNode left, ExprNode right)
        : base(lexeme)
        {
            Left = left;
            Right = right;
        }

        public ExprNode Left { get; }
        public ExprNode Right { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Left.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            Right.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => Lexeme.Source;
    }

    public class UnaryOperNode : ExprNode
    {
        public UnaryOperNode(Lexeme lexeme, ExprNode expr) : base(lexeme)
        {
            Expr = expr;
        }
        public ExprNode Expr { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "└──── ");
            Expr.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => Lexeme.Source;
    }

    public class RecordAccessNode : ExprNode
    {
        public RecordAccessNode(ExprNode record, ExprNode field)
        : base()
        {
            Record = record;
            Field = field;
        }

        public ExprNode Record { get; }
        public ExprNode Field { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Record.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            Field.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => "record access";
    }

    public class ArrayAccessNode : ExprNode
    {
        public ArrayAccessNode(ExprNode arrayIdent, List<ExprNode> accessExprs)
        : base()
        {
            ArrayIdent = arrayIdent;
            AccessExprs = accessExprs;
        }

        public ExprNode ArrayIdent { get; }
        public List<ExprNode> AccessExprs { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            ArrayIdent.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            for (int i = 0; i < AccessExprs.Count; ++i)
            {
                if (i == AccessExprs.Count - 1)
                {
                    Console.Write(indent + "└──── ");
                    AccessExprs[i].PrintTree(depth + 1, indent + "".PadRight(6, ' '));
                }
                else
                {
                    Console.Write(indent + "├──── ");
                    AccessExprs[i].PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
                }
            }
        }

        override public string ToString() => "array access";
    }

    public class FunctionCallNode : ExprNode
    {
        public FunctionCallNode(IdentNode funcIdent, List<ExprNode> args)
        : base(funcIdent.Lexeme)
        {
            Args = args;
        }

        public List<ExprNode> Args { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            for (int i = 0; i < Args.Count; ++i)
            {
                if (i == Args.Count - 1)
                {
                    Console.Write(indent + "└──── ");
                    Args[i].PrintTree(depth + 1, indent + "".PadRight(6, ' '));
                }
                else
                {
                    Console.Write(indent + "├──── ");
                    Args[i].PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
                }
            }
        }

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class WriteCallNode : FunctionCallNode
    {
        public WriteCallNode(IdentNode arrayIdent, List<ExprNode> args, bool newLine)
        : base(arrayIdent, args)
        {
            NewLine = newLine;
        }

        public bool NewLine { get; }

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class IdentNode : ExprNode
    {
        public IdentNode(Lexeme lexeme) : base(lexeme) { }

        override public void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class ConstIntegerLiteral : ExprNode
    {
        public ConstIntegerLiteral(Lexeme lexeme) : base(lexeme) { }

        override public void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class ConstDoubleLiteral : ExprNode
    {
        public ConstDoubleLiteral(Lexeme lexeme) : base(lexeme) { }

        override public void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class ConstCharLiteral : ExprNode
    {
        public ConstCharLiteral(Lexeme lexeme) : base(lexeme) { }

        override public void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class ConstStringLiteral : ExprNode
    {
        public ConstStringLiteral(Lexeme lexeme) : base(lexeme) { }

        override public void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        override public string ToString() => Lexeme.Value.ToString()!;
    }
}