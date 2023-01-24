using PascalCompiler.AsmGenerator;
using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.Semantics;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.Visitor
{
    public class AsmVisitor : IGenVisitor
    {
        private Generator _g;

        public AsmVisitor(Generator generator)
        {
            _g = generator;
        }

        public void Visit(FullProgramNode node, bool withResult)
        {
            _g.AddModule(Instruction.GLOBAL, "main");
            _g.AddModule(Instruction.EXTERN, "printf");
            _g.AddModule(Instruction.EXTERN, "scanf");

            node.Block.Accept(this, withResult);

            _g.GenCommand(Instruction.MOV, new(Register.EAX), new(0));
            _g.GenCommand(Instruction.RET);
        }

        public void Visit(ProgramHeaderNode node, bool withResult)
        {
            throw new NotImplementedException();
        }

        public void Visit(ProgramBlockNode node, bool withResult)
        {
            _g.GenLabel("_main");

            foreach (var decl in node.Decls)
                decl.Accept(this, true);

            node.CompoundStmt.Accept(this, true);
        }

        public void Visit(CastNode node, bool withResult)
        {
            node.Expr.Accept(this, true);

            _g.GenCommand(Instruction.POP, new(Register.EAX));
            _g.GenCommand(Instruction.CVTSI2SD, new(Register.XMM0), new(Register.EAX));
            GenerateDoublePush(Register.XMM0);
        }

        public void Visit(BinOperNode node, bool withResult)
        {
            node.Left.Accept(this, false);
            node.Right.Accept(this, false);

            if (node.Left.SymType is SymDoubleType)
            {
                _g.GenCommand(Instruction.MOVSD, new(Register.XMM1),
                    new Operand(Register.ESP, OperandFlag.QWORD, OperandFlag.INDIRECT) + 0);
                _g.GenCommand(Instruction.MOVSD, new(Register.XMM0),
                    new Operand(Register.ESP, OperandFlag.QWORD, OperandFlag.INDIRECT) + 8);
                _g.GenCommand(Instruction.ADD, new(Register.ESP), new(16));

                switch (node.Lexeme.Value)
                {
                    case Token.ADD: _g.GenCommand(Instruction.ADDSD, new(Register.XMM0), new(Register.XMM1)); break;
                    case Token.SUB: _g.GenCommand(Instruction.SUBSD, new(Register.XMM0), new(Register.XMM1)); break;
                    case Token.MUL: _g.GenCommand(Instruction.MULSD, new(Register.XMM0), new(Register.XMM1)); break;
                    case Token.O_DIV: _g.GenCommand(Instruction.DIVSD, new(Register.XMM0), new(Register.XMM1)); break;
                    case Token.EQUAL: GenerateDoubleCmp(Instruction.SETE); break;
                    case Token.NOT_EQUAL: GenerateDoubleCmp(Instruction.SETNE); break;
                    case Token.MORE: GenerateDoubleCmp(Instruction.SETG); break;
                    case Token.LESS: GenerateDoubleCmp(Instruction.SETL); break;
                    case Token.MORE_EQUAL: GenerateDoubleCmp(Instruction.SETGE); break;
                    case Token.LESS_EQUAL: GenerateDoubleCmp(Instruction.SETLE); break;
                }

                switch (node.Lexeme.Value)
                {
                    case Token.EQUAL:
                    case Token.NOT_EQUAL:
                    case Token.MORE:
                    case Token.LESS:
                    case Token.MORE_EQUAL:
                    case Token.LESS_EQUAL:
                        _g.GenCommand(Instruction.PUSH, new(Register.EAX));
                        break;
                    default:
                        _g.GenCommand(Instruction.SUB, new(Register.ESP), new(8));
                        _g.GenCommand(Instruction.MOVSD, new Operand(Register.ESP, OperandFlag.QWORD, OperandFlag.INDIRECT),
                            new(Register.XMM0));
                        break;
                }
            }
            else
            {
                _g.GenCommand(Instruction.POP, new(Register.EBX));
                _g.GenCommand(Instruction.POP, new(Register.EAX));

                switch (node.Lexeme.Value)
                {
                    case Token.ADD: _g.GenCommand(Instruction.ADD, new(Register.EAX), new(Register.EBX)); break;
                    case Token.SUB: _g.GenCommand(Instruction.SUB, new(Register.EAX), new(Register.EBX)); break;
                    case Token.MUL: _g.GenCommand(Instruction.IMUL, new(Register.EAX), new(Register.EBX)); break;
                    case Token.DIV:
                    case Token.MOD:
                        _g.GenCommand(Instruction.CDQ);
                        _g.GenCommand(Instruction.IDIV, new(Register.EBX));

                        if (node.Lexeme == Token.DIV) break;

                        _g.GenCommand(Instruction.MOV, new(Register.EAX), new(Register.EDX));
                        break;
                    case Token.O_SHL:
                    case Token.SHL:
                    case Token.O_SHR:
                    case Token.SHR:
                        var instruction = node.Lexeme == Token.SHL || node.Lexeme == Token.O_SHL ? Instruction.SHL : Instruction.SHR;

                        _g.GenCommand(Instruction.MOV, new(Register.ECX), new(Register.EBX));
                        _g.GenCommand(instruction, new(Register.EAX), new(Register.CL));
                        break;
                    case Token.AND: _g.GenCommand(Instruction.AND, new(Register.EAX), new(Register.EBX)); break;
                    case Token.OR: _g.GenCommand(Instruction.OR, new(Register.EAX), new(Register.EBX)); break;
                    case Token.XOR: _g.GenCommand(Instruction.XOR, new(Register.EAX), new(Register.EBX)); break;
                    case Token.EQUAL: GenerateIntCmp(Instruction.SETE); break;
                    case Token.NOT_EQUAL: GenerateIntCmp(Instruction.SETNE); break;
                    case Token.MORE: GenerateIntCmp(Instruction.SETG); break;
                    case Token.LESS: GenerateIntCmp(Instruction.SETL); break;
                    case Token.MORE_EQUAL: GenerateIntCmp(Instruction.SETGE); break;
                    case Token.LESS_EQUAL: GenerateIntCmp(Instruction.SETLE); break;
                }

                _g.GenCommand(Instruction.PUSH, new(Register.EAX));
            }
        }

        public void Visit(UnaryOperNode node, bool withResult)
        {
            node.Expr.Accept(this, false);

            if (node.Lexeme == Token.ADD) return;
            if (node.Expr.SymType is SymIntegerType)
            {
                _g.GenCommand(Instruction.POP, new(Register.EAX));
                _g.GenCommand(Instruction.IMUL, new(Register.EAX), new(-1));
                _g.GenCommand(Instruction.PUSH, new(Register.EAX));
            }
            else if (node.Expr.SymType is SymBooleanType)
            {
                _g.GenCommand(Instruction.POP, new(Register.EAX));
                _g.GenCommand(Instruction.XOR, new(Register.EAX), new(1));
                _g.GenCommand(Instruction.PUSH, new(Register.EAX));
            }
            else
            {
                _g.GenCommand(Instruction.MOVSD, new(Register.XMM0),
                    new(Register.ESP, OperandFlag.QWORD, OperandFlag.INDIRECT));
                _g.GenCommand(Instruction.MULSD, new(Register.XMM0),
                    new("double_minus", OperandFlag.QWORD, OperandFlag.INDIRECT));
                _g.GenCommand(Instruction.MOVSD, new(Register.ESP, OperandFlag.QWORD, OperandFlag.INDIRECT),
                    new(Register.XMM0));
            }
            return;
        }

        public void Visit(RecordAccessNode node, bool withResult)
        {
            return;
        }

        public void Visit(ArrayAccessNode node, bool withResult)
        {
            return;
        }

        public void Visit(UserCallNode node, bool withResult)
        {
            _g.GenCommand(Instruction.CALL, new(node.Lexeme.Value));
        }

        public void Visit(WriteCallNode node, bool withResult)
        {
            foreach (var arg in node.Args)
                arg.Accept(this, false);

            var symType = node.Args[0].SymType;

            if (symType is SymIntegerType || symType is SymBooleanType)
                _g.GenCommand(Instruction.PUSH, new("integer_template"));
            else if (symType is SymDoubleType)
                _g.GenCommand(Instruction.PUSH, new("double_template"));
            else if (symType is SymCharType)
                _g.GenCommand(Instruction.PUSH, new("char_template"));

            var size = symType is SymIntegerType || symType is SymBooleanType ? 8 :
                       symType is SymDoubleType ? 12 : 4;

            _g.GenCommand(Instruction.CALL, new("_printf"));
            _g.GenCommand(Instruction.ADD, new(Register.ESP), new(size));
        }

        public void Visit(ReadCallNode node, bool withResult)
        {
            return;
        }

        public void Visit(IdentNode node, bool withResult)
        {
            if (node.SymType is SymDoubleType)
            {
                _g.GenCommand(Instruction.MOV, new(Register.EAX), new($"var_{node.ToString()}"));
                if (withResult)
                    _g.GenCommand(Instruction.PUSH, new(Register.EAX));
                else
                {
                    _g.GenCommand(Instruction.MOVSD, new(Register.XMM0),
                        new(Register.EAX, OperandFlag.QWORD, OperandFlag.INDIRECT));
                    GenerateDoublePush(Register.XMM0);
                }
            }
            else
            {
                var size = node.SymType is SymStringType || node.SymType is SymCharType ? OperandFlag.WORD :
                        node.SymType is SymIntegerType || node.SymType is SymBooleanType ? OperandFlag.DWORD :
                        OperandFlag.BYTE;

                if (withResult)
                    _g.GenCommand(Instruction.PUSH, new($"var_{node.ToString()}"));
                else
                    _g.GenCommand(Instruction.PUSH, new($"var_{node.ToString()}", OperandFlag.INDIRECT, size));
            }
        }

        public void Visit(ConstIntegerLiteral node, bool withResult)
        {
            _g.GenCommand(Instruction.PUSH, new(node.Lexeme.Value, OperandFlag.DWORD));
        }

        public void Visit(ConstDoubleLiteral node, bool withResult)
        {
            var newConst = _g.GenConstant(double.Parse(node.Lexeme.Value.ToString()!));
            _g.GenCommand(Instruction.MOVSD, new(Register.XMM0),
                new(newConst, OperandFlag.INDIRECT, OperandFlag.QWORD));
            _g.GenCommand(Instruction.SUB, new(Register.ESP), new(8));
            _g.GenCommand(Instruction.MOVSD, new(Register.ESP, OperandFlag.INDIRECT, OperandFlag.QWORD),
                new(Register.XMM0));
        }

        public void Visit(ConstCharLiteral node, bool withResult)
        {
            _g.GenCommand(Instruction.PUSH, new($"\'{node.Lexeme.Value}\'"));
        }

        public void Visit(ConstStringLiteral node, bool withResult)
        {
            _g.GenCommand(Instruction.PUSH, new(_g.GenConstant($"\"{node.Lexeme.Value}\", 0xA, 0")));
        }

        public void Visit(ConstBooleanLiteral node, bool withResult)
        {
            _g.GenCommand(Instruction.PUSH, new(node.Lexeme == Token.TRUE ? 1 : 0));
        }

        public void Visit(DeclsPartNode node, bool withResult)
        {
            foreach (var decl in node.Decls)
                decl.Accept(this, withResult);
        }

        public void Visit(ConstDeclNode node, bool withResult)
        {
            var instruction = node.Ident.SymType is SymDoubleType ? Instruction.RESQ : Instruction.RESD;
            var newVar = _g.GenVariable(node.Ident.ToString(), instruction, 1);

            node.Expr.Accept(this, true);

            if (node.Ident.SymType is SymDoubleType)
            {
                GenerateDoublePop(Register.XMM0);
                _g.GenCommand(Instruction.MOVSD, new(newVar, OperandFlag.INDIRECT, OperandFlag.QWORD), new(Register.XMM0));
            }
            else
                _g.GenCommand(Instruction.POP, new(newVar, OperandFlag.DWORD, OperandFlag.INDIRECT));
        }

        public void Visit(VarDeclNode node, bool withResult)
        {
            foreach (var ident in node.IdentsList)
            {
                var instruction = ident.SymType is SymDoubleType ? Instruction.RESQ : Instruction.RESD;
                var newVar = _g.GenVariable(ident.ToString(), instruction, 1);

                if (node.Expr is not null)
                {
                    node.Expr.Accept(this, true);

                    if (ident.SymType is SymDoubleType)
                    {
                        GenerateDoublePop(Register.XMM0);
                        _g.GenCommand(Instruction.MOVSD, new(newVar, OperandFlag.INDIRECT, OperandFlag.QWORD), new(Register.XMM0));
                    }
                    else
                        _g.GenCommand(Instruction.POP, new(newVar, OperandFlag.DWORD, OperandFlag.INDIRECT));
                }
            }
        }

        public void Visit(TypeDeclNode node, bool withResult) { return; }

        public void Visit(CallDeclNode node, bool withResult) { return; }

        public void Visit(CallHeaderNode node, bool withResult) { return; }

        public void Visit(FormalParamNode node, bool withResult) { return; }

        public void Visit(SubroutineBlockNode node, bool withResult)
        {
            foreach (var decl in node.Decls)
                decl.Accept(this, true);

            node.CompoundStmt.Accept(this, true);
        }

        public void Visit(KeywordNode node, bool withResult)
        {
            return;
        }

        public void Visit(CompoundStmtNode node, bool withResult)
        {
            foreach (var stmt in node.Statements)
                stmt.Accept(this, withResult);
        }

        public void Visit(EmptyStmtNode node, bool withResult) { return; }

        public void Visit(CallStmtNode node, bool withResult)
        {
            node.Expression.Accept(this, withResult);
        }

        public void Visit(AssignStmtNode node, bool withResult)
        {
            node.Right.Accept(this, false);

            if (node.Left.SymType is SymIntegerType)
            {
                node.Left.Accept(this, true);

                _g.GenCommand(Instruction.POP, new(Register.ECX));
                _g.GenCommand(Instruction.POP, new(Register.EBX));
                switch (node.Lexeme.Value)
                {
                    case Token.ASSIGN:
                        _g.GenCommand(Instruction.MOV, new(Register.ECX, OperandFlag.INDIRECT), new(Register.EBX)); break;
                    case Token.ADD_ASSIGN:
                        _g.GenCommand(Instruction.ADD, new(Register.ECX, OperandFlag.INDIRECT), new(Register.EBX)); break;
                    case Token.SUB_ASSIGN:
                        _g.GenCommand(Instruction.SUB, new(Register.ECX, OperandFlag.INDIRECT), new(Register.EBX)); break;
                    case Token.MUL_ASSIGN:
                        _g.GenCommand(Instruction.MOV, new(Register.EAX), new(Register.ECX, OperandFlag.INDIRECT));
                        _g.GenCommand(Instruction.IMUL, new(Register.EBX));
                        _g.GenCommand(Instruction.MOV, new(Register.ECX, OperandFlag.INDIRECT, OperandFlag.DWORD), new(Register.EAX)); break;
                }
            }
            else
            {
                node.Left.Accept(this, false);

                GenerateDoublePop(Register.XMM0);
                GenerateDoublePop(Register.XMM1);

                switch (node.Lexeme.Value)
                {
                    case Token.ASSIGN: _g.GenCommand(Instruction.MOVSD, new(Register.XMM0), new(Register.XMM1)); break;
                    case Token.ADD_ASSIGN: _g.GenCommand(Instruction.ADDSD, new(Register.XMM0), new(Register.XMM1)); break;
                    case Token.SUB_ASSIGN: _g.GenCommand(Instruction.SUBSD, new(Register.XMM0), new(Register.XMM1)); break;
                    case Token.MUL_ASSIGN: _g.GenCommand(Instruction.MULSD, new(Register.XMM0), new(Register.XMM1)); break;
                    case Token.DIV_ASSIGN: _g.GenCommand(Instruction.DIVSD, new(Register.XMM0), new(Register.XMM1)); break;
                }

                _g.GenCommand(Instruction.MOVSD, new(Register.EAX, OperandFlag.QWORD, OperandFlag.INDIRECT), new(Register.XMM0));
            }
        }

        public void Visit(IfStmtNode node, bool withResult)
        {
            var ifLabel = _g.AddLabel("if_start");
            var elseLabel = _g.AddLabel("if_else");
            var endLabel = _g.AddLabel("if_end");

            node.Condition.Accept(this, false);

            _g.GenCommand(Instruction.POP, new(Register.EAX));
            _g.GenCommand(Instruction.CMP, new(Register.EAX), new(0));

            if (node.ElsePart is not null)
                _g.GenCommand(Instruction.JE, new(elseLabel));
            else
                _g.GenCommand(Instruction.JE, new(endLabel));

            _g.GenLabel(ifLabel);
            node.IfPart.Accept(this, true);
            _g.GenCommand(Instruction.JMP, new(endLabel));

            if (node.ElsePart is not null)
            {
                _g.GenLabel(elseLabel);
                node.ElsePart.Accept(this, true);
                _g.GenCommand(Instruction.JMP, new(endLabel));
            }

            _g.GenLabel(endLabel);
        }

        public void Visit(WhileStmtNode node, bool withResult)
        {
            var startLabel = _g.AddLabel("while_start");
            var endLabel = _g.AddLabel("while_end");

            _g.GenLabel(startLabel);

            node.Condition.Accept(this, false);

            _g.GenCommand(Instruction.POP, new(Register.EAX));
            _g.GenCommand(Instruction.CMP, new(Register.EAX), new(0));
            _g.GenCommand(Instruction.JE, new(endLabel));

            node.Statement.Accept(this, true);
            _g.GenCommand(Instruction.JMP, new(startLabel));

            _g.GenLabel(endLabel);
        }

        public void Visit(RepeatStmtNode node, bool withResult)
        {
            var startLabel = _g.AddLabel("repeat_start");
            var endLabel = _g.AddLabel("repeat_end");

            _g.GenLabel(startLabel);

            node.Condition.Accept(this, false);

            _g.GenCommand(Instruction.POP, new(Register.EAX));
            _g.GenCommand(Instruction.CMP, new(Register.EAX), new(1));
            _g.GenCommand(Instruction.JE, new(endLabel));

            foreach (var stmt in node.Statements)
                stmt.Accept(this, true);

            _g.GenCommand(Instruction.JMP, new(startLabel));

            _g.GenLabel(endLabel);
        }

        public void Visit(ForStmtNode node, bool withResult)
        {
            var startLabel = _g.AddLabel("for_start");
            var endLabel = _g.AddLabel("for_end");

            node.ForRange.StartValue.Accept(this, false);

            _g.GenCommand(Instruction.POP, new(Register.EAX));
            _g.GenCommand(Instruction.MOV, new($"var_{node.CtrlIdent}", OperandFlag.INDIRECT), new(Register.EAX));

            _g.GenLabel(startLabel);

            node.CtrlIdent.Accept(this, false);
            node.ForRange.FinalValue.Accept(this, true);

            _g.GenCommand(Instruction.POP, new(Register.EBX));
            _g.GenCommand(Instruction.POP, new(Register.EAX));

            if (node.ForRange.Lexeme == Token.TO)
                GenerateIntCmp(Instruction.SETLE);
            else
                GenerateIntCmp(Instruction.SETGE);

            _g.GenCommand(Instruction.CMP, new(Register.EAX), new(0));
            _g.GenCommand(Instruction.JE, new(endLabel));

            node.Statement.Accept(this, false);

            node.CtrlIdent.Accept(this, true);
            _g.GenCommand(Instruction.POP, new(Register.EAX));

            if (node.ForRange.Lexeme == Token.TO)
                _g.GenCommand(Instruction.ADD, new(Register.EAX, OperandFlag.INDIRECT, OperandFlag.DWORD), new(1));
            else
                _g.GenCommand(Instruction.SUB, new(Register.EAX, OperandFlag.INDIRECT, OperandFlag.DWORD), new(1));

            _g.GenCommand(Instruction.JMP, new(startLabel));
            _g.GenLabel(endLabel);
        }

        public void Visit(ForRangeNode node, bool withResult) { return; }

        public void Visit(SimpleTypeNode node, bool withResult) { return; }

        public void Visit(ArrayTypeNode node, bool withResult) { return; }

        public void Visit(SubrangeTypeNode node, bool withResult) { return; }

        public void Visit(RecordTypeNode node, bool withResult) { return; }

        public void Visit(RecordFieldNode node, bool withResult) { return; }

        public void Visit(ConformatArrayTypeNode node, bool withResult) { return; }

        private void GenerateIntCmp(Instruction cmpInstruction)
        {
            _g.GenCommand(Instruction.XOR, new(Register.ECX), new(Register.ECX));
            _g.GenCommand(Instruction.CMP, new(Register.EAX), new(Register.EBX));
            _g.GenCommand(cmpInstruction, new(Register.CL));
            _g.GenCommand(Instruction.MOV, new(Register.EAX), new(Register.ECX));
        }

        private void GenerateDoubleCmp(Instruction cmpInstruction)
        {
            _g.GenCommand(Instruction.XOR, new(Register.ECX), new(Register.ECX));
            _g.GenCommand(Instruction.COMISD, new(Register.XMM0), new(Register.XMM1));
            _g.GenCommand(cmpInstruction, new(Register.CL));
            _g.GenCommand(Instruction.MOV, new(Register.EAX), new(Register.ECX));
        }

        private void GenerateDoublePush(Register register)
        {
            _g.GenCommand(Instruction.SUB, new(Register.ESP), new(8));
            _g.GenCommand(Instruction.MOVSD,
                new Operand(Register.ESP, OperandFlag.QWORD, OperandFlag.INDIRECT), new(register));
        }

        private void GenerateDoublePop(Register register)
        {
            _g.GenCommand(Instruction.MOVSD, new(register),
                new Operand(Register.ESP, OperandFlag.QWORD, OperandFlag.INDIRECT));
            _g.GenCommand(Instruction.ADD, new(Register.ESP), new(8));
        }
    }
}