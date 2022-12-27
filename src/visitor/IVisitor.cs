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
}