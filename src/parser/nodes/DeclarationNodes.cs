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

            foreach (var ident in IdentsList)
            {
                Console.Write(indent + "├──── ");
                ident.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
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
        { }

        override public void PrintTree(int depth = 0, string indent = "")
        {
            throw new NotImplementedException();
        }
    }

    public class CallHeaderNode : SyntaxNode
    {
        public CallHeaderNode(IdentNode name, List<SyntaxNode> paramsList,
            TypeNode? type = null) : base()
        {
            Name = name;
            ParamsList = paramsList;
            Type = type;
        }

        public IdentNode Name { get; }
        public List<SyntaxNode> ParamsList { get; }
        public TypeNode? Type { get; }


        override public void PrintTree(int depth = 0, string indent = "")
        {
            throw new NotImplementedException();
        }
    }
}