using PascalCompiler.Exceptions;

namespace PascalCompiler.Semantics
{
    public class SymStack
    {
        public static readonly SymType SymInt = new SymIntegerType();
        public static readonly SymType SymDouble = new SymDoubleType();
        public static readonly SymType SymChar = new SymCharType();
        public static readonly SymType SymString = new SymStringType();
        public static readonly SymType SymBoolean = new SymBooleanType();

        private Stack<SymTable> _stack;

        public int Count { get { return _stack.Count; } }

        public SymStack()
        {
            _stack = new Stack<SymTable>();
            SetBuiltinsTable();
            Push();
        }

        private void SetBuiltinsTable()
        {
            _stack.Push(new SymTable());
            Add(SymInt);
            Add(SymDouble);
            Add(SymChar);
            Add(SymString);
            Add(SymBoolean);
        }

        public void Push()
        {
            _stack.Push(new SymTable());
        }

        public void Push(SymTable table)
        {
            _stack.Push(table);
        }

        public SymTable Pop()
        {
            return _stack.Pop();
        }

        public void Add(Symbol sym)
        {
            _stack.Peek().Add(sym);
        }

        public void AddConst(string symName, SymType type)
        {
            Add(new SymConstant(symName, type));
        }

        public void AddVar(string symName, SymType type)
        {
            Add(new SymVar(symName, type));
        }

        public void AddAliasType(string symName, SymType type)
        {
            Add(new SymAliasType(symName, type));
        }

        public void AddCall(string symName, SymTable @params, SymTable locals, SymType? type = null)
        {
            if (type is null)
                Add(new SymProc(symName, @params, locals));
            else
                Add(new SymFunc(symName, @params, locals, type));
        }

        public void AddParameter(string symName, SymType type, string modifier = "")
        {
            Add(new SymParameter(symName, type, modifier));
        }

        public void AddEmptySym(string symName)
        {
            Add(new SymVar(symName, null!));
        }

        public void Remove(string symName)
        {
            _stack.Peek().Remove(symName.ToLower());
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

        public SymVar? FindIdent(string symName, bool inScope = false)
        {
            var ident = Find(symName, inScope);

            return ident is SymVar ? ident as SymVar : null;
        }

        public SymProc? FindCall(string symName, bool inScope = false)
        {
            switch (Find(symName, inScope))
            {
                case SymFunc symFunc:
                    return symFunc;
                case SymProc symProc:
                    return symProc;
                default:
                    return null;
            }
        }

        public SymType? FindType(string symName, bool inScope = false)
        {
            switch (Find(symName, inScope))
            {
                case SymAliasType symAliasType:
                    return symAliasType;
                case SymType symType:
                    return symType;
                default:
                    return null;
            }
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

        public void CheckDuplicate(string symName)
        {
            if (Contains(symName, true))
                throw new SemanticException($"Duplicate identifier {symName}");
        }
    }
}