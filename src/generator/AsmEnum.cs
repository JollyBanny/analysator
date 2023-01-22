namespace PascalCompiler.AsmGenerator
{
    public enum OperandFlag
    {
        NONE,
        INDIRECT,
        WORD,
        DWORD,
        QWORD,
    }

    public enum Instruction
    {
        EXTERN, GLOBAL,
        PUSH, POP, CDQ, DD, DQ,
        ADD, SUB, IMUL, IDIV, NEG, INC, DEC,
        OR, XOR, AND, NOT, SHL, SHR,
        MOV,
        CMP,
        SETE, SETNE, SETL, SETG, SETLE, SETGE,
        SETA, SETB, SETAE, SETBE,
        CALL, RET,

        ADDSD, SUBSD, MULSD, DIVSD,
        MOVSD,
        COMISD,
        CVTSI2SD,
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
}