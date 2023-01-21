namespace PascalCompiler.AsmGenerator
{
    public class Command : AsmPart
    {
        public Command(Instruction instruction, params Operand[] operands)
        {
            Instruction = instruction;
            Operands = operands;
        }

        public Instruction Instruction { get; }
        public Operand[] Operands { get; }

        public override string ToString() =>
            $"{Instruction} {string.Join(", ", Operands.Select(o => o.ToString()))}";
    }

    public class Operand
    {
        public Operand(object value)
        {
            Value = value;
            SetOffset = false;
        }

        public Operand(object value, params OperandFlag[] flag)
        {
            Value = value;
            SetOffset = false;

            if (flag.Contains(OperandFlag.INDIRECT))
                OperandFlag = OperandFlag.INDIRECT;
            if (flag.Contains(OperandFlag.WORD))
                SizeFlag = OperandFlag.WORD;
            if (flag.Contains(OperandFlag.WORD))
                SizeFlag = OperandFlag.DWORD;
            if (flag.Contains(OperandFlag.QWORD))
                SizeFlag = OperandFlag.QWORD;
        }

        private object Value { get; set; }
        private bool SetOffset { get; set; }
        private int Offset { get; set; }
        private OperandFlag OperandFlag { get; set; }
        private OperandFlag SizeFlag { get; set; }

        public static Operand operator +(Operand operand, int offset)
        {
            operand.SetOffset = true;
            operand.Offset = offset;
            return operand;
        }

        public static Operand operator -(Operand operand, int offset)
        {
            operand.SetOffset = true;
            operand.Offset = -offset;
            return operand;
        }

        public override string ToString()
        {
            var result = Value.ToString()!;

            if (SetOffset)
                result = $"[{result} {(Offset >= 0 ? "+" : "-")} {Math.Abs(Offset)}]";

            if (OperandFlag == OperandFlag.INDIRECT)
                result = $"[{result}]";

            if (SizeFlag != OperandFlag.NONE)
                result = $"{SizeFlag} {result}";

            return result;
        }
    }
}