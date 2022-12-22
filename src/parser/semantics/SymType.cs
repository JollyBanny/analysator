using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.Semantics
{
    public abstract class SymType : Symbol
    {
        protected SymType(string? ident = null) : base(ident!)
        {
        }
    }

    public class SymIntegerType : SymType
    {
        public SymIntegerType() : base("integer")
        {
        }
    }

    public class SymDoubleType : SymType
    {
        public SymDoubleType() : base("double")
        {
        }
    }

    public class SymCharType : SymType
    {
        public SymCharType() : base("char")
        {
        }
    }

    public class SymStringType : SymType
    {
        public SymStringType() : base("string")
        {
        }
    }

    public class SymBooleanType : SymType
    {
        public SymBooleanType() : base("boolean")
        {
        }
    }

    public class SymAliasType : SymType
    {
        public SymAliasType(string ident, SymType symType) : base(ident)
        {
            Origin = symType;
        }

        public SymType Origin { get; }

        private SymType GetBase()
        {
            var type = Origin;

            while (type is SymAliasType)
                type = (type as SymAliasType)!.Origin;

            return type;
        }
    }

    public class SymRecordType : SymType
    {
        public SymRecordType(SymTable table) : base("record")
        {
            Table = table;
        }

        public SymTable Table { get; }
    }

    public class SymArrayType : SymType
    {
        public SymArrayType(List<Pair<ExprNode>> ranges, SymType elemType) : base("array")
        {
            Ranges = ranges;
            ElemType = elemType;
        }

        public List<Pair<ExprNode>> Ranges { get; }
        public SymType ElemType { get; }
    }

    public class SymConformatArrayType : SymType
    {
        public SymConformatArrayType(SymType elemType) : base()
        {
            ElemType = elemType;
        }

        public SymType ElemType { get; }
    }
}