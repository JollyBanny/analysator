using PascalCompiler.Exceptions;
using PascalCompiler.SyntaxAnalyzer.Nodes;

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

        public void AddCall(string symName, SymTable @params, SymTable locals, StmtNode? block, SymType? type)
        {
            if (type is null)
                Add(new SymProc(symName, @params, locals, block));
            else
                Add(new SymFunc(symName, @params, locals, type, block));
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

            if (ident is SymVar)
                return ident as SymVar;
            else
                return null;
        }

        public SymProc? FindProc(string symName, bool inScope = false)
        {
            var proc = Find(symName, inScope);

            if (proc is SymFunc)
                return proc as SymFunc;
            if (proc is SymProc)
                return proc as SymProc;
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

        public SymType? FindType(string symName)
        {
            var symType = Find(symName);

            return symType is SymType ? symType as SymType : null;
        }

        public SymProc? FindCall(string symName)
        {
            var symType = Find(symName);

            return symType is SymFunc || symType is SymProc ? symType as SymProc : null;
        }

        public SymType GetSymType(TypeNode typeNode)
        {
            SymType? symType;

            switch (typeNode)
            {
                case RecordTypeNode type:
                    var symRecord = new SymRecordType(new SymTable());

                    foreach (var field in type.FieldsList)
                    {
                        symType = GetSymType(field.Type);
                        foreach (var ident in field.IdentsList)
                            symRecord.Table.Add(new SymVar(ident.Lexeme.ToString()!, symType));
                    }

                    return symRecord;

                case ArrayTypeNode type:
                    var ranges = new List<Pair<ExprNode>>();
                    var symArray = new SymArrayType(ranges, GetSymType(type.Type));

                    foreach (var range in type.Ranges)
                        ranges.Add(new Pair<ExprNode>(range.LeftBound, range.RightBound));

                    return symArray;

                case ConformatArrayTypeNode type:
                    return new SymConformatArrayType(GetSymType(type.Type));

                default:
                    var typeName = typeNode.Lexeme.Value.ToString()!;
                    symType = FindType(typeName);

                    if (symType is not null)
                        return symType;
                    else
                        throw new SemanticException($"type '{typeName}' not found");
            }
        }

        public void CheckDuplicate(string symName)
        {
            if (Contains(symName, true))
                throw new SemanticException($"Duplicate identifier {symName}");
        }

        public void CheckPreLastScopeDuplicate(string symName)
        {
            CheckDuplicate(symName);
            var cache_table = Pop();

            if (Contains(symName, true))
                throw new SemanticException($"Duplicate identifier {symName}");

            Push(cache_table);
        }
    }
}