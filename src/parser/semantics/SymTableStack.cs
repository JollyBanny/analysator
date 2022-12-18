namespace PascalCompiler.Semantics
{
    public class SymTableStack
    {
        private Stack<SymTable> _stack;

        public SymTableStack()
        {
            _stack = new Stack<SymTable>();
            _stack.Push(new SymTable());
        }

        public void Push()
        {
            _stack.Push(new SymTable());
        }

        public void Pop()
        {
            _stack.Pop();
        }

        public void Add(string symName, Symbol sym)
        {
            var table = _stack.Peek();
            table.Add(symName, sym);
        }

        public Symbol? Find(string symName, bool inScope)
        {
            if (inScope)
            {
                if (!_stack.TryPeek(out var table))
                    return null;

                return table.Find(symName.ToLower());
            }

            foreach (var table in _stack)
            {
                var symbol = table.Find(symName);
                if (symbol is not null)
                    return symbol;
            }

            return null;
        }

        public bool Contains(string symName, bool inScope)
        {
            if (inScope)
                return Find(symName, true) is not null;

            return Find(symName, false) is not null;
        }
    }
}