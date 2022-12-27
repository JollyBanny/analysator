using System.Collections;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.Semantics
{
    public abstract class SymType : Symbol
    {
        protected SymType(string ident) : base(ident)
        {
        }

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
        public SymIntegerType() : base("integer")
        {
        }

        public override bool IsEquivalent(SymIntegerType other) => true;
    }

    public class SymDoubleType : SymType
    {
        public SymDoubleType() : base("double")
        {
        }

        public override bool IsEquivalent(SymDoubleType other) => true;
    }

    public class SymCharType : SymType
    {
        public SymCharType() : base("char")
        {
        }

        public override bool IsEquivalent(SymCharType other) => true;
    }

    public class SymStringType : SymType
    {
        public SymStringType() : base("string")
        {
        }

        public override bool IsEquivalent(SymStringType other) => true;
    }

    public class SymBooleanType : SymType
    {
        public SymBooleanType() : base("boolean")
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
            var type = Origin;

            while (type is SymAliasType)
                type = (type as SymAliasType)!.Origin;

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

        public override bool IsEquivalent(SymRecordType other)
        {
            if (Table.Count != other.Table.Count)
                return false;

            foreach (DictionaryEntry field in Table)
            {
                var item = field.Value as SymParameter;
                var otherItem = other.Table[field.Key.ToString()!] as SymVar;

                if (otherItem is null)
                    return false;

                if (item?.Type.IsEquivalent(otherItem.Type) is false)
                    return false;
            }

            return true;
        }
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
                    (count, type) = (count + 1, arrayType);

                return count;
            }
        }

        public override bool IsEquivalent(SymArrayType other)
        {
            if (DimensionsCount != other.DimensionsCount)
                return false;

            var type = ElemType;
            var otherType = other.ElemType;

            for (int i = 1; i < DimensionsCount; i++)
            {
                type = (type as SymArrayType)!.ElemType;
                otherType = (otherType as SymArrayType)!.ElemType;
            }

            if (!type.IsEquivalent(otherType))
                return false;

            return true;
        }
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