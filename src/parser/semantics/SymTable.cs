using PascalCompiler.Exceptions;
using System.Collections.Specialized;

namespace PascalCompiler.Semantics
{
    public class SymTable
    {
        private OrderedDictionary _table;

        public SymTable()
        {
            _table = new OrderedDictionary();
        }

        public Symbol? Find(string symName)
        {
            return _table.Contains(symName) ? _table[symName] as Symbol : null;
        }

        public void Add(string symName, Symbol sym)
        {
            try
            {
                _table.Add(symName, sym);
            }
            catch
            {
                throw new SemanticException($"Duplicate identifier {symName}");
            }
        }
    }
}