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

            _g.GenLibrary(AccessModifier.GLOBAL, "main");
            _g.GenLibrary(AccessModifier.EXTERN, "printf");
            _g.GenLibrary(AccessModifier.EXTERN, "scanf");

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
            foreach (var decl in node.Decls)
                decl.Accept(this);

            _g.GenLabel("main");

            node.CompoundStmt.Accept(this);

            _g.GenCommand(Instruction.RET);
            return _g;
        }

        public Generator Visit(CastNode node)
        {
            return _g;
        }

        public Generator Visit(BinOperNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);

            if (node.Left.SymType is SymDoubleType)
            {
                _g.GenCommand(Instruction.MOVSD,
                    new(Register.XMM1), new Operand(Register.ESP, OperandFlag.QWORD) + 8);
                _g.GenCommand(Instruction.MOVSD,
                    new(Register.XMM0), new Operand(Register.ESP, OperandFlag.QWORD) + 0);

                _g.GenCommand(Instruction.ADD, new(Register.ESP), new(16));

                switch (node.Lexeme.Value)
                {
                    case Token.ADD:
                        _g.GenCommand(Instruction.ADDSD, new(Register.XMM0), new(Register.XMM1));
                        break;
                    case Token.SUB:
                        _g.GenCommand(Instruction.SUB, new(Register.EAX), new(Register.EBX));
                        break;
                    case Token.MUL:
                        _g.GenCommand(Instruction.IMUL, new(Register.EAX), new(Register.EBX));
                        break;
                    case Token.DIV:
                        break;
                    case Token.EQUAL:
                        GenerateIntCmp(Instruction.SETE);
                        break;
                    case Token.NOT_EQUAL:
                        GenerateIntCmp(Instruction.SETNE);
                        break;
                    case Token.MORE:
                        GenerateIntCmp(Instruction.SETG);
                        break;
                    case Token.LESS:
                        GenerateIntCmp(Instruction.SETL);
                        break;
                    case Token.MORE_EQUAL:
                        GenerateIntCmp(Instruction.SETGE);
                        break;
                    case Token.LESS_EQUAL:
                        GenerateIntCmp(Instruction.SETLE);
                        break;
                    default:
                        break;
                }

                _g.GenCommand(Instruction.SUB, new(Register.ESP), new(8));
                _g.GenCommand(Instruction.MOVSD,
                    new Operand(Register.ESP, OperandFlag.QWORD) + 0, new(Register.XMM1));

                _g.GenCommand(Instruction.PUSH, new(Register.ESP));
            }
            else
            {
                _g.GenCommand(Instruction.POP, new(Register.EBX));
                _g.GenCommand(Instruction.POP, new(Register.EAX));

                switch (node.Lexeme.Value)
                {
                    case Token.ADD:
                        _g.GenCommand(Instruction.ADD, new(Register.EAX), new(Register.EBX));
                        break;
                    case Token.SUB:
                        _g.GenCommand(Instruction.SUB, new(Register.EAX), new(Register.EBX));
                        break;
                    case Token.MUL:
                        _g.GenCommand(Instruction.IMUL, new(Register.EAX), new(Register.EBX));
                        break;
                    case Token.O_DIV:
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
                    case Token.AND:
                        _g.GenCommand(Instruction.AND, new(Register.EAX), new(Register.EBX));
                        break;
                    case Token.OR:
                        _g.GenCommand(Instruction.OR, new(Register.EAX), new(Register.EBX));
                        break;
                    case Token.XOR:
                        _g.GenCommand(Instruction.XOR, new(Register.EAX), new(Register.EBX));
                        break;
                    case Token.EQUAL:
                        GenerateIntCmp(Instruction.SETE);
                        break;
                    case Token.NOT_EQUAL:
                        GenerateIntCmp(Instruction.SETNE);
                        break;
                    case Token.MORE:
                        GenerateIntCmp(Instruction.SETG);
                        break;
                    case Token.LESS:
                        GenerateIntCmp(Instruction.SETL);
                        break;
                    case Token.MORE_EQUAL:
                        GenerateIntCmp(Instruction.SETGE);
                        break;
                    case Token.LESS_EQUAL:
                        GenerateIntCmp(Instruction.SETLE);
                        break;
                }

                _g.GenCommand(Instruction.PUSH, new(Register.EAX));
            }

            return _g;
        }

        public Generator Visit(UnaryOperNode node)
        {
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

            _g.GenCommand(Instruction.PUSH, new("msg"));
            _g.GenCommand(Instruction.CALL, new("_printf"));
            _g.GenCommand(Instruction.ADD, new(Register.ESP), new(12));

            return _g;
        }

        public Generator Visit(ReadCallNode node)
        {
            return _g;
        }

        public Generator Visit(IdentNode node)
        {
            return _g;
        }

        public Generator Visit(ConstIntegerLiteral node)
        {
            _g.GenCommand(Instruction.PUSH, new(node.Lexeme.Value, OperandFlag.DWORD));
            return _g;
        }

        public Generator Visit(ConstDoubleLiteral node)
        {
            return _g;
        }

        public Generator Visit(ConstCharLiteral node)
        {
            return _g;
        }

        public Generator Visit(ConstStringLiteral node)
        {
            return _g;
        }

        public Generator Visit(ConstBooleanLiteral node)
        {
            return _g;
        }

        public Generator Visit(DeclsPartNode node)
        {
            return _g;
        }

        public Generator Visit(ConstDeclNode node)
        {
            return _g;
        }

        public Generator Visit(VarDeclNode node)
        {
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
            _g.GenCommand(Instruction.CMP, new(Register.EAX), new(Register.EBX));
            _g.GenCommand(cmpInstruction, new(Register.EAX), new(Register.EBX));
            _g.GenCommand(Instruction.MOV, new(Register.EAX), new(Register.EBX));
        }
    }
}