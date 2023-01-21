namespace PascalCompiler.AsmGenerator
{
    public class AsmPart
    {
    }

    public class Library : AsmPart
    {
        public Library(Instruction instruction, string name)
        {
            Instruction = instruction;
            Name = name;
        }

        public Instruction Instruction { get; }
        public string Name { get; }

        public override string ToString() => $"{Instruction} _{Name}";
    }

    public class Label : AsmPart
    {
        public Label(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString() => $"_{Name}:";
    }

    public class Data : AsmPart
    {
        public Data(Instruction instruction, string label, object value)
        {
            Instruction = instruction;
            Label = label;
            Value = value;
        }

        public Instruction Instruction { get; }
        public string Label { get; }
        public object Value { get; }

        public override string ToString() => $"{Label}: {Instruction} {Value}";
    }
}