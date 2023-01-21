namespace PascalCompiler.AsmGenerator
{
    public enum OperandFlag
    {
        INDERECT,
        WORD,
        DWORD,
        QWORD,
        NONE,
    }

    public enum Instruction
    {
        MUL,
        DIV,

        PUSH, POP, CDQ,
        ADD, SUB, IMUL, IDIV, NEG, INC, DEC,
        OR, XOR, AND, NOT, SHL, SHR,
        MOV,
        CMP,
        SETE, SETNE, SETL, SETG, SETLE, SETGE,
        CALL, RET,

        ADDSD, SUBSD, MULSD, DIVSD,
        MOVSD,
    }

    public enum Register
    {
        AX,
        BX,
        EAX,
        EBX,
        ECX,
        EDX,
        ESP,
        CL,
        XMM0,
        XMM1,

    }

    public enum AccessModifier
    {
        EXTERN,
        GLOBAL,
    }
}