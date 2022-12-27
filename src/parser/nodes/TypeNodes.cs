using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.Semantics;
using PascalCompiler.Visitor;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public abstract class TypeNode : SyntaxNode
    {
        protected TypeNode(Lexeme? lexeme = null) : base(lexeme)
        {
            SymType = null!;
        }

        public SymType SymType { get; set; }
    }

    public class SimpleTypeNode : TypeNode
    {
        public SimpleTypeNode(SyntaxNode typeIdent) : base(typeIdent.Lexeme)
        {
            TypeIdent = typeIdent;
        }

        public SyntaxNode TypeIdent { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => TypeIdent is IdentNode ?
            Lexeme.Value.ToString()!.ToLower() : Lexeme.Value.ToString()!;
    }

    public class ArrayTypeNode : TypeNode
    {
        public ArrayTypeNode(Lexeme lexeme, SubrangeTypeNode range, TypeNode type) : base(lexeme)
        {
            Range = range;
            Type = type;
        }

        public SubrangeTypeNode Range { get; }
        public TypeNode Type { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class SubrangeTypeNode : TypeNode
    {
        public SubrangeTypeNode(Lexeme lexeme, ExprNode left, ExprNode right) : base(lexeme)
        {
            LeftBound = left;
            RightBound = right;
        }

        public ExprNode LeftBound { get; }
        public ExprNode RightBound { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Source;
    }

    public class RecordTypeNode : TypeNode
    {
        public RecordTypeNode(Lexeme lexeme, List<RecordFieldNode> fieldsList) : base(lexeme)
        {
            FieldsList = fieldsList;
        }

        public List<RecordFieldNode> FieldsList { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class RecordFieldNode : TypeNode
    {
        public RecordFieldNode(Lexeme lexeme, List<IdentNode> identsList, TypeNode type) : base(lexeme)
        {
            IdentsList = identsList;
            Type = type;
        }

        public List<IdentNode> IdentsList { get; }
        public TypeNode Type { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Source;
    }

    public class ConformatArrayTypeNode : TypeNode
    {
        public ConformatArrayTypeNode(Lexeme lexeme, TypeNode type) : base(lexeme)
        {
            Type = type;
        }

        public TypeNode Type { get; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString() => Lexeme.Value.ToString()!;
    }
}