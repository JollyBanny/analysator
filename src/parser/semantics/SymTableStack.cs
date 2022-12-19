namespace PascalCompiler.Semantics
{
    public class SymTableStack
    {
        private Stack<SymTable> _stack;

        public SymTableStack()
        {
            _stack = new Stack<SymTable>();
            SetBuiltinsTable();
            Push();
        }

        public int Count { get { return _stack.Count; } }

        private void SetBuiltinsTable()
        {
            _stack.Push(new SymTable());
            Add("integer", new SymIntegerType());
            Add("double", new SymDoubleType());
            Add("string", new SymStringType());
            Add("char", new SymCharType());
            Add("boolean", new SymBooleanType());
        }

        public void Push()
        {
            _stack.Push(new SymTable());
        }

        public SymTable Pop()
        {
            return _stack.Pop();
        }

        public void Add(string symName, Symbol sym)
        {
            var table = _stack.Peek();
            table.Add(symName.ToLower(), sym);
        }

        public void Remove(string symName)
        {
            var table = _stack.Peek();
            table.Remove(symName.ToLower());
        }

        public Symbol? Find(string symName, bool inScope = false)
        {
            if (inScope)
                return _stack.Peek().Find(symName.ToLower());

            foreach (var table in _stack)
            {
                var symbol = table.Find(symName.ToLower());
                if (symbol is not null)
                    return symbol;
            }

            return null;
        }

        public SymType? FindType(string symName)
        {
            var symType = Find(symName);

            if (symType is SymType)
                return symType as SymType;
            else
                return null;
        }

        public bool Contains(string symName, bool inScope = false)
        {
            if (inScope)
                return _stack.Peek().Contains(symName.ToLower());

            foreach (var table in _stack)
                if (table.Contains(symName.ToLower()))
                    return true;

            return false;
        }
    }
}