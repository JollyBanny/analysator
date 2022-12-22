namespace PascalCompiler.Semantics
{
    public class SymVar : Symbol
    {
        public SymVar(string ident, SymType type) : base(ident)
        {
            Type = type;
        }

        public SymType Type { get; }

        public override string ToString()
        {
            return $"name: {Name}\ttype: {Type}";
        }
    }

    public class SymConstant : SymVar
    {
        public SymConstant(string ident, SymType type) : base(ident, type)
        {
        }
    }

    public class SymParameter : SymVar
    {
        public SymParameter(string ident, SymType type, string modifier = "") : base(ident, type)
        {
            Modifier = modifier;
        }

        public string Modifier { get; }
    }
}