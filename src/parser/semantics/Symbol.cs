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

    public class SymProc : Symbol
    {
        public SymProc(string ident, SymTable @params, SymTable locals) : base(ident)
        {
            Params = @params;
            Locals = locals;
        }

        public SymTable Params { get; }
        public SymTable Locals { get; }
    }

    public class SymFunc : SymProc
    {
        public SymFunc(string ident, SymTable @params, SymTable locals, SymType type)
        : base(ident, @params, locals)
        {
            ReturnType = type;
        }

        public SymType ReturnType { get; }
    }
}