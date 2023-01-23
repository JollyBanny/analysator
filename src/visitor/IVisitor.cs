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

    public interface IGenVisitor<T>
    {
        // program nodes visit
        T Visit(FullProgramNode node, bool withResult);
        T Visit(ProgramHeaderNode node, bool withResult);
        T Visit(ProgramBlockNode node, bool withResult);

        // expression nodes visit
        T Visit(BinOperNode node, bool withResult);
        T Visit(CastNode node, bool withResult);
        T Visit(UnaryOperNode node, bool withResult);
        T Visit(RecordAccessNode node, bool withResult);
        T Visit(ArrayAccessNode node, bool withResult);
        T Visit(UserCallNode node, bool withResult);
        T Visit(WriteCallNode node, bool withResult);
        T Visit(ReadCallNode node, bool withResult);

        T Visit(IdentNode node, bool withResult);
        T Visit(ConstIntegerLiteral node, bool withResult);
        T Visit(ConstDoubleLiteral node, bool withResult);
        T Visit(ConstCharLiteral node, bool withResult);
        T Visit(ConstStringLiteral node, bool withResult);
        T Visit(ConstBooleanLiteral node, bool withResult);

        // declaration nodes visit
        T Visit(DeclsPartNode node, bool withResult);
        T Visit(ConstDeclNode node, bool withResult);
        T Visit(VarDeclNode node, bool withResult);
        T Visit(TypeDeclNode node, bool withResult);
        T Visit(CallDeclNode node, bool withResult);
        T Visit(CallHeaderNode node, bool withResult);
        T Visit(FormalParamNode node, bool withResult);
        T Visit(SubroutineBlockNode node, bool withResult);
        T Visit(KeywordNode node, bool withResult);

        // statement nodes visit
        T Visit(CompoundStmtNode node, bool withResult);
        T Visit(EmptyStmtNode node, bool withResult);
        T Visit(CallStmtNode node, bool withResult);
        T Visit(AssignStmtNode node, bool withResult);
        T Visit(IfStmtNode node, bool withResult);
        T Visit(WhileStmtNode node, bool withResult);
        T Visit(RepeatStmtNode node, bool withResult);
        T Visit(ForStmtNode node, bool withResult);
        T Visit(ForRangeNode node, bool withResult);

        // type nodes visit
        T Visit(SimpleTypeNode node, bool withResult);
        T Visit(ArrayTypeNode node, bool withResult);
        T Visit(SubrangeTypeNode node, bool withResult);
        T Visit(RecordTypeNode node, bool withResult);
        T Visit(RecordFieldNode node, bool withResult);
        T Visit(ConformatArrayTypeNode node, bool withResult);
    }
}