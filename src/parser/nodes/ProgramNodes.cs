using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public class ProgramNode : SyntaxNode
    {
        protected ProgramNode(Lexeme? lexeme = null) : base(lexeme!) { }
        override public void PrintTree(int depth = 0, string indent = "") { }
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

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);


            if (Header is not null)
                Console.Write(indent + "├──── ");

            Header?.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            Block.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => "program";
    }

    public class ProgramHeaderNode : ProgramNode
    {
        public ProgramHeaderNode(IdentNode programName) : base()
        {
            ProgramName = programName;
        }

        public IdentNode ProgramName { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "└──── ");
            ProgramName.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => "program header";
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

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            foreach (var decl in Decls)
            {
                Console.Write(indent + "├──── ");
                decl.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
            }

            Console.Write(indent + "└──── ");
            CompoundStmt.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => "program block";
    }
}