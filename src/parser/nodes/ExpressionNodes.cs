using PascalCompiler.Semantics;
using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.Visitor;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public abstract class ExprNode : SyntaxNode
    {
        protected ExprNode(Lexeme? lexeme = null) : base(lexeme!)
        {
            SymType = null!;
            IsLValue = false;
            IsConstValue = false;
        }

        public bool IsLValue { get; set; }

        public bool IsConstValue { get; set; }

        public SymType SymType { get; set; }

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class CastNode : ExprNode
    {
        public CastNode(ExprNode expr) : base(expr.Lexeme)
        {
            Expr = expr;
            IsLValue = expr.IsLValue;
            IsConstValue = expr.IsConstValue;
        }

        public ExprNode Expr { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => "cast";
    }

    public class BinOperNode : ExprNode
    {
        public BinOperNode(Lexeme lexeme, ExprNode left, ExprNode right) : base(lexeme)
        {
            Left = left;
            Right = right;
        }

        public ExprNode Left { get; set; }
        public ExprNode Right { get; set; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => Lexeme.Source;
    }

    public class UnaryOperNode : ExprNode
    {
        public UnaryOperNode(Lexeme lexeme, ExprNode expr) : base(lexeme)
        {
            Expr = expr;
        }

        public ExprNode Expr { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => Lexeme.Source;
    }

    public class RecordAccessNode : ExprNode
    {
        public RecordAccessNode(Lexeme lexeme, ExprNode record, IdentNode field) : base(lexeme)
        {
            Record = record;
            Field = field;
        }

        public ExprNode Record { get; }
        public IdentNode Field { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => "record access";
    }

    public class ArrayAccessNode : ExprNode
    {
        public ArrayAccessNode(Lexeme lexeme, ExprNode arrayIdent, List<ExprNode> accessExprs) : base(lexeme)
        {
            ArrayIdent = arrayIdent;
            AccessExprs = accessExprs;
        }

        public ExprNode ArrayIdent { get; }
        public List<ExprNode> AccessExprs { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);

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
        public UserCallNode(IdentNode funcIdent, List<ExprNode> args) : base(funcIdent, args)
        {
        }

        public SymProc? SymProc { get; set; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);
    }

    public class WriteCallNode : CallNode
    {
        public WriteCallNode(IdentNode funcIdent, List<ExprNode> args, bool newLine) : base(funcIdent, args)
        {
            NewLine = newLine;
        }

        public bool NewLine { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);
    }

    public class ReadCallNode : CallNode
    {
        public ReadCallNode(IdentNode funcIdent, List<ExprNode> args, bool newLine) : base(funcIdent, args)
        {
            NewLine = newLine;
        }

        public bool NewLine { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);
    }

    public class IdentNode : ExprNode
    {
        public IdentNode(Lexeme lexeme) : base(lexeme)
        {
        }

        public SymVar? SymVar { get; set; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => Lexeme.Value.ToString()!.ToLower();
    }

    public abstract class ConstantNode : ExprNode
    {
        protected ConstantNode(Lexeme lexeme) : base(lexeme)
        {
            IsConstValue = true;
        }
    }

    public class ConstIntegerLiteral : ConstantNode
    {
        public ConstIntegerLiteral(Lexeme lexeme) : base(lexeme)
        {
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);
    }

    public class ConstDoubleLiteral : ConstantNode
    {
        public ConstDoubleLiteral(Lexeme lexeme) : base(lexeme)
        {
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);
    }

    public class ConstCharLiteral : ConstantNode
    {
        public ConstCharLiteral(Lexeme lexeme) : base(lexeme)
        {
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => $"'{Lexeme.Value}'";
    }

    public class ConstStringLiteral : ConstantNode
    {
        public ConstStringLiteral(Lexeme lexeme) : base(lexeme)
        {
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => $"'{Lexeme.Value}'";
    }

    public class ConstBooleanLiteral : ConstantNode
    {
        public ConstBooleanLiteral(Lexeme lexeme) : base(lexeme)
        {
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override T Accept<T>(IGenVisitor<T> visitor, bool withResult) => visitor.Visit(this, withResult);
    }
}