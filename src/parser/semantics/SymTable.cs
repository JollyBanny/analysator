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

        public int Count { get { return _table.Count; } }

        public void Add(Symbol sym)
        {
            _table.Add(sym.Name.ToLower(), sym);
        }

        public void Remove(string symName)
        {
            _table.Remove(symName);
        }

        public Symbol? Find(string symName)
        {
            return _table.Contains(symName) ? _table[symName] as Symbol : null;
        }

        public bool Contains(string symName)
        {
            return _table.Contains(symName);
        }

        public object? this[string key]
        {
            get => _table[key];
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