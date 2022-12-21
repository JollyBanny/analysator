using System.Collections;
using System.Collections.Specialized;

namespace PascalCompiler.Semantics
{
    public class SymTable : IEnumerable
    {
        private OrderedDictionary _table;

        public SymTable()
        {
            _table = new OrderedDictionary();
        }

        public void Add(Symbol sym)
        {
            _table.Add(sym.Name.ToLower(), sym);
        }

        public Symbol? Find(string symName)
        {
            return _table.Contains(symName) ? _table[symName] as Symbol : null;
        }

        public bool Contains(string symName)
        {
            return _table.Contains(symName);
        }

        public void Remove(string symName)
        {
            _table.Remove(symName);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}