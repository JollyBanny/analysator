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

            PrintTable("builtins", builtins);
            PrintTable("global", global);
        }

        private void PrintTable(string tableName, SymTable table, string indent = "")
        {
            PrintLine(TableLinePos.Start, indent);
            PrintRow(indent, TableLinePos.Center, null, tableName);

            foreach (DictionaryEntry item in table)
            {
                var linePos = item.Value == table.Last() ? TableLinePos.End : TableLinePos.Center;

                switch (item.Value)
                {
                    case SymProc symProc:
                        PrintRow(indent, linePos, symProc);

                        PrintTable($"{symProc.Name} params:", symProc.Params, indent + "".PadLeft(4));
                        PrintTable($"{symProc.Name} locals:", symProc.Locals, indent + "".PadLeft(4));
                        break;
                    default:
                        PrintRow(indent, linePos, item.Value as Symbol);
                        break;

                }
            }
        }

        private void PrintLine(TableLinePos linePos, string indent = "")
        {
            var tableWidth = 120;
            var line = new string('─', tableWidth - 2);
            var template = linePos == TableLinePos.Start ? @"┌{0}┐" : linePos == TableLinePos.End ? @"└{0}┘" : @"├{0}┤";

            if (linePos == TableLinePos.Start) Console.ForegroundColor = ConsoleColor.DarkGray;
            else Console.ResetColor();

            Console.WriteLine(indent + string.Format(template, line));
        }

        private void PrintRow(string indent, TableLinePos linePos, Symbol? sym, string title = "")
        {
            var columns = sym switch
            {
                SymAliasType aliasType => new string[] { "AliasType", aliasType.Name, aliasType.Origin.ToString() },
                SymType symType => new string[] { "Type", symType.ToString() },
                SymParameter symParam => new string[] { "Parameter", symParam.Name, symParam.Type.ToString(), symParam.Modifier },
                SymConstant symConst => new string[] { "Constant", symConst.Name, symConst.Type.ToString() },
                SymVar symVar => new string[] { "Var", symVar.Name, symVar.Type.ToString() },
                SymFunc symFunc => new string[] { "Function", symFunc.Name, symFunc.ReturnType.ToString() },
                SymProc symProc => new string[] { "Procedure", symProc.Name, "No return type" },
                _ => new string[] { title },
            };

            var width = columns.Length switch
            {
                2 => new int[] { 20, 99 },
                3 => new int[] { 20, 30, 69 },
                4 => new int[] { 20, 30, 59, 10 },
                _ => new int[] { 119 }
            };

            var row = "│";
            for (var i = 0; i < columns.Length; i++)
                row += columns[i].PadLeft(columns[i].Length + 2).PadRight(width[i] - 1) + "│";

            Console.WriteLine(indent + row);
            PrintLine(linePos, indent);
        }

        private void NextLexeme() => _currentLexeme = _lexer.GetLexeme();

        private void Require<T>(bool getNext, params T[] tokens)
        {
            foreach (var token in tokens)
                if (_currentLexeme.Equals(token))
                {
                    if (getNext) NextLexeme();
                    return;
                }

            var expectedArg = tokens[0] is Token ?
                ((Token)(object)tokens[0]!).Stringify() : tokens[0]!.ToString()!;

            throw ExpectedException(expectedArg, _currentLexeme.Source);
        }

        private SyntaxException ExpectedException(string expected, string found) =>
            new SyntaxException(_lexer.Cursor, $"'{expected}' expected but '{found}' found");

        private SyntaxException FatalException(string msg) =>
            new SyntaxException(_lexer.Cursor, msg);
    }
}