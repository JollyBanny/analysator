using PascalCompiler.Semantics;
using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.Visitor;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public abstract class ExprNode : SyntaxNode
    {
        protected ExprNode(Lexeme? lexeme = null, SymType type = null!) : base(lexeme!)
        {
            SymType = type;
        }

        public SymType SymType { get; }

        public override string ToString() => $"'{Lexeme.Value}'";
    }

    public class CastNode : ExprNode
    {
        public CastNode(ExprNode expr, TypeNode toType) : base()
        {
            Expr = expr;
            ToType = toType;
        }

        public ExprNode Expr { get; }
        public TypeNode ToType { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => "cast";
    }

    public class BinOperNode : ExprNode
    {
        public BinOperNode(Lexeme lexeme, ExprNode left, ExprNode right) : base(lexeme)
        {
            Left = left;
            Right = right;
        }

        public ExprNode Left { get; }
        public ExprNode Right { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
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

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Source;
    }

    public class RecordAccessNode : ExprNode
    {
        public RecordAccessNode(ExprNode record, IdentNode field) : base()
        {
            Record = record;
            Field = field;
        }

        public ExprNode Record { get; }
        public IdentNode Field { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => "record access";
    }

    public class ArrayAccessNode : ExprNode
    {
        public ArrayAccessNode(ExprNode arrayIdent, List<ExprNode> accessExprs) : base()
        {
            ArrayIdent = arrayIdent;
            AccessExprs = accessExprs;
        }

        public ExprNode ArrayIdent { get; }
        public List<ExprNode> AccessExprs { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => "array access";
    }

    public abstract class CallNode : ExprNode
    {
        public CallNode(IdentNode funcIdent, List<ExprNode> args) : base(funcIdent.Lexeme)
        {
            Args = args;
        }

        public List<ExprNode> Args { get; }

        public override string ToString() => Lexeme.Value.ToString()!.ToLower();
    }

    public class UserCallNode : CallNode
    {
        public UserCallNode(IdentNode funcIdent, List<ExprNode> args, SymProc sym = null!) : base(funcIdent, args)
        {
            SymProc = sym;
        }

        public SymProc SymProc { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class WriteCallNode : CallNode
    {
        public WriteCallNode(IdentNode funcIdent, List<ExprNode> args, bool newLine) : base(funcIdent, args)
        {
            NewLine = newLine;
        }

        public bool NewLine { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class IdentNode : ExprNode
    {
        public IdentNode(Lexeme lexeme, SymType type) : base(lexeme, type)
        {
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Value.ToString()!.ToLower();
    }

    public abstract class ConstantNode : ExprNode
    {
        protected ConstantNode(Lexeme lexeme, SymType type) : base(lexeme, type)
        {
        }
    }

    public class ConstIntegerLiteral : ConstantNode
    {
        public ConstIntegerLiteral(Lexeme lexeme, SymType type) : base(lexeme, type)
        {
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class ConstDoubleLiteral : ConstantNode
    {
        public ConstDoubleLiteral(Lexeme lexeme, SymType type) : base(lexeme, type)
        {
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class ConstCharLiteral : ConstantNode
    {
        public ConstCharLiteral(Lexeme lexeme, SymType type) : base(lexeme, type)
        {
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => $"'{Lexeme.Value}'";
    }

    public class ConstStringLiteral : ConstantNode
    {
        public ConstStringLiteral(Lexeme lexeme, SymType type) : base(lexeme, type)
        {
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => $"'{Lexeme.Value}'";
    }

    public class ConstBooleanLiteral : ConstantNode
    {
        public ConstBooleanLiteral(Lexeme lexeme, SymType type) : base(lexeme, type)
        {
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Value.ToString()!;
    }
}