using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.Visitor
{
    public interface IVisitor<T>
    {
        // program nodes visit
        T Visit(FullProgramNode node);
        T Visit(ProgramHeaderNode node);
        T Visit(ProgramBlockNode node);

        // expression nodes visit
        T Visit(BinOperNode node);
        T Visit(CastNode node);
        T Visit(UnaryOperNode node);
        T Visit(RecordAccessNode node);
        T Visit(ArrayAccessNode node);
        T Visit(UserCallNode node);
        T Visit(WriteCallNode node);
        T Visit(ReadCallNode node);

        T Visit(IdentNode node);
        T Visit(ConstIntegerLiteral node);
        T Visit(ConstDoubleLiteral node);
        T Visit(ConstCharLiteral node);
        T Visit(ConstStringLiteral node);
        T Visit(ConstBooleanLiteral node);

        // declaration nodes visit
        T Visit(DeclsPartNode node);
        T Visit(ConstDeclNode node);
        T Visit(VarDeclNode node);
        T Visit(TypeDeclNode node);
        T Visit(CallDeclNode node);
        T Visit(CallHeaderNode node);
        T Visit(FormalParamNode node);
        T Visit(SubroutineBlockNode node);
        T Visit(KeywordNode node);

        // statement nodes visit
        T Visit(CompoundStmtNode node);
        T Visit(EmptyStmtNode node);
        T Visit(CallStmtNode node);
        T Visit(AssignStmtNode node);
        T Visit(IfStmtNode node);
        T Visit(WhileStmtNode node);
        T Visit(RepeatStmtNode node);
        T Visit(ForStmtNode node);
        T Visit(ForRangeNode node);

        // type nodes visit
        T Visit(SimpleTypeNode node);
        T Visit(ArrayTypeNode node);
        T Visit(SubrangeTypeNode node);
        T Visit(RecordTypeNode node);
        T Visit(RecordFieldNode node);
        T Visit(ConformatArrayTypeNode node);
    }

    public interface IGenVisitor
    {
        // program nodes visit
        void Visit(FullProgramNode node, bool withResult);
        void Visit(ProgramHeaderNode node, bool withResult);
        void Visit(ProgramBlockNode node, bool withResult);

        // expression nodes visit
        void Visit(BinOperNode node, bool withResult);
        void Visit(CastNode node, bool withResult);
        void Visit(UnaryOperNode node, bool withResult);
        void Visit(RecordAccessNode node, bool withResult);
        void Visit(ArrayAccessNode node, bool withResult);
        void Visit(UserCallNode node, bool withResult);
        void Visit(WriteCallNode node, bool withResult);
        void Visit(ReadCallNode node, bool withResult);

        void Visit(IdentNode node, bool withResult);
        void Visit(ConstIntegerLiteral node, bool withResult);
        void Visit(ConstDoubleLiteral node, bool withResult);
        void Visit(ConstCharLiteral node, bool withResult);
        void Visit(ConstStringLiteral node, bool withResult);
        void Visit(ConstBooleanLiteral node, bool withResult);

        // declaration nodes visit
        void Visit(DeclsPartNode node, bool withResult);
        void Visit(ConstDeclNode node, bool withResult);
        void Visit(VarDeclNode node, bool withResult);
        void Visit(TypeDeclNode node, bool withResult);
        void Visit(CallDeclNode node, bool withResult);
        void Visit(CallHeaderNode node, bool withResult);
        void Visit(FormalParamNode node, bool withResult);
        void Visit(SubroutineBlockNode node, bool withResult);
        void Visit(KeywordNode node, bool withResult);

        // statement nodes visit
        void Visit(CompoundStmtNode node, bool withResult);
        void Visit(EmptyStmtNode node, bool withResult);
        void Visit(CallStmtNode node, bool withResult);
        void Visit(AssignStmtNode node, bool withResult);
        void Visit(IfStmtNode node, bool withResult);
        void Visit(WhileStmtNode node, bool withResult);
        void Visit(RepeatStmtNode node, bool withResult);
        void Visit(ForStmtNode node, bool withResult);
        void Visit(ForRangeNode node, bool withResult);

        // type nodes visit
        void Visit(SimpleTypeNode node, bool withResult);
        void Visit(ArrayTypeNode node, bool withResult);
        void Visit(SubrangeTypeNode node, bool withResult);
        void Visit(RecordTypeNode node, bool withResult);
        void Visit(RecordFieldNode node, bool withResult);
        void Visit(ConformatArrayTypeNode node, bool withResult);
    }
}