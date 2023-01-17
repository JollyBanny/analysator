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

        public int Count { get => _table.Count; }

        public object? this[string key] { get => _table[key]; }

        public object? this[int key] { get => _table[key]; }

        public void Add(Symbol sym) => _table.Add(sym.Name.ToLower(), sym);

        public void Remove(string symName) => _table.Remove(symName);

        public Symbol? Find(string symName) => _table[symName] as Symbol ?? null;

        public bool Contains(string symName) => _table.Contains(symName);

        public object? Last() => _table[Count - 1];

        public IDictionaryEnumerator GetEnumerator() => _table.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}