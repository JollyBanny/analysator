using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public class DeclsPartNode : SyntaxNode
    {
        protected DeclsPartNode(List<SyntaxNode> decls, Lexeme lexeme) : base(lexeme)
        {
            Decls = decls;
        }

        public List<SyntaxNode> Decls { get; }

        public override void PrintTree(int depth, string indent)
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
        public ExprNode Expr { get; }

        public override void PrintTree(int depth, string indent)
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
        public ExprNode? Expr { get; }

        public override void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            for (int i = 0; i < IdentsList.Count; ++i)
            {
                Console.Write(IdentsList[i] + (i == IdentsList.Count - 1 ? "\n" : ", "));
            }

            if (Expr is not null)
            {
                Console.Write(indent + "├──── ");
                Type.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
                Console.Write(indent + "└──── ");
            }
            else
            {
                Console.Write(indent + "└──── ");
                Type.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
            }

            Expr?.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

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

        public override void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Ident.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            Type.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

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

        public override void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Header.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            Console.Write(indent + "└──── ");
            if (IsForward)
                Console.WriteLine("forward");
            Block?.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
        }

        public override string ToString() => Lexeme.Value.ToString()!;
    }

    public class CallHeaderNode : SyntaxNode
    {
        public CallHeaderNode(IdentNode name, List<FormalParamNode>? paramsList,
            TypeNode? type = null) : base()
        {
            Name = name;
            ParamsList = paramsList;
            Type = type;
        }

        public IdentNode Name { get; }
        public List<FormalParamNode>? ParamsList { get; }
        public TypeNode? Type { get; }


        public override void PrintTree(int depth, string indent)
        {
            Console.WriteLine(this);

            Console.Write(indent + "├──── ");
            Name.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));

            if (ParamsList is not null)
                if (Type is not null)
                    foreach (var param in ParamsList)
                    {
                        Console.Write(indent + "├──── ");
                        param.PrintTree(depth + 1, indent + "│".PadRight(6, ' '));
                    }
                else
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

            if (Type is not null)
            {
                Console.Write(indent + "└──── ");
                Type?.PrintTree(depth + 1, indent + "".PadRight(6, ' '));
            }
        }

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

        public override void PrintTree(int depth, string indent)
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

        public override void PrintTree(int depth, string indent)
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

        public override string ToString() => "block";
    }

    public class KeywordNode : SyntaxNode
    {
        public KeywordNode(Lexeme lexeme) : base(lexeme)
        { }

        public override void PrintTree(int depth, string indent) =>
            Console.WriteLine(this);

        public override string ToString() => Lexeme.Value.ToString()!;
    }
}