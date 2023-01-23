namespace PascalCompiler.AsmGenerator
{
    public enum OperandFlag
    {
        NONE,
        INDIRECT,
        BYTE, WORD, DWORD, QWORD,
    }

    public enum Instruction
    {
        EXTERN, GLOBAL,
        PUSH, POP, CDQ, DB, DD, DQ, RESD, RESQ,
        ADD, SUB, IMUL, IDIV,
        OR, XOR, AND, NOT, SHL, SHR,
        MOV,
        CMP,
        JE, JNE, JL, JG, JLE, JGE, JMP,
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
        AX, BX,
        EAX, EBX, ECX, EDX,
        ESP,
        CL,
        XMM0, XMM1,
    }
}