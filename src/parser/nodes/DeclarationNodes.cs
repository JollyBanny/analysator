using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.Semantics;
using PascalCompiler.Visitor;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public abstract class DeclsPartNode : SyntaxNode
    {
        protected DeclsPartNode(List<SyntaxNode> decls, Lexeme lexeme) : base(lexeme)
        {
            Decls = decls;
        }

        public List<SyntaxNode> Decls { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override void Accept(IGenVisitor visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class ConstDeclsPartNode : DeclsPartNode
    {
        public ConstDeclsPartNode(Lexeme lexeme, List<SyntaxNode> decls) : base(decls, lexeme)
        { }
    }

    public class ConstDeclNode : SyntaxNode
    {
        public ConstDeclNode(IdentNode ident, TypeNode? type, ExprNode expr) : base()
        {
            Ident = ident;
            Type = type;
            Expr = expr;
        }

        public IdentNode Ident { get; }
        public TypeNode? Type { get; }
        public ExprNode Expr { get; set; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override void Accept(IGenVisitor visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => "const declaration";
    }

    public class VarDeclsPartNode : DeclsPartNode
    {
        public VarDeclsPartNode(Lexeme lexeme, List<SyntaxNode> decls) : base(decls, lexeme)
        { }
    }

    public class VarDeclNode : SyntaxNode
    {
        public VarDeclNode(List<IdentNode> identsList, TypeNode type, ExprNode? expr) : base()
        {
            IdentsList = identsList;
            Type = type;
            Expr = expr;
        }

        public List<IdentNode> IdentsList { get; }
        public TypeNode Type { get; }
        public ExprNode? Expr { get; set; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override void Accept(IGenVisitor visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => "var declaration";
    }

    public class TypeDeclsPartNode : DeclsPartNode
    {
        public TypeDeclsPartNode(Lexeme lexeme, List<SyntaxNode> decls) : base(decls, lexeme)
        { }
    }

    public class TypeDeclNode : SyntaxNode
    {
        public TypeDeclNode(IdentNode ident, TypeNode type) : base()
        {
            Ident = ident;
            Type = type;
        }

        public IdentNode Ident { get; }
        public TypeNode Type { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override void Accept(IGenVisitor visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => "type declaration";
    }

    public class CallDeclNode : SyntaxNode
    {
        public CallDeclNode(Lexeme lexeme, CallHeaderNode header, SyntaxNode? block = null) : base(lexeme)
        {
            Header = header;
            Block = block;
            IsForward = block is null;
        }

        public CallHeaderNode Header { get; }
        public SyntaxNode? Block { get; }
        public bool IsForward { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override void Accept(IGenVisitor visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class CallHeaderNode : SyntaxNode
    {
        public CallHeaderNode(IdentNode name, List<FormalParamNode> paramsList, TypeNode? type = null) : base()
        {
            Name = name;
            ParamsList = paramsList;
            Type = type;
        }

        public IdentNode Name { get; }
        public List<FormalParamNode> ParamsList { get; }
        public TypeNode? Type { get; }
        public SymProc? symCallable { get; set; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override void Accept(IGenVisitor visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => "header";
    }

    public class FormalParamNode : SyntaxNode
    {
        public FormalParamNode(List<IdentNode> identsList, TypeNode type,
            SyntaxNode? modifier) : base()
        {
            IdentsList = identsList;
            Type = type;
            Modifier = modifier;
        }

        public List<IdentNode> IdentsList { get; }
        public TypeNode Type { get; }
        public SyntaxNode? Modifier { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override void Accept(IGenVisitor visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => "parameter";
    }

    public class SubroutineBlockNode : SyntaxNode
    {
        public SubroutineBlockNode(List<SyntaxNode> decls, StmtNode compoundStmt) : base()
        {
            Decls = decls;
            CompoundStmt = compoundStmt;
        }

        public List<SyntaxNode> Decls { get; }
        public StmtNode CompoundStmt { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override void Accept(IGenVisitor visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => "block";
    }

    public class KeywordNode : SyntaxNode
    {
        public KeywordNode(Lexeme lexeme) : base(lexeme)
        { }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override void Accept(IGenVisitor visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => Lexeme.Value.ToString()!;
    }
}