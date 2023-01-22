using PascalCompiler.AsmGenerator;
using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.Semantics;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.Visitor
{
    public class AsmVisitor : IVisitor<Generator>
    {
        private Generator _g;

        public AsmVisitor(SymStack stack)
        {
            _g = new Generator(stack);
        }

        public Generator Visit(FullProgramNode node)
        {

            _g.AddModule(Instruction.GLOBAL, "main");
            _g.AddModule(Instruction.EXTERN, "printf");
            _g.AddModule(Instruction.EXTERN, "scanf");

            node.Header?.Accept(this);
            node.Block.Accept(this);

            return _g;
        }

        public Generator Visit(ProgramHeaderNode node)
        {
            return _g;
        }

        public Generator Visit(ProgramBlockNode node)
        {
            _g.GenLabel("main");

            foreach (var decl in node.Decls)
                decl.Accept(this);

            node.CompoundStmt.Accept(this);

            _g.GenCommand(Instruction.RET);
            return _g;
        }

        public Generator Visit(CastNode node)
        {
            node.Expr.Accept(this);
            _g.GenCommand(Instruction.POP, new(Register.EAX));
            _g.GenCommand(Instruction.CVTSI2SD, new(Register.XMM0), new(Register.EAX));
            GenerateDoublePush(Register.XMM0);
            return _g;
        }

        public Generator Visit(BinOperNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);

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
                        _g.GenCommand(Instruction.MOVSD,
                            new Operand(Register.ESP, OperandFlag.QWORD, OperandFlag.INDIRECT),
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

                        if (node.Lexeme == Token.MOD)
                            _g.GenCommand(Instruction.MOV, new(Register.EAX), new(Register.EDX));
                        break;
                    case Token.O_SHL:
                    case Token.SHL:
                    case Token.O_SHR:
                    case Token.SHR:
                        var instruction = node.Lexeme == Token.SHL || node.Lexeme == Token.O_SHL ?
                            Instruction.SHL : Instruction.SHR;

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

            return _g;
        }

        public Generator Visit(UnaryOperNode node)
        {
            node.Expr.Accept(this);

            if (node.Lexeme == Token.ADD) return _g;
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
            return _g;
        }

        public Generator Visit(RecordAccessNode node)
        {
            return _g;
        }

        public Generator Visit(ArrayAccessNode node)
        {
            return _g;
        }

        public Generator Visit(UserCallNode node)
        {
            _g.GenCommand(Instruction.CALL, new(node.Lexeme.Value));

            return _g;
        }

        public Generator Visit(WriteCallNode node)
        {
            foreach (var arg in node.Args)
                arg.Accept(this);

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

            return _g;
        }

        public Generator Visit(ReadCallNode node)
        {
            return _g;
        }

        public Generator Visit(IdentNode node)
        {
            _g.GenCommand(Instruction.MOV, new(Register.EAX), new($"var_{node.ToString()}", OperandFlag.INDIRECT));

            if (node.SymType is SymDoubleType)
            {
                _g.GenCommand(Instruction.MOVSD, new(Register.XMM0),
                       new(Register.EAX, OperandFlag.INDIRECT, OperandFlag.QWORD));
                GenerateDoublePush(Register.XMM0);
            }
            else
                _g.GenCommand(Instruction.PUSH, new(Register.EAX));

            return _g;
        }

        public Generator Visit(ConstIntegerLiteral node)
        {
            _g.GenCommand(Instruction.PUSH, new(node.Lexeme.Value, OperandFlag.DWORD));
            return _g;
        }

        public Generator Visit(ConstDoubleLiteral node)
        {
            var newConst = _g.GenConstant(double.Parse(node.Lexeme.Value.ToString()!));
            _g.GenCommand(Instruction.MOVSD, new(Register.XMM0),
                new(newConst, OperandFlag.INDIRECT, OperandFlag.QWORD));
            _g.GenCommand(Instruction.SUB, new(Register.ESP), new(8));
            _g.GenCommand(Instruction.MOVSD, new(Register.ESP, OperandFlag.INDIRECT, OperandFlag.QWORD),
                new(Register.XMM0));
            return _g;
        }

        public Generator Visit(ConstCharLiteral node)
        {
            _g.GenCommand(Instruction.PUSH, new($"\'{node.Lexeme.Value}\'"));
            return _g;
        }

        public Generator Visit(ConstStringLiteral node)
        {
            var newConst = _g.GenConstant($"\"{node.Lexeme.Value}\", 0xA, 0");
            _g.GenCommand(Instruction.PUSH, new(newConst));
            return _g;
        }

        public Generator Visit(ConstBooleanLiteral node)
        {
            _g.GenCommand(Instruction.PUSH, new(node.Lexeme == Token.TRUE ? 1 : 0, OperandFlag.DWORD));
            return _g;
        }

        public Generator Visit(DeclsPartNode node)
        {
            foreach (var decl in node.Decls)
                decl.Accept(this);

            return _g;
        }

        public Generator Visit(ConstDeclNode node)
        {
            return _g;
        }

        public Generator Visit(VarDeclNode node)
        {
            foreach (var ident in node.IdentsList)
            {
                var instruction = ident.SymType is SymDoubleType ? Instruction.RESQ : Instruction.RESD;
                _g.GenVariable($"var_{ident.ToString()}", instruction);

                if (node.Expr is not null)
                {
                    node.Expr.Accept(this);

                    _g.GenCommand(Instruction.POP, new(Register.EAX));
                    _g.GenCommand(Instruction.MOV, new($"var_{ident.ToString()}", OperandFlag.INDIRECT),
                        new(Register.EAX));
                }
            }
            return _g;
        }

        public Generator Visit(TypeDeclNode node)
        {
            return _g;
        }

        public Generator Visit(CallDeclNode node)
        {
            return _g;
        }

        public Generator Visit(CallHeaderNode node)
        {
            return _g;
        }

        public Generator Visit(FormalParamNode node)
        {
            return _g;
        }

        public Generator Visit(SubroutineBlockNode node)
        {
            return _g;
        }

        public Generator Visit(KeywordNode node)
        {
            return _g;
        }

        public Generator Visit(CompoundStmtNode node)
        {
            foreach (var stmt in node.Statements)
                stmt.Accept(this);

            return _g;
        }

        public Generator Visit(EmptyStmtNode node)
        {
            return _g;
        }

        public Generator Visit(CallStmtNode node)
        {
            node.Expression.Accept(this);
            return _g;
        }

        public Generator Visit(AssignStmtNode node)
        {
            return _g;
        }

        public Generator Visit(IfStmtNode node)
        {
            return _g;
        }

        public Generator Visit(WhileStmtNode node)
        {
            return _g;
        }

        public Generator Visit(RepeatStmtNode node)
        {
            return _g;
        }

        public Generator Visit(ForStmtNode node)
        {
            return _g;
        }

        public Generator Visit(ForRangeNode node)
        {
            return _g;
        }

        public Generator Visit(SimpleTypeNode node)
        {
            return _g;
        }

        public Generator Visit(ArrayTypeNode node)
        {
            return _g;
        }

        public Generator Visit(SubrangeTypeNode node)
        {
            return _g;
        }

        public Generator Visit(RecordTypeNode node)
        {
            return _g;
        }

        public Generator Visit(RecordFieldNode node)
        {
            return _g;
        }

        public Generator Visit(ConformatArrayTypeNode node)
        {
            return _g;
        }

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