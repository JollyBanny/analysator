using PascalCompiler.SyntaxAnalyzer.Nodes;

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
        public SymProc(string ident, SymTable @params, SymTable locals, StmtNode? block) : base(ident)
        {
            Params = @params;
            Locals = locals;
            Block = block;
        }

        public SymTable Params { get; }
        public SymTable Locals { get; }
        public StmtNode? Block { get; }
    }

    public class SymFunc : SymProc
    {
        public SymFunc(string ident, SymTable @params, SymTable locals, SymType type, StmtNode? block)
        : base(ident, @params, locals, block)
        {
            ReturnType = type;
        }

        public SymType ReturnType { get; }
    }
}