using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.Visitor;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public abstract class ProgramNode : SyntaxNode
    {
        protected ProgramNode(Lexeme? lexeme = null) : base(lexeme) { }
    }

    public class FullProgramNode : ProgramNode
    {
        public FullProgramNode(ProgramNode? header, ProgramNode block) : base()
        {
            Header = header;
            Block = block;
        }

        public ProgramNode? Header { get; }
        public ProgramNode Block { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override void Accept(IGenVisitor visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => "program";
    }

    public class ProgramHeaderNode : ProgramNode
    {
        public ProgramHeaderNode(IdentNode programName) : base()
        {
            ProgramName = programName;
        }

        public IdentNode ProgramName { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override void Accept(IGenVisitor visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => "program header";
    }

    public class ProgramBlockNode : ProgramNode
    {
        public ProgramBlockNode(List<SyntaxNode> decls, StmtNode compoundStmt) : base()
        {
            Decls = decls;
            CompoundStmt = compoundStmt;
        }

        public List<SyntaxNode> Decls { get; }
        public StmtNode CompoundStmt { get; }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);

        public override void Accept(IGenVisitor visitor, bool withResult) => visitor.Visit(this, withResult);

        public override string ToString() => "program block";
    }
}