using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public class StmtNode : SyntaxNode
    {
        protected StmtNode(Lexeme? lexeme = null) : base(lexeme!) { }
        override public void PrintTree(int depth = 0, string indent = "") { }
    }

    public class CompoundStmtNode : StmtNode
    {
        public CompoundStmtNode(List<StmtNode> statements) : base()
        {
            Statements = statements;
        }

        public List<StmtNode> Statements { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            for (int i = 0; i < Statements.Count; ++i)
            {
                if (i == Statements.Count - 1)
                {
                    Console.Write(indent + "└──── ");
                    Statements[i].PrintTree(depth + 1, indent + "".PadRight(6, ' '));
                }
                else
                {
                    Console.Write(indent + "├──── ");
                    Statements[i].PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
                }
            }
        }

        override public string ToString() => "Compound statement";
    }

    public class EmptyStmtNode : StmtNode
    {
        public EmptyStmtNode() : base()
        { }

        override public void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        override public string ToString() => "Empty statement";
    }

    public class CallStmtNode : StmtNode
    {
        public CallStmtNode(ExprNode expression) : base()
        {
            Expression = expression;
        }

        public ExprNode Expression { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "└──── ");
            Expression.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => "Call statement";
    }

    public class AssignStmtNode : StmtNode
    {
        public AssignStmtNode(Lexeme lexeme, ExprNode left, ExprNode right) : base(lexeme)
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

    public class IfStmtNode : StmtNode
    {
        public IfStmtNode(Lexeme lexeme, ExprNode condition, StmtNode ifPart,
            StmtNode? elsePart) : base(lexeme)
        {
            Condition = condition;
            IfPart = ifPart;
            ElsePart = elsePart;
        }

        public ExprNode Condition { get; }
        public StmtNode IfPart { get; }
        public StmtNode? ElsePart { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Condition.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            if (ElsePart is not null)
            {
                Console.Write(indent + "├──── ");
                IfPart.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

                Console.Write(indent + "└──── ");
            }
            else
            {
                Console.Write(indent + "└──── ");
                IfPart.PrintTree(depth + 1, indent + "".PadRight(6, ' '));

            }

            ElsePart?.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class WhileStmtNode : StmtNode
    {
        public WhileStmtNode(Lexeme lexeme, ExprNode condition, StmtNode statement)
        : base(lexeme)
        {
            Condition = condition;
            Statement = statement;
        }

        public ExprNode Condition { get; }
        public StmtNode Statement { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Condition.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            Statement.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class RepeatStmtNode : StmtNode
    {
        public RepeatStmtNode(Lexeme lexeme, ExprNode condition, List<StmtNode> statements)
        : base(lexeme)
        {
            Condition = condition;
            Statements = statements;
        }

        public ExprNode Condition { get; }
        public List<StmtNode> Statements { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Condition.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            for (int i = 0; i < Statements.Count; ++i)
            {
                if (i == Statements.Count - 1)
                {
                    Console.Write(indent + "└──── ");
                    Statements[i].PrintTree(depth + 1, indent + "".PadRight(6, ' '));
                }
                else
                {
                    Console.Write(indent + "├──── ");
                    Statements[i].PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
                }
            }
        }

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class ForStmtNode : StmtNode
    {
        public ForStmtNode(Lexeme lexeme, IdentNode ctrlIdent, ForRangeNode forRange,
            StmtNode statement) : base(lexeme)
        {
            CtrlIdent = ctrlIdent;
            ForRange = forRange;
            Statement = statement;
        }

        public IdentNode CtrlIdent { get; }
        public ForRangeNode ForRange { get; }
        public StmtNode Statement { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            CtrlIdent.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "├──── ");
            ForRange.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            Statement.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class ForRangeNode : SyntaxNode
    {
        public ForRangeNode(Lexeme lexeme, ExprNode startValue, ExprNode finalValue)
        : base(lexeme)
        {
            StartValue = startValue;
            FinalValue = finalValue;
        }

        public ExprNode StartValue { get; }
        public ExprNode FinalValue { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            StartValue.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            FinalValue.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => Lexeme.Value.ToString()!;
    }
}