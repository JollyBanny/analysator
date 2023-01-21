namespace PascalCompiler.AsmGenerator
{
    public class AsmPart
    {
    }

    public class Library : AsmPart
    {
        public Library(AccessModifier modifier, string name)
        {
            Modifier = modifier;
            Name = name;
        }

        public AccessModifier Modifier { get; }
        public string Name { get; }

        public override string ToString() => $"{Modifier} _{Name}";
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
}