using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public class TypeNode : SyntaxNode
    {
        protected TypeNode(Lexeme? lexeme = null) : base(lexeme!) { }
        override public void PrintTree(int depth = 0, string indent = "") { }
    }

    public class IdentTypeNode : TypeNode
    {
        public IdentTypeNode(SyntaxNode typeIdent) : base(typeIdent.Lexeme)
        {
            TypeIdent = typeIdent;
        }

        public SyntaxNode TypeIdent { get; }

        override public void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class ArrayTypeNode : TypeNode
    {
        public ArrayTypeNode(Lexeme lexeme, List<SubrangeTypeNode> ranges, TypeNode type)
        : base(lexeme)
        {
            Ranges = ranges;
            Type = type;
        }

        public List<SubrangeTypeNode> Ranges { get; }
        public SyntaxNode Type { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            foreach (var range in Ranges)
            {
                Console.Write(indent + "├──── ");
                range.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
            }

            Console.Write(indent + "└──── ");
            Type.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class SubrangeTypeNode : TypeNode
    {
        public SubrangeTypeNode(Lexeme lexeme, ExprNode left, ExprNode right)
        : base(lexeme)
        {
            LeftBound = left;
            RightBound = right;
        }

        public ExprNode LeftBound { get; }
        public ExprNode RightBound { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            LeftBound.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            RightBound.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => Lexeme.Source;
    }

    public class RecordTypeNode : TypeNode
    {
        public RecordTypeNode(Lexeme lexeme, List<RecordFieldNode> fieldsList) : base(lexeme)
        {
            FieldsList = fieldsList;
        }

        public List<RecordFieldNode> FieldsList { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);
            for (int i = 0; i < FieldsList.Count; ++i)
            {
                if (i == FieldsList.Count - 1)
                {
                    Console.Write(indent + "└──── ");
                    FieldsList[i].PrintTree(depth + 1, indent + "".PadRight(6, ' '));
                }
                else
                {
                    Console.Write(indent + "├──── ");
                    FieldsList[i].PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
                }
            }
        }

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class RecordFieldNode : TypeNode
    {
        public RecordFieldNode(Lexeme lexeme, List<IdentNode> identsList, TypeNode type)
        : base(lexeme)
        {
            IdentsList = identsList;
            Type = type;
        }

        public List<IdentNode> IdentsList { get; }
        public TypeNode Type { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);
            for (int i = 0; i < IdentsList.Count; ++i)
            {
                Console.Write(indent + "├──── ");
                IdentsList[i].PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
            }
            Console.Write(indent + "└──── ");
            Type.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => Lexeme.Source;
    }

    public class ParamArrayTypeNode : TypeNode
    {
        public ParamArrayTypeNode(Lexeme lexeme, TypeNode type) : base(lexeme)
        {
            Type = type;
        }

        public TypeNode Type { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);
            Console.Write(indent + "└──── ");
            Type.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => Lexeme.Value.ToString()!;
    }
}