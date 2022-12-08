using PascalCompiler.Enums;
using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public class DeclsPartNode : SyntaxNode
    {
        protected DeclsPartNode(List<SyntaxNode> decls, Lexeme? lexeme = null)
        : base(lexeme!)
        {
            Decls = decls;
        }

        public List<SyntaxNode> Decls { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            for (int i = 0; i < Decls.Count; ++i)
            {
                if (i == Decls.Count - 1)
                {
                    Console.Write(indent + "└──── ");
                    Decls[i].PrintTree(depth + 1, indent + "".PadRight(6, ' '));
                }
                else
                {
                    Console.Write(indent + "├──── ");
                    Decls[i].PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
                }
            }
        }

        override public string ToString() => Lexeme.Value.ToString()!;
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
        public ExprNode Expr { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);


            Console.Write(indent + "├──── ");
            Ident.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            if (Type is not null)
            {
                Console.Write(indent + "├──── ");
                Type.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
            }

            Console.Write(indent + "└──── ");
            Expr.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => "const declaration";
    }

    public class VarDeclsPartNode : DeclsPartNode
    {
        public VarDeclsPartNode(Lexeme lexeme, List<SyntaxNode> decls) : base(decls, lexeme)
        { }
    }

    public class VarDeclNode : SyntaxNode
    {
        public VarDeclNode(List<IdentNode> identsList, TypeNode type) : base()
        {
            IdentsList = identsList;
            Type = type;
        }

        public List<IdentNode> IdentsList { get; }
        public TypeNode Type { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            for (int i = 0; i < IdentsList.Count; ++i)
            {
                Console.Write(IdentsList[i] + (i == IdentsList.Count - 1 ? "\n" : ", "));
            }

            Console.Write(indent + "└──── ");
            Type.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => "var declaration";
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

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Ident.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            Type.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => "type declaration";
    }

    public class CallDeclNode : SyntaxNode
    {
        public CallDeclNode(Lexeme lexeme, CallHeaderNode header, SyntaxNode block)
        : base(lexeme)
        {
            Header = header;
            Block = block;
        }

        public CallHeaderNode Header { get; }
        public SyntaxNode Block { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Header.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            Block.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => Lexeme.Value.ToString()!;
    }

    public class CallHeaderNode : SyntaxNode
    {
        public CallHeaderNode(IdentNode name, List<FormalParamNode> paramsList,
            TypeNode? type = null) : base()
        {
            Name = name;
            ParamsList = paramsList;
            Type = type;
        }

        public IdentNode Name { get; }
        public List<FormalParamNode> ParamsList { get; }
        public TypeNode? Type { get; }


        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Name.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            if (Type is not null)
            {
                foreach (var param in ParamsList)
                {
                    Console.Write(indent + "├──── ");
                    param.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
                }
                Console.Write(indent + "└──── ");
            }
            else
            {
                for (int i = 0; i < ParamsList.Count; ++i)
                {
                    if (i == ParamsList.Count - 1)
                    {
                        Console.Write(indent + "└──── ");
                        ParamsList[i].PrintTree(depth + 1, indent + "".PadRight(6, ' '));
                    }
                    else
                    {
                        Console.Write(indent + "├──── ");
                        ParamsList[i].PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
                    }
                }
            }

            Type?.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => "header";
    }

    public class FormalParamNode : SyntaxNode
    {
        public FormalParamNode(List<IdentNode> identsList, TypeNode type,
            KeywordNode? modifier) : base()
        {
            IdentsList = identsList;
            Type = type;
            Modifier = modifier;
        }

        public List<IdentNode> IdentsList { get; }
        public TypeNode Type { get; }
        public KeywordNode? Modifier { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            if (Modifier is not null)
            {
                Console.Write(indent + "├──── ");
                Modifier.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
            }

            Console.Write(indent + "├──── ");
            for (int i = 0; i < IdentsList.Count; ++i)
            {
                Console.Write(IdentsList[i] + (i == IdentsList.Count - 1 ? "\n" : ", "));
            }

            Console.Write(indent + "└──── ");
            Type.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => "parameter";
    }

    public class SubroutineBlockNode : SyntaxNode
    {
        public SubroutineBlockNode(List<DeclsPartNode> decls, StmtNode compoundStmt)
        : base()
        {
            Decls = decls;
            CompoundStmt = compoundStmt;
        }

        public List<DeclsPartNode> Decls { get; }
        public StmtNode CompoundStmt { get; }

        override public void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            foreach (var decl in Decls)
            {
                Console.Write(indent + "├──── ");
                decl.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
            }

            Console.Write(indent + "└──── ");
            CompoundStmt.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        override public string ToString() => "block";
    }

    public class KeywordNode : SyntaxNode
    {
        public KeywordNode(Lexeme lexeme) : base(lexeme)
        { }

        override public void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        override public string ToString() => Lexeme.Value.ToString()!;
    }
}