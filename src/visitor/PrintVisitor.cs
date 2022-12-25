using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.Visitor
{
    public class PrintVisitor : IVisitor<PrintVisitorNode>
    {
        public PrintVisitorNode Visit(FullProgramNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            if (node.Header is not null)
                printNode.AddChild(node.Header.Accept(this));

            printNode.AddChild(node.Block.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(ProgramHeaderNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.ProgramName.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(ProgramBlockNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());

            foreach (var decl in node.Decls)
                printNode.AddChild(decl.Accept(this));

            printNode.AddChild(node.CompoundStmt.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(CastNode node)
        {
            var printNode = new PrintVisitorNode($"{node.ToString()} to {node.SymType}");
            printNode.AddChild(node.Expr.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(BinOperNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Left.Accept(this));
            printNode.AddChild(node.Right.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(UnaryOperNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Expr.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(RecordAccessNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Record.Accept(this));
            printNode.AddChild(node.Field.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(ArrayAccessNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.ArrayIdent.Accept(this));

            foreach (var expr in node.AccessExprs)
                printNode.AddChild(expr.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(UserCallNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());

            foreach (var arg in node.Args)
                printNode.AddChild(arg.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(WriteCallNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());

            foreach (var arg in node.Args)
                printNode.AddChild(arg.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(IdentNode node)
        {
            return new PrintVisitorNode(node.ToString());
        }

        public PrintVisitorNode Visit(ConstIntegerLiteral node)
        {
            return new PrintVisitorNode(node.ToString());
        }

        public PrintVisitorNode Visit(ConstDoubleLiteral node)
        {
            return new PrintVisitorNode(node.ToString());
        }

        public PrintVisitorNode Visit(ConstCharLiteral node)
        {
            return new PrintVisitorNode(node.ToString());
        }

        public PrintVisitorNode Visit(ConstStringLiteral node)
        {
            return new PrintVisitorNode(node.ToString());
        }

        public PrintVisitorNode Visit(ConstBooleanLiteral node)
        {
            return new PrintVisitorNode(node.ToString());
        }

        public PrintVisitorNode Visit(DeclsPartNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            foreach (var decl in node.Decls)
                printNode.AddChild(decl.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(ConstDeclNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Ident.Accept(this));

            if (node.Type is not null)
                printNode.AddChild(node.Type.Accept(this));

            printNode.AddChild(node.Expr.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(VarDeclNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            var identsListString = new List<string>();

            foreach (var ident in node.IdentsList)
                identsListString.Add(ident.ToString());

            printNode.AddChild(new PrintVisitorNode(String.Join(", ", identsListString)));
            printNode.AddChild(node.Type.Accept(this));

            if (node.Expr is not null)
                printNode.AddChild(node.Expr.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(TypeDeclNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Ident.Accept(this));
            printNode.AddChild(node.Type.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(CallDeclNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Header.Accept(this));
            printNode.AddChild(node.Block is null ? new PrintVisitorNode("forward") :
                                                    node.Block.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(CallHeaderNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Name.Accept(this));

            foreach (var param in node.ParamsList)
                printNode.AddChild(param.Accept(this));

            if (node.Type is not null)
                printNode.AddChild(node.Type.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(FormalParamNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());

            if (node.Modifier is not null)
                printNode.AddChild(node.Modifier.Accept(this));

            var identsListString = new List<string>();

            foreach (var ident in node.IdentsList)
                identsListString.Add(ident.ToString());

            printNode.AddChild(new PrintVisitorNode(String.Join(", ", identsListString)));
            printNode.AddChild(node.Type.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(SubroutineBlockNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());

            foreach (var decl in node.Decls)
                printNode.AddChild(decl.Accept(this));

            printNode.AddChild(node.CompoundStmt.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(KeywordNode node)
        {
            return new PrintVisitorNode(node.ToString());
        }

        public PrintVisitorNode Visit(CompoundStmtNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());

            foreach (var stmt in node.Statements)
                printNode.AddChild(stmt.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(EmptyStmtNode node)
        {
            return new PrintVisitorNode(node.ToString());
        }

        public PrintVisitorNode Visit(CallStmtNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Expression.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(AssignStmtNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Left.Accept(this));
            printNode.AddChild(node.Right.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(IfStmtNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Condition.Accept(this));
            printNode.AddChild(node.IfPart.Accept(this));

            if (node.ElsePart is not null)
                printNode.AddChild(node.ElsePart.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(WhileStmtNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Condition.Accept(this));
            printNode.AddChild(node.Statement.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(RepeatStmtNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Condition.Accept(this));

            foreach (var stmt in node.Statements)
                printNode.AddChild(stmt.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(ForStmtNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.CtrlIdent.Accept(this));
            printNode.AddChild(node.ForRange.Accept(this));
            printNode.AddChild(node.Statement.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(ForRangeNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.StartValue.Accept(this));
            printNode.AddChild(node.FinalValue.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(SimpleTypeNode node)
        {
            return new PrintVisitorNode(node.ToString());
        }

        public PrintVisitorNode Visit(ArrayTypeNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());

            foreach (var range in node.Ranges)
                printNode.AddChild(range.Accept(this));

            printNode.AddChild(node.Type.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(SubrangeTypeNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.LeftBound.Accept(this));
            printNode.AddChild(node.RightBound.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(RecordTypeNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());

            foreach (var field in node.FieldsList)
                printNode.AddChild(field.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(RecordFieldNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            var identsListString = new List<string>();

            foreach (var ident in node.IdentsList)
                identsListString.Add(ident.ToString());

            printNode.AddChild(new PrintVisitorNode(String.Join(", ", identsListString)));
            printNode.AddChild(node.Type.Accept(this));

            return printNode;
        }

        public PrintVisitorNode Visit(ConformatArrayTypeNode node)
        {
            var printNode = new PrintVisitorNode(node.ToString());
            printNode.AddChild(node.Type.Accept(this));

            return printNode;
        }
    }

    public class PrintVisitorNode
    {
        private string _header;
        private List<PrintVisitorNode> _children;

        public PrintVisitorNode(string header)
        {
            _header = header;
            _children = new List<PrintVisitorNode>();
        }

        public void AddChild(PrintVisitorNode node)
        {
            _children.Add(node);
        }

        public void PrintTree(string indent = "")
        {
            Console.WriteLine(_header);

            for (int i = 0; i < _children.Count; i++)
            {
                if (i < _children.Count - 1)
                {
                    Console.Write(indent + "├──── ");
                    _children[i].PrintTree(indent + "│".PadRight(6, ' '));
                }
                else
                {
                    Console.Write(indent + "└──── ");
                    _children[i].PrintTree(indent + "".PadRight(6, ' '));
                }
            }
        }
    }
}