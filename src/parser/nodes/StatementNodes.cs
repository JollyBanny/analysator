using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.Visitor;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public abstract class StmtNode : SyntaxNode
    {
        protected StmtNode(Lexeme? lexeme = null) : base(lexeme) { }
    }

    public class CompoundStmtNode : StmtNode
    {
        public CompoundStmtNode(List<StmtNode> stmts) : base()
        {
            Statements = stmts;
        }

        public List<StmtNode> Statements { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => "Compound statement";
    }

    public class EmptyStmtNode : StmtNode
    {
        public EmptyStmtNode() : base()
        { }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => "Empty statement";
    }

    public class CallStmtNode : StmtNode
    {
        public CallStmtNode(ExprNode expr) : base()
        {
            Expression = expr;
        }

        public ExprNode Expression { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => "Call statement";
    }

    public class AssignStmtNode : StmtNode
    {
        public AssignStmtNode(Lexeme lexeme, ExprNode left, ExprNode right) : base(lexeme)
        {
            Left = left;
            Right = right;
        }

        public ExprNode Left { get; }
        public ExprNode Right { get; set; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Source;
    }

    public class IfStmtNode : StmtNode
    {
        public IfStmtNode(Lexeme lexeme, ExprNode cond, StmtNode ifPart,
            StmtNode? elsePart) : base(lexeme)
        {
            Condition = cond;
            IfPart = ifPart;
            ElsePart = elsePart;
        }

        public ExprNode Condition { get; }
        public StmtNode IfPart { get; }
        public StmtNode? ElsePart { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class WhileStmtNode : StmtNode
    {
        public WhileStmtNode(Lexeme lexeme, ExprNode cond, StmtNode stmts) : base(lexeme)
        {
            Condition = cond;
            Statement = stmts;
        }

        public ExprNode Condition { get; }
        public StmtNode Statement { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class RepeatStmtNode : StmtNode
    {
        public RepeatStmtNode(Lexeme lexeme, ExprNode cond, List<StmtNode> stmts) : base(lexeme)
        {
            Condition = cond;
            Statements = stmts;
        }

        public ExprNode Condition { get; }
        public List<StmtNode> Statements { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Value.ToString()!;
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

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class ForRangeNode : SyntaxNode
    {
        public ForRangeNode(Lexeme lexeme, ExprNode startValue, ExprNode finalValue) : base(lexeme)
        {
            StartValue = startValue;
            FinalValue = finalValue;
        }

        public ExprNode StartValue { get; }
        public ExprNode FinalValue { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Value.ToString()!;
    }
}