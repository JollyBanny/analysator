using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public class ExprNode : SyntaxNode
    {
        protected ExprNode(Lexeme? lexeme = null) : base(lexeme!) { }
        public override void PrintTree(int depth = 0, string indent = "") { }
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

        public override void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Left.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            Right.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        public override string ToString() => Lexeme.Source;
    }

    public class UnaryOperNode : ExprNode
    {
        public UnaryOperNode(Lexeme lexeme, ExprNode expr) : base(lexeme)
        {
            Expr = expr;
        }
        public ExprNode Expr { get; }

        public override void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "└──── ");
            Expr.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        public override string ToString() => Lexeme.Source;
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

        public override void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Record.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            Field.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        public override string ToString() => "record access";
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

        public override void PrintTree(int depth, string indent)
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

        public override string ToString() => "array access";
    }

    public class FunctionCallNode : ExprNode
    {
        public FunctionCallNode(IdentNode funcIdent, List<ExprNode> args)
        : base(funcIdent.Lexeme)
        {
            Args = args;
        }

        public List<ExprNode> Args { get; }

        public override void PrintTree(int depth, string indent)
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

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class CustomCallNode : FunctionCallNode
    {
        public CustomCallNode(IdentNode funcIdent, List<ExprNode> args)
        : base(funcIdent, args)
        {

        }
    }

    public class WriteCallNode : FunctionCallNode
    {
        public WriteCallNode(IdentNode funcIdent, List<ExprNode> args, bool newLine)
        : base(funcIdent, args)
        {
            NewLine = newLine;
        }

        public bool NewLine { get; }
    }

    public class IdentNode : ExprNode
    {
        public IdentNode(Lexeme lexeme) : base(lexeme) { }

        public override void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class ConstantNode : ExprNode
    {
        protected ConstantNode(Lexeme lexeme) : base(lexeme) { }

        public override void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class ConstIntegerLiteral : ConstantNode
    {
        public ConstIntegerLiteral(Lexeme lexeme) : base(lexeme) { }
    }

    public class ConstDoubleLiteral : ConstantNode
    {
        public ConstDoubleLiteral(Lexeme lexeme) : base(lexeme) { }
    }

    public class ConstCharLiteral : ConstantNode
    {
        public ConstCharLiteral(Lexeme lexeme) : base(lexeme) { }

        public override string ToString() => $"'{Lexeme.Value.ToString()!}'";
    }

    public class ConstStringLiteral : ConstantNode
    {
        public ConstStringLiteral(Lexeme lexeme) : base(lexeme) { }

        public override string ToString() => $"'{Lexeme.Value.ToString()!}'";
    }

    public class ConstBooleanLiteral : ConstantNode
    {
        public ConstBooleanLiteral(Lexeme lexeme) : base(lexeme) { }
    }

    public class Nil : ConstantNode
    {
        public Nil(Lexeme lexeme) : base(lexeme) { }
    }
}