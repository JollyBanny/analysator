using System.Collections;
using PascalCompiler.Enums;
using PascalCompiler.Exceptions;
using PascalCompiler.Semantics;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.Visitor
{
    public class SymVisitor : IVisitor<bool>
    {
        private static readonly List<SymType> ReadWriteTypes = new List<SymType>
        {
            SymStack.SymBoolean, SymStack.SymChar, SymStack.SymString,
            SymStack.SymInt, SymStack.SymDouble,
        };

        private SymStack _symStack;

        public SymVisitor(SymStack symStack)
        {
            _symStack = symStack;
        }

        private bool IsOverloaded(SymType left, SymType right, params SymType[] types)
        {
            return types.Contains(left) && types.Contains(right);
        }

        public bool Visit(FullProgramNode node)
        {
            return true;
        }

        public bool Visit(ProgramHeaderNode node)
        {
            return true;
        }

        public bool Visit(ProgramBlockNode node)
        {
            return true;
        }

        public bool Visit(CastNode node)
        {
            return true;
        }

        public bool Visit(BinOperNode node)
        {

            var left = node.Left.SymType!;
            var right = node.Right.SymType!;

            switch (node.Lexeme.Value)
            {
                case Token.ADD:
                    {
                        if (!IsOverloaded(left, right, SymStack.SymInt, SymStack.SymDouble) &&
                            !IsOverloaded(left, right, SymStack.SymChar, SymStack.SymString))
                            throw new SemanticException($"operator is not overloaded '{left}' {node} '{right}'");

                        if (left == SymStack.SymDouble || right == SymStack.SymDouble)
                            node.SymType = SymStack.SymDouble;
                        else if (left == SymStack.SymInt && right == SymStack.SymInt)
                            node.SymType = SymStack.SymInt;
                        else
                            node.SymType = SymStack.SymString;

                        break;
                    }
                case Token.SUB:
                case Token.MUL:
                    {
                        if (!IsOverloaded(left, right, SymStack.SymInt, SymStack.SymDouble))
                            throw new SemanticException($"operator is not overloaded '{left}' {node} '{right}'");

                        if (left == SymStack.SymDouble || right == SymStack.SymDouble)
                            node.SymType = SymStack.SymDouble;
                        else
                            node.SymType = SymStack.SymInt;

                        break;
                    }
                case Token.O_DIV:
                    {
                        if (!IsOverloaded(left, right, SymStack.SymInt, SymStack.SymDouble))
                            throw new SemanticException($"operator is not overloaded '{left}' {node} '{right}'");

                        node.SymType = SymStack.SymDouble;
                        break;
                    }
                case Token.MOD:
                case Token.DIV:
                case Token.SHL:
                case Token.SHR:
                case Token.O_SHL:
                case Token.O_SHR:
                    {
                        if (!IsOverloaded(left, right, SymStack.SymInt))
                            throw new SemanticException($"operator is not overloaded '{left}' {node} '{right}'");

                        node.SymType = SymStack.SymInt;
                        break;
                    }
                case Token.AND:
                case Token.OR:
                case Token.XOR:
                    {
                        if (!IsOverloaded(left, right, SymStack.SymBoolean))
                            throw new SemanticException($"operator is not overloaded '{left}' {node} '{right}'");

                        node.SymType = SymStack.SymBoolean;
                        break;
                    }
                case Token.EQUAL:
                case Token.NOT_EQUAL:
                case Token.MORE:
                case Token.LESS:
                case Token.MORE_EQUAL:
                case Token.LESS_EQUAL:
                    {
                        if (!IsOverloaded(left, right, SymStack.SymInt, SymStack.SymDouble) &&
                            !IsOverloaded(left, right, SymStack.SymChar, SymStack.SymString))
                            throw new SemanticException($"operator is not overloaded '{left}' {node} '{right}'");

                        node.SymType = SymStack.SymBoolean;
                        break;
                    }
                default:
                    break;
            }

            if (left == SymStack.SymInt && right == SymStack.SymDouble)
                node.Left = new CastNode(node.Left) { SymType = SymStack.SymDouble };

            if (left == SymStack.SymDouble && right == SymStack.SymInt)
                node.Right = new CastNode(node.Right) { SymType = SymStack.SymDouble };

            return true;
        }

        public bool Visit(UnaryOperNode node)
        {
            if (node.Expr.SymType != SymStack.SymInt && node.Expr.SymType != SymStack.SymDouble)
                throw new SemanticException($"{SymStack.SymInt} expexted but {node.Expr.SymType} found");

            node.SymType = node.Expr.SymType;
            return true;
        }

        public bool Visit(RecordAccessNode node)
        {
            node.Record.Accept(this);

            if (node.Record.SymType is not SymRecordType)
                throw new SemanticException("Illegal qualifier");

            var table = (node.Record.SymType as SymRecordType)!.Table;
            var member = table.Find(node.Field.ToString()) as SymVar;

            if (member is null)
                throw new SemanticException($"identifier idents no member '{node.Field}'");

            node.SymType = member.Type;
            return true;
        }

        public bool Visit(ArrayAccessNode node)
        {
            node.ArrayIdent.Accept(this);
            var symType = node.ArrayIdent.SymType;

            if (symType is not SymArrayType && symType is not SymStringType)
                throw new SemanticException("Illegal qualifier");

            foreach (var expr in node.AccessExprs)
                if (expr.SymType != SymStack.SymInt)
                    throw new SemanticException("index is not integer");

            SymType type;

            if (symType is SymArrayType)
                type = (symType as SymArrayType)!.ElemType;
            else
                type = SymStack.SymChar;

            for (int i = 1; i < node.AccessExprs.Count; i++)
                if (type is SymArrayType)
                    type = (type as SymArrayType)!.ElemType;
                else
                    throw new SemanticException("Illegal qualifier");

            node.SymType = type;
            return true;
        }

        public bool Visit(UserCallNode node)
        {
            var symProc = _symStack.FindProc(node.ToString()!);

            if (symProc is null)
                throw new SemanticException($"procedure {node} is not declared");
            else
                node.SymProc = symProc;

            if (symProc is SymFunc)
                node.SymType = (symProc as SymFunc)!.ReturnType;

            return true;
        }

        public bool Visit(WriteCallNode node)
        {
            foreach (var arg in node.Args)
                if (!ReadWriteTypes.Contains(arg.SymType!))
                    throw new SemanticException($"{arg.SymType} is not writable type");

            return true;
        }

        public bool Visit(ReadCallNode node)
        {
            foreach (var arg in node.Args)
                if (!ReadWriteTypes.Contains(arg.SymType!))
                    throw new SemanticException($"{arg.SymType} is not readable type");

            return true;
        }

        public bool Visit(IdentNode node)
        {
            var symVar = _symStack.FindIdent(node.ToString()!);

            if (symVar is not null)
            {
                node.SymVar = symVar;
                node.SymType = symVar.Type;
            }
            else
                throw new SemanticException($"variable {node} is not declared");

            return true;
        }

        public bool Visit(ConstIntegerLiteral node)
        {
            node.SymType = SymStack.SymInt;
            return true;
        }

        public bool Visit(ConstDoubleLiteral node)
        {
            node.SymType = SymStack.SymDouble;
            return true;
        }

        public bool Visit(ConstCharLiteral node)
        {
            node.SymType = SymStack.SymChar;
            return true;
        }

        public bool Visit(ConstStringLiteral node)
        {
            node.SymType = SymStack.SymString;
            return true;
        }

        public bool Visit(ConstBooleanLiteral node)
        {
            node.SymType = SymStack.SymBoolean;
            return true;
        }

        public bool Visit(DeclsPartNode node)
        {
            return true;
        }

        public bool Visit(ConstDeclsPartNode node)
        {
            return true;
        }

        public bool Visit(VarDeclsPartNode node)
        {
            return true;
        }

        public bool Visit(TypeDeclsPartNode node)
        {
            return true;
        }

        public bool Visit(ConstDeclNode node)
        {
            return true;
        }

        public bool Visit(VarDeclNode node)
        {
            return true;
        }

        public bool Visit(TypeDeclNode node)
        {
            return true;
        }

        public bool Visit(CallDeclNode node)
        {
            return true;
        }

        public bool Visit(CallHeaderNode node)
        {
            return true;
        }

        public bool Visit(FormalParamNode node)
        {
            return true;
        }

        public bool Visit(SubroutineBlockNode node)
        {
            return true;
        }

        public bool Visit(KeywordNode node)
        {
            return true;
        }

        public bool Visit(CompoundStmtNode node)
        {
            return true;
        }

        public bool Visit(EmptyStmtNode node)
        {
            return true;
        }

        public bool Visit(CallStmtNode node)
        {
            return true;
        }

        public bool Visit(AssignStmtNode node)
        {
            return true;
        }

        public bool Visit(IfStmtNode node)
        {
            return true;
        }

        public bool Visit(WhileStmtNode node)
        {
            return true;
        }

        public bool Visit(RepeatStmtNode node)
        {
            return true;
        }

        public bool Visit(ForStmtNode node)
        {
            return true;
        }

        public bool Visit(ForRangeNode node)
        {
            return true;
        }

        public bool Visit(SimpleTypeNode node)
        {
            return true;
        }

        public bool Visit(ArrayTypeNode node)
        {
            return true;
        }

        public bool Visit(SubrangeTypeNode node)
        {
            return true;
        }

        public bool Visit(RecordTypeNode node)
        {
            return true;
        }

        public bool Visit(RecordFieldNode node)
        {
            return true;
        }

        public bool Visit(ConformatArrayTypeNode node)
        {
            return true;
        }
    }
}