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
            PrintLine(PTPos.Start, indent);
            PrintRow(indent, PTPos.Center, tableName);

            foreach (DictionaryEntry item in table)
            {
                var sym = item.Value;
                var ptpos = sym == table.Last() ? PTPos.End : PTPos.Center;

                switch (sym)
                {
                    case SymAliasType aliasType:
                        PrintRow(indent, ptpos, "AliasType", aliasType.Name, aliasType.Origin.ToString());
                        break;
                    case SymType symType:
                        PrintRow(indent, ptpos, "Type", symType.ToString());
                        break;
                    case SymParameter symParam:
                        PrintRow(indent, ptpos, "Parameter", symParam.Name, symParam.Type.ToString(), symParam.Modifier);
                        break;
                    case SymConstant symConst:
                        PrintRow(indent, ptpos, "Constant", symConst.Name, symConst.Type.ToString());
                        break;
                    case SymVar symVar:
                        PrintRow(indent, ptpos, "Var", symVar.Name, symVar.Type.ToString());
                        break;
                    case SymProc symProc:
                        if (symProc is SymFunc symFunc)
                            PrintRow(indent, ptpos, "Function", symProc.Name, symFunc.ReturnType.ToString());
                        else
                            PrintRow(indent, ptpos, "Procedure", symProc.Name);

                        PrintTable($"{symProc.Name} params:", symProc.Params, indent + "".PadLeft(4));
                        PrintTable($"{symProc.Name} locals:", symProc.Locals, indent + "".PadLeft(4));
                        Console.WriteLine();
                        break;
                }
            }
        }

        private void PrintLine(PTPos ptpos, string indent = "")
        {
            var tableWidth = 120;
            var line = ptpos switch
            {
                PTPos.Start => $"┌{new string('─', tableWidth - 2)}┐",
                PTPos.Center => $"├{new string('─', tableWidth - 2)}┤",
                PTPos.End => $"└{new string('─', tableWidth - 2)}┘",
                _ => ""
            };

            if (ptpos == PTPos.Start)
                Console.ForegroundColor = ConsoleColor.DarkGray;
            else
                Console.ResetColor();

            Console.WriteLine(indent + line);
        }

        private void PrintRow(string indent, PTPos ptpos, params string[] columns)
        {
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
            PrintLine(ptpos, indent);
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