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
            OperandFlag = OperandFlag.NONE;
        }

        public Operand(object value, OperandFlag flag)
        {
            Value = value;
            SetOffset = false;
            OperandFlag = flag;
        }

        private object Value { get; set; }
        private bool SetOffset { get; set; }
        private int Offset { get; set; }
        private OperandFlag OperandFlag { get; set; }

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

            if (SetOffset is true)
                result = $"[{result} {(Offset >= 0 ? "+" : "-")} {Math.Abs(Offset)}]";

            if (OperandFlag == OperandFlag.INDERECT)
                result = $"[{result}]";
            else if (OperandFlag != OperandFlag.NONE)
                result = $"{OperandFlag} {result}";

            return result;
        }
    }
}