using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.SyntaxAnalyzer;
using PascalCompiler.Enums;
using PascalCompiler.Visitor;
using PascalCompiler.AsmGenerator;

namespace PascalCompiler
{
    static class Tester
    {
        static private int _totalSuccess = 0;

        static private bool CompareAnswers(string file, string expected, string found)
        {
            if (expected.Equals(found)) return true;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"File: {file}\nExpected:\n{expected}\nFound:\n{found}");
            Console.ResetColor();
            return false;
        }

        static private bool LexerTest(string inFile, string outFile)
        {
            var ofstream = new StreamReader(outFile);
            var _lexer = new Lexer();
            _lexer.ChangeFile(inFile);

            while (true)
                try
                {
                    var lexeme = _lexer.GetLexeme();
                    var expected = ofstream.ReadLine()!;
                    var found = lexeme.ToString();
                    if (!CompareAnswers(inFile, expected, found)) return false;
                    if (lexeme.Type == TokenType.EOF) break;
                }
                catch (Exception e)
                {
                    var expected = ofstream.ReadLine()!;
                    var found = e.Message;
                    if (!CompareAnswers(inFile, expected, found)) return false;
                    break;
                }

            return true;
        }

        static private bool ParserTest(string inFile, string outFile, bool withSemantics)
        {
            var expected = File.ReadAllText(outFile);
            var found = new StringWriter();

            Console.SetOut(found);

            try
            {
                {
                    var _parser = new Parser(inFile);
                    var syntaxTree = _parser.Parse();
                    syntaxTree.Accept(new SymVisitor(_parser._symStack));

                    syntaxTree.Accept(new PrintVisitor()).PrintTree();

                    if (withSemantics)
                    {
                        Console.WriteLine();
                        _parser.PrintTables();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            var originOutput = new StreamWriter(Console.OpenStandardOutput());
            originOutput.AutoFlush = true;
            Console.SetOut(originOutput);

            if (!CompareAnswers(inFile, expected, found.ToString()))
                return false;

            return true;
        }

        static private bool GeneratorTest(string inFile, string outFile)
        {
            var expected = File.ReadAllText(outFile);
            var found = new StringWriter();

            Console.SetOut(found);

            try
            {
                Parser _parser = new Parser(inFile);
                var syntaxTree = _parser.Parse();
                syntaxTree.Accept(new SymVisitor(_parser._symStack));
                var generator = new Generator(_parser._symStack);
                syntaxTree.Accept(new AsmVisitor(generator), false);
                generator.RunProgram();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            var originOutput = new StreamWriter(Console.OpenStandardOutput());
            originOutput.AutoFlush = true;
            Console.SetOut(originOutput);

            if (!CompareAnswers(inFile, expected, found.ToString()))
                return false;

            return true;
        }

        static private bool ParserTest(string inFile, string outFile) =>
            ParserTest(inFile, outFile, false);

        static private bool SemanticTest(string inFile, string outFile) =>
            ParserTest(inFile, outFile, true);

        static public void RunTests(CompilerFlag mode)
        {
            Func<string, string, bool> TestFile = mode switch
            {
                CompilerFlag.Lexer => LexerTest,
                CompilerFlag.Parser => ParserTest,
                CompilerFlag.Semantics => SemanticTest,
                CompilerFlag.Generator => GeneratorTest,
                _ => LexerTest,
            };

            var path = "./tests" + mode switch
            {
                CompilerFlag.Lexer => "/lexer",
                CompilerFlag.Parser => "/parser",
                CompilerFlag.Semantics => "/semantics",
                CompilerFlag.Generator => "/asm",
                _ => "/lexer",
            };

            var files = Directory.EnumerateFiles(path, "*.in", SearchOption.AllDirectories)
                .Select((f) => f[..^3]).ToList();

            for (var i = 0; i < files.Count; ++i)
            {
                if (TestFile($"{files[i]}.in", $"{files[i]}.out"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Test {(i + 1).ToString().PadLeft(2, '0')} OK");
                    Console.ResetColor();
                    _totalSuccess++;
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nTotal: {_totalSuccess}/{files.Count()}");
            Console.ResetColor();
        }
    }
}