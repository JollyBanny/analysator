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
        private bool _inScope;

        public SymVisitor(SymStack symStack)
        {
            _symStack = symStack;
        }

        private bool IsOverloaded(SymType left, SymType right, params SymType[] types)
        {
            return types.Contains(left) && types.Contains(right);
        }

        private bool IsOverloaded(SymType symType, params SymType[] types)
        {
            return types.Contains(symType);
        }

        public bool Visit(FullProgramNode node)
        {
            node.Header?.Accept(this);
            node.Block.Accept(this);
            return true;
        }

        public bool Visit(ProgramHeaderNode node)
        {
            _symStack.AddVar(node.ProgramName.ToString(), null!);
            return true;
        }

        public bool Visit(ProgramBlockNode node)
        {
            foreach (var decl in node.Decls)
                decl.Accept(this);

            foreach (var decl in node.Decls)
                if (decl is CallDeclNode callNode && callNode?.IsForward is true)
                {
                    var callName = callNode.Header.Name;
                    var symCall = _symStack.FindCall(callName.ToString());

                    if (symCall?.IsForward is true)
                        throw new SemanticException(callName.Lexeme.Pos, "forward declaration not solved");
                }

            node.CompoundStmt.Accept(this);
            return true;
        }

        public bool Visit(CastNode node)
        {
            node.Expr.Accept(this);
            return true;
        }

        public bool Visit(BinOperNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);

            var left = node.Left.SymType;
            var right = node.Right.SymType;

            switch (node.Lexeme.Value)
            {
                case Token.ADD:
                    {
                        if (!IsOverloaded(left, right, SymStack.SymInt, SymStack.SymDouble) &&
                            !IsOverloaded(left, right, SymStack.SymChar, SymStack.SymString))
                            throw new SemanticException(node.Lexeme.Pos,
                                $"operator is not overloaded '{left}' {node} '{right}'");

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
                            throw new SemanticException(node.Lexeme.Pos,
                                $"operator is not overloaded '{left}' {node} '{right}'");

                        if (left == SymStack.SymDouble || right == SymStack.SymDouble)
                            node.SymType = SymStack.SymDouble;
                        else
                            node.SymType = SymStack.SymInt;

                        break;
                    }
                case Token.O_DIV:
                    {
                        if (!IsOverloaded(left, right, SymStack.SymInt, SymStack.SymDouble))
                            throw new SemanticException(node.Lexeme.Pos,
                                $"operator is not overloaded '{left}' {node} '{right}'");

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
                            throw new SemanticException(node.Lexeme.Pos,
                                $"operator is not overloaded '{left}' {node} '{right}'");

                        node.SymType = SymStack.SymInt;
                        break;
                    }
                case Token.AND:
                case Token.OR:
                case Token.XOR:
                    {
                        if (!IsOverloaded(left, right, SymStack.SymBoolean))
                            throw new SemanticException(node.Lexeme.Pos,
                                $"operator is not overloaded '{left}' {node} '{right}'");

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
                            throw new SemanticException(node.Lexeme.Pos,
                                $"operator is not overloaded '{left}' {node} '{right}'");

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
            node.Expr.Accept(this);

            switch (node.Lexeme.Value)
            {
                case Token.ADD:
                case Token.SUB:
                    {
                        if (!IsOverloaded(symType: node.Expr.SymType, SymStack.SymInt, SymStack.SymDouble))
                            throw new SemanticException(node.Lexeme.Pos,
                                $"integer expexted but {node.Expr.SymType} found");
                        else
                            break;
                    }
                case Token.NOT:
                    {
                        if (!IsOverloaded(symType: node.Expr.SymType, SymStack.SymBoolean))
                            throw new SemanticException(node.Lexeme.Pos,
                                $"boolean expexted but {node.Expr.SymType} found");
                        else
                            break;
                    }
            }

            node.SymType = node.Expr.SymType;
            return true;
        }

        public bool Visit(RecordAccessNode node)
        {
            node.Record.Accept(this);
            _symStack.Push((node.Record.SymType as SymRecordType)!.Table);
            _inScope = true;

            node.Field.Accept(this);

            _inScope = false;
            _symStack.Pop();

            node.SymType = node.Field.SymType;
            node.IsLValue = true;
            return true;
        }

        public bool Visit(ArrayAccessNode node)
        {
            node.ArrayIdent.Accept(this);

            var symType = node.ArrayIdent.SymType;

            if (symType is not SymArrayType && symType is not SymStringType)
                throw new SemanticException(node.Lexeme.Pos, "Illegal qualifier");

            foreach (var expr in node.AccessExprs)
            {
                expr.Accept(this);
                if (expr.SymType != SymStack.SymInt)
                    throw new SemanticException(node.Lexeme.Pos, "index is not integer");
            }

            SymType type;

            if (symType is SymArrayType)
                type = (symType as SymArrayType)!.ElemType;
            else
                type = SymStack.SymChar;

            for (int i = 1; i < node.AccessExprs.Count; i++)
                if (type is SymArrayType)
                    type = (type as SymArrayType)!.ElemType;
                else
                    throw new SemanticException(node.Lexeme.Pos, "Illegal qualifier");

            node.SymType = type;
            node.IsLValue = true;
            return true;
        }

        public bool Visit(IdentNode node)
        {
            var symVar = _inScope ? _symStack.FindIdent(node.ToString()!, true) :
                                    _symStack.FindIdent(node.ToString()!);

            var symReturn = _symStack.FindCall(node.ToString(), true);

            if (symVar is null)
            {
                if (_inScope && symReturn is null)
                    throw new SemanticException(node.Lexeme.Pos, $"procedure {node} no return type");
                else if (_inScope)
                    throw new SemanticException(node.Lexeme.Pos, $"identifier idents no member '{node}'");
                else if (symReturn is SymFunc symFunc)
                {
                    node.SymVar = new SymVar(node.ToString(), symFunc.ReturnType);
                    node.SymType = node.SymVar.Type;
                    node.IsLValue = true;
                    return true;
                }
                else
                    throw new SemanticException(node.Lexeme.Pos, $"variable {node} is not declared");
            }

            SymType type = symVar.Type;
            while (type is SymAliasType aliasType)
                type = aliasType.Origin;

            node.SymVar = symVar;
            node.SymType = type;
            node.IsLValue = symVar is not SymConstant;
            return true;
        }

        public bool Visit(UserCallNode node)
        {
            var symProc = _symStack.FindCall(node.ToString()!);

            if (symProc is null)
                throw new SemanticException(node.Lexeme.Pos, $"procedure {node} is not declared");
            else
                node.SymProc = symProc;

            if (symProc is SymFunc)
                node.SymType = (symProc as SymFunc)!.ReturnType;

            return true;
        }

        public bool Visit(WriteCallNode node)
        {
            foreach (var arg in node.Args)
            {
                arg.Accept(this);

                if (!ReadWriteTypes.Contains(arg.SymType))
                    throw new SemanticException(node.Lexeme.Pos, $"{arg.SymType} is not writable type");
            }

            return true;
        }

        public bool Visit(ReadCallNode node)
        {
            foreach (var arg in node.Args)
            {
                arg.Accept(this);

                if (!ReadWriteTypes.Contains(arg.SymType))
                    throw new SemanticException(node.Lexeme.Pos, $"{arg.SymType} is not readable type");
            }

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
            foreach (var decl in node.Decls)
                decl.Accept(this);

            return true;
        }

        public bool Visit(ConstDeclNode node)
        {
            _symStack.CheckDuplicate(node.Ident);
            node.Expr.Accept(this);
            node.Type?.Accept(this);

            if (node.Type is not null && !node.Type.SymType.IsEquivalent(node.Expr.SymType))
                throw new SemanticException(node.Type.Lexeme.Pos,
                    $"incompatible types: got '{node.Type.SymType}' expected '{node.Expr.SymType}'");

            _symStack.AddConst(node.Ident.ToString(), node.Expr.SymType);
            return true;
        }

        public bool Visit(VarDeclNode node)
        {
            node.Type.Accept(this);

            foreach (var ident in node.IdentsList)
            {
                _symStack.CheckDuplicate(ident);
                _symStack.AddVar(ident.ToString(), node.Type.SymType);

                ident.SymVar = _symStack.FindIdent(ident.ToString(), true);
                ident.SymType = ident.SymVar!.Type;
            }

            return true;
        }

        public bool Visit(TypeDeclNode node)
        {
            _symStack.CheckDuplicate(node.Ident);

            node.Type.Accept(this);

            _symStack.AddAliasType(node.Ident.ToString(), node.Type.SymType);

            return true;
        }

        public bool Visit(CallDeclNode node)
        {
            node.Header.Accept(this);

            var forwardedCall = _symStack.FindCall(node.Header.Name.ToString(), true);

            if (forwardedCall is not null)
            {
                if (forwardedCall.Params.Count != node.Header.symCall?.Params.Count)
                    throw new SemanticException(node.Header.Name.Lexeme.Pos,
                        $"function header {node.Header.Name} doesn't match forward");

                var forwardedCallType = forwardedCall is SymFunc oldSymFunc ? oldSymFunc.ReturnType : null;
                var newCallType = node.Header.symCall is SymFunc newSymFunc ? newSymFunc.ReturnType : null;

                if (forwardedCallType?.IsEquivalent(newCallType!) == false)
                    throw new SemanticException(node.Header.Name.Lexeme.Pos,
                         $"function header {node.Header.Name} doesn't match forward");

                for (int i = 0; i < forwardedCall.Params.Count; i++)
                {
                    var oldParam = forwardedCall.Params[i] as SymParameter;
                    var newParam = node.Header.symCall.Params[i] as SymParameter;

                    if (oldParam!.Name != newParam!.Name ||
                        oldParam.Modifier != newParam.Modifier ||
                        !oldParam.Type.IsEquivalent(newParam.Type))
                        throw new SemanticException(node.Header.Name.Lexeme.Pos,
                         $"function header {node.Header.Name} doesn't match forward");
                }
            }

            if (node.Block is not null)
            {
                node.Header.symCall!.IsForward = false;
                _symStack.Push(node.Header.symCall!.Locals);
                _inScope = true;

                node.Block.Accept(this);

                _inScope = false;
                node.Header.symCall.Locals = _symStack.Pop();
                node.Header.symCall.Block = node.Block as StmtNode;

                if (forwardedCall is not null)
                    _symStack.Remove(forwardedCall.Name);
            }
            else
                _symStack.CheckDuplicate(node.Header.Name);

            _symStack.Add(node.Header.symCall!);

            return true;
        }

        public bool Visit(CallHeaderNode node)
        {
            _symStack.Push();

            var symCall = node.Type is null ?
                new SymProc(node.Name.ToString(), new SymTable(), new SymTable()) :
                new SymFunc(node.Name.ToString(), new SymTable(), new SymTable(), null!);

            if (node.Type is not null)
            {
                node.Type.Accept(this);
                (symCall as SymFunc)!.ReturnType = node.Type.SymType;
                _symStack.Add(symCall);
            }

            foreach (var param in node.ParamsList)
            {
                var modifier = param.Modifier is not null ? param.Modifier.ToString() : "";
                param.Type.Accept(this);

                foreach (var ident in param.IdentsList)
                {
                    _symStack.CheckDuplicate(ident);
                    _symStack.AddParameter(ident.ToString(), param.Type.SymType, modifier!);
                }
            }

            symCall.Locals = _symStack.Pop();

            for (int i = 0; i < symCall.Locals.Count; i++)
                if (symCall.Locals[i] is SymParameter symParam)
                    symCall.Params.Add(symParam);

            node.symCall = symCall;
            return true;
        }

        public bool Visit(FormalParamNode node)
        {
            return true;
        }

        public bool Visit(SubroutineBlockNode node)
        {
            foreach (var decl in node.Decls)
                decl.Accept(this);

            foreach (var decl in node.Decls)
                if (decl is CallDeclNode callNode && callNode?.IsForward is true)
                {
                    var callName = callNode.Header.Name;
                    var symCall = _symStack.FindCall(callName.ToString());

                    if (symCall?.IsForward is true)
                        throw new SemanticException(callName.Lexeme.Pos, "forward declaration not solved");
                }

            _inScope = false;
            node.CompoundStmt.Accept(this);
            return true;
        }

        public bool Visit(KeywordNode node)
        {
            return true;
        }

        public bool Visit(CompoundStmtNode node)
        {
            foreach (var stmt in node.Statements)
                stmt.Accept(this);

            return true;
        }

        public bool Visit(EmptyStmtNode node)
        {
            return true;
        }

        public bool Visit(CallStmtNode node)
        {
            node.Expression.Accept(this);
            return true;
        }

        public bool Visit(AssignStmtNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);

            if (node.Left.IsLValue is false)
                throw new SemanticException(node.Left.Lexeme.Pos, "variable identifier expected");

            if (node.Right is CallNode callNode && callNode.SymType is null)
                throw new SemanticException(node.Right.Lexeme.Pos, $"procedure {node.Right} no return type");

            if (node.Left.SymType == SymStack.SymDouble && node.Right.SymType == SymStack.SymInt)
            {
                node.Right = new CastNode(node.Right) { SymType = SymStack.SymDouble };
                return true;
            }

            var left = node.Left.SymType;
            var right = node.Right.SymType;

            switch (node.Lexeme.Value)
            {
                case Token.ASSIGN:
                    if (left.IsEquivalent(right))
                        return true;

                    throw new SemanticException(node.Right.Lexeme.Pos,
                        $"incompatible types: got '{right}' expected '{left}'");
                case Token.ADD_ASSIGN:
                case Token.SUB_ASSIGN:
                case Token.MUL_ASSIGN:
                case Token.DIV_ASSIGN:
                    if ((left == SymStack.SymInt || left == SymStack.SymDouble) && left.IsEquivalent(right))
                        return true;

                    throw new SemanticException(node.Right.Lexeme.Pos,
                      $"incompatible types: got '{right}' expected '{left}'");
            }

            return true;
        }

        public bool Visit(IfStmtNode node)
        {
            node.Condition.Accept(this);

            if (node.Condition.SymType != SymStack.SymBoolean)
                throw new SemanticException(node.Condition.Lexeme.Pos,
                    $"boolean expected but {node.Condition.SymType} found");

            node.IfPart.Accept(this);
            node.ElsePart?.Accept(this);
            return true;
        }

        public bool Visit(WhileStmtNode node)
        {
            node.Condition.Accept(this);

            if (node.Condition.SymType != SymStack.SymBoolean)
                throw new SemanticException(node.Condition.Lexeme.Pos,
                    $"boolean expected but {node.Condition.SymType} found");

            node.Statement.Accept(this);
            return true;
        }

        public bool Visit(RepeatStmtNode node)
        {
            node.Condition.Accept(this);

            if (node.Condition.SymType != SymStack.SymBoolean)
                throw new SemanticException(node.Condition.Lexeme.Pos,
                    $"boolean expected but {node.Condition.SymType} found");

            foreach (var stmt in node.Statements)
                stmt.Accept(this);

            return true;
        }

        public bool Visit(ForStmtNode node)
        {
            node.CtrlIdent.Accept(this);

            if (node.CtrlIdent.SymType != SymStack.SymInt)
                throw new SemanticException(node.CtrlIdent.Lexeme.Pos,
                    $"integer expected but {node.CtrlIdent.SymType} found");

            node.ForRange.Accept(this);
            node.Statement.Accept(this);
            return true;
        }

        public bool Visit(ForRangeNode node)
        {
            node.StartValue.Accept(this);

            if (node.StartValue.SymType != SymStack.SymInt)
                throw new SemanticException(node.StartValue.Lexeme.Pos,
                    $"integer expected but {node.StartValue.SymType} found");

            node.FinalValue.Accept(this);

            if (node.FinalValue.SymType != SymStack.SymInt)
                throw new SemanticException(node.FinalValue.Lexeme.Pos,
                    $"integer expected but {node.FinalValue.SymType} found");

            return true;
        }

        public bool Visit(SimpleTypeNode node)
        {
            var type = _symStack.FindType(node.ToString());

            if (type is null)
                throw new SemanticException(node.Lexeme.Pos, $"type '{node}' is not declared");

            while (type is SymAliasType aliasType)
                type = aliasType.Origin;

            node.SymType = type;
            return true;
        }

        public bool Visit(ArrayTypeNode node)
        {
            node.Range.Accept(this);
            node.Type.Accept(this);

            var range = new Pair<ExprNode>(node.Range.LeftBound, node.Range.RightBound);
            node.SymType = new SymArrayType(range, node.Type.SymType);
            return true;
        }

        public bool Visit(SubrangeTypeNode node)
        {
            node.LeftBound.Accept(this);

            if (node.LeftBound.SymType != SymStack.SymInt)
                throw new SemanticException(node.Lexeme.Pos, $"index '{node}' is not integer");

            node.RightBound.Accept(this);

            if (node.RightBound.SymType != SymStack.SymInt)
                throw new SemanticException(node.Lexeme.Pos, $"index '{node}' is not integer");

            return true;
        }

        public bool Visit(RecordTypeNode node)
        {
            _symStack.Push();

            foreach (var field in node.FieldsList)
            {
                field.Accept(this);
            }

            node.SymType = new SymRecordType(_symStack.Pop());
            return true;
        }

        public bool Visit(RecordFieldNode node)
        {
            node.Type.Accept(this);

            foreach (var ident in node.IdentsList)
            {
                _symStack.CheckDuplicate(ident);
                _symStack.AddVar(ident.ToString(), node.Type.SymType);
            }

            return true;
        }

        public bool Visit(ConformatArrayTypeNode node)
        {
            node.Type.Accept(this);
            node.SymType = new SymConformatArrayType(node.Type.SymType);
            return true;
        }
    }
}