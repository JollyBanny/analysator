using System.Collections;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.Semantics
{
    public abstract class SymType : Symbol
    {
        protected SymType(string ident, int size = 0) : base(ident)
        {
            Size = size;
        }

        public int Size { get; }

        public virtual bool IsEquivalent(SymType other)
        {
            switch (other)
            {
                case SymAliasType symAliasType:
                    return IsEquivalent(symAliasType);
                case SymArrayType symArrayType:
                    return IsEquivalent(symArrayType);
                case SymBooleanType SymBooleanType:
                    return IsEquivalent(SymBooleanType);
                case SymCharType symCharType:
                    return IsEquivalent(symCharType);
                case SymConformatArrayType symConformatArrayType:
                    return IsEquivalent(symConformatArrayType);
                case SymDoubleType SymDoubleType:
                    return IsEquivalent(SymDoubleType);
                case SymIntegerType symInteger:
                    return IsEquivalent(symInteger);
                case SymRecordType symRecordType:
                    return IsEquivalent(symRecordType);
                case SymStringType symStringType:
                    return IsEquivalent(symStringType);
            }

            return false;
        }

        public virtual bool IsEquivalent(SymIntegerType other) => false;
        public virtual bool IsEquivalent(SymDoubleType other) => false;
        public virtual bool IsEquivalent(SymCharType other) => false;
        public virtual bool IsEquivalent(SymStringType other) => false;
        public virtual bool IsEquivalent(SymBooleanType other) => false;
        public virtual bool IsEquivalent(SymAliasType other) => IsEquivalent(other.Origin);
        public virtual bool IsEquivalent(SymRecordType other) => false;
        public virtual bool IsEquivalent(SymArrayType other) => false;
        public virtual bool IsEquivalent(SymConformatArrayType other) => false;
    }

    public class SymIntegerType : SymType
    {
        public SymIntegerType() : base("integer", 4)
        {
        }

        public override bool IsEquivalent(SymIntegerType other) => true;
    }

    public class SymDoubleType : SymType
    {
        public SymDoubleType() : base("double", 8)
        {
        }

        public override bool IsEquivalent(SymDoubleType other) => true;
    }

    public class SymCharType : SymType
    {
        public SymCharType() : base("char", 4)
        {
        }

        public override bool IsEquivalent(SymCharType other) => true;
    }

    public class SymStringType : SymType
    {
        public SymStringType() : base("string", 4)
        {
        }

        public override bool IsEquivalent(SymStringType other) => true;
    }

    public class SymBooleanType : SymType
    {
        public SymBooleanType() : base("boolean", 4)
        {
        }

        public override bool IsEquivalent(SymBooleanType other) => true;
    }

    public class SymAliasType : SymType
    {
        public SymAliasType(string ident, SymType symType) : base(ident)
        {
            Origin = symType;
        }

        public SymType Origin { get; }

        public SymType GetBase()
        {
            for (var type = Origin; true; type = (type as SymAliasType)!.Origin)
                if (type is not SymAliasType)
                    return type;
        }

        public override bool IsEquivalent(SymType other) => GetBase().IsEquivalent(other);
    }

    public class SymRecordType : SymType
    {
        public SymRecordType(SymTable table) : base("record")
        {
            Table = table;
        }

        public SymTable Table { get; }

        public Dictionary<string, string> StringTable
        {
            get
            {
                var dictionary = new Dictionary<string, string>();

                foreach (DictionaryEntry field in Table)
                {
                    var item = (field.Value as SymVar)!;
                    dictionary.Add(item.Name, item.Type.ToString());
                }

                return dictionary;
            }
        }

        public override bool IsEquivalent(SymRecordType other) =>
            StringTable.Count == other.StringTable.Count && !StringTable.Except(other.StringTable).Any();

        public override string ToString() =>
            $"{{ {string.Join(", ", StringTable.Select(kv => kv.Key + ": " + kv.Value))} }}";
    }

    public class SymArrayType : SymType
    {
        public SymArrayType(Pair<ExprNode> range, SymType elemType) : base("array")
        {
            Range = range;
            ElemType = elemType;
        }

        public Pair<ExprNode> Range { get; }
        public SymType ElemType { get; }

        public int DimensionsCount
        {
            get
            {
                var type = ElemType;
                var count = 1;

                while (type is SymArrayType arrayType)
                    (count, type) = (count + 1, arrayType.ElemType);

                return count;
            }
        }

        public SymType Origin
        {
            get
            {
                for (var type = ElemType; true; type = (type as SymArrayType)!.ElemType)
                    if (type is not SymArrayType)
                        return type;
            }
        }

        public override bool IsEquivalent(SymArrayType other) =>
            DimensionsCount == other.DimensionsCount && Origin.IsEquivalent(other.Origin);

        public override string ToString() =>
            string.Concat(Enumerable.Repeat("array of ", DimensionsCount)) + Origin;
    }

    public class SymConformatArrayType : SymType
    {
        public SymConformatArrayType(SymType elemType) : base("сonformat array")
        {
            ElemType = elemType;
        }

        public SymType ElemType { get; }
    }
}