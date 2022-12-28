using System.Collections;
using PascalCompiler.Enums;
using PascalCompiler.Exceptions;
using PascalCompiler.Extensions;
using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.Semantics;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.SyntaxAnalyzer
{
    public partial class Parser
    {
        private Lexer _lexer;
        private Lexeme _currentLexeme;
        public SymStack _symStack = new SymStack();

        public Parser()
        {
            _lexer = new Lexer();
            _currentLexeme = _lexer.GetLexeme();
        }

        public Parser(string path)
        {
            _lexer = new Lexer(path);
            _currentLexeme = _lexer.GetLexeme();
        }

        public SyntaxNode Parse()
        {
            return ParseProgram();
        }

        public void PrintTables()
        {
            var tables = new List<Tuple<string, SymTable>>();
            var global = _symStack.Pop();
            var builtins = _symStack.Pop();

            PrintLine();
            PrintTable("builtins", builtins);
            PrintLine();
            PrintTable("global", global);
        }

        private void PrintTable(string tableName, SymTable table, string indent = "")
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            PrintRow(indent, tableName);
            Console.ResetColor();

            foreach (DictionaryEntry item in table)
                switch (item.Value)
                {
                    case SymAliasType aliasType:
                        PrintRow(indent, "AliasType", $"{item.Key}", $"{aliasType.Origin}");
                        break;
                    case SymArrayType arrayType:
                        PrintRow(indent, "ArrayType", $"{item.Key}", $"{arrayType.ElemType}");
                        break;
                    case SymRecordType recordType:
                        PrintRow(indent, "RecordType", $"{item.Key}");
                        PrintLine();
                        PrintLine(indent + "".PadLeft(4));
                        PrintTable($"{item.Key} fields:", recordType.Table, indent + "".PadLeft(4));
                        break;
                    case SymType symType:
                        PrintRow(indent, "Type", $"{symType}");
                        break;
                    case SymParameter symParam:
                        PrintRow(indent, "Parameter", symParam.Name, $"{symParam.Type}", symParam.Modifier);
                        break;
                    case SymConstant symConst:
                        PrintRow(indent, "Constant", symConst.Name, $"{symConst.Type}");
                        break;
                    case SymVar symVar:
                        PrintRow(indent, "Var", symVar.Name, $"{symVar.Type}");
                        break;
                    case SymProc symProc:
                        if (symProc is SymFunc symFunc)
                        {
                            symProc.Locals.Remove(symProc.Name);
                            PrintRow(indent, "Function", symProc.Name, $"{symFunc.ReturnType}");
                        }
                        else
                            PrintRow(indent, "Procedure", symProc.Name);

                        PrintLine(indent + "".PadLeft(4));
                        PrintTable($"{symProc.Name} params:", symProc.Params, indent + "".PadLeft(4));
                        PrintTable($"{symProc.Name} locals:", symProc.Locals, indent + "".PadLeft(4));
                        break;
                }
        }

        private void PrintLine(string indent = "")
        {
            var tableWidth = 60;
            Console.WriteLine(indent + new string('-', tableWidth));
        }

        private void PrintRow(string indent, params string[] columns)
        {
            var tableWidth = 60;
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns)
                row += AlignCentre(column, width) + "|";

            Console.WriteLine(indent + row);
            PrintLine(indent);
        }

        private string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
                return new string(' ', width);
            else
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
        }

        private void NextLexeme()
        {
            _currentLexeme = _lexer.GetLexeme();
        }

        private void Require<T>(bool getNext, params T[] tokens)
        {
            foreach (var token in tokens)
                if (_currentLexeme.Equals(token))
                {
                    if (getNext)
                        NextLexeme();
                    return;
                }

            if (tokens[0] is Token)
            {
                Token tok = (Token)(object)tokens[0]!;
                throw ExpectedException(tok.Stringify()!, _currentLexeme.Source);
            }

            throw ExpectedException(tokens[0]!.ToString()!, _currentLexeme.Source);
        }

        private SyntaxException ExpectedException(string expected, string found)
        {
            return new SyntaxException(_lexer.Cursor, $"'{expected}' expected but '{found}' found");
        }

        private SyntaxException FatalException(string msg)
        {
            return new SyntaxException(_lexer.Cursor, msg);
        }
    }
}