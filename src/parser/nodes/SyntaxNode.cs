using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public abstract class SyntaxNode
    {
        public SyntaxNode(Lexeme lexeme) => Lexeme = lexeme;

        public Lexeme Lexeme { get; }

        abstract public void PrintTree(int depth = 0, string indent = "");
    }

    // class StmtNode : SyntaxNode
    // {
    //     public StmtNode(Lexeme lexeme) : base(lexeme) { }
    //     override public void PrintTree(int depth = 0, string indent = "") { }
    //     override public bool HasChildren() => false;
    // }

    // class DeclareVarsNode : StmtNode
    // {
    //     public DeclareVarsNode(Lexeme lexeme, Lexeme[] vars) : base(lexeme) { }
    // }

    // class WhileStmtNode : StmtNode
    // {
    //     public WhileStmtNode(Lexeme lexeme, ExprNode cond, StmtNode body) : base(lexeme)
    //     {
    //         Condition = cond;
    //         Body = body;
    //     }

    //     public ExprNode Condition { get; }
    //     public StmtNode Body { get; }

    //     override public void PrintTree(int depth, string indent)
    //     {
    //         Console.WriteLine(this);

    //         Console.Write(indent + "├──── ");
    //         Condition.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

    //         Console.Write(indent + "└──── ");
    //         Body.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
    //     }

    //     override public string ToString() => Lexeme.Source;
    // }

    // class IfStmtNode : StmtNode
    // {
    //     public IfStmtNode(Lexeme lexeme, ExprNode cond, StmtNode ifPart, StmtNode elsePart)
    //     : base(lexeme)
    //     {
    //         Condition = cond;
    //         IfPart = ifPart;
    //         ElsePart = elsePart;
    //     }

    //     public ExprNode Condition { get; }
    //     public StmtNode IfPart { get; }
    //     public StmtNode? ElsePart { get; }
    // }

    // class AssignStmt : StmtNode
    // {
    //     public AssignStmt(Lexeme lexeme, ExprNode left, ExprNode right) : base(lexeme)
    //     {
    //         Left = left;
    //         Right = right;
    //     }

    //     public ExprNode Left { get; }
    //     public ExprNode Right { get; }
    // }

    // class BlockStmt : StmtNode
    // {
    //     public BlockStmt(Lexeme lexeme, List<StmtNode> body) : base(lexeme)
    //     {
    //         Body = body;
    //     }

    //     public List<StmtNode> Body { get; }
    // }
}