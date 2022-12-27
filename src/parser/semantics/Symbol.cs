namespace PascalCompiler.Semantics
{
    public abstract class Symbol
    {
        protected Symbol(string name)
        {
            Name = name.ToLower();
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}