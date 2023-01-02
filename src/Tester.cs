using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.SimpleSyntaxAnalyzer;
using PascalCompiler.SyntaxAnalyzer;
using PascalCompiler.Enums;
using PascalCompiler.Visitor;

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

        static private bool ParserTest(string inFile, string outFile, string mode)
        {
            var expected = File.ReadAllText(outFile);
            var found = new StringWriter();

            Console.SetOut(found);

            try
            {
                if (mode == "-sp")
                {
                    var _parser = new SimpleParser(inFile);
                    _parser.ParseExpression().PrintTree();
                }
                else
                {
                    var _parser = new Parser(inFile);
                    var syntaxTree = _parser.Parse();
                    syntaxTree.Accept(new SymVisitor(_parser._symStack));

                    if (mode == "-p")
                        syntaxTree.Accept(new PrintVisitor()).PrintTree();
                    else
                        _parser.PrintTables();
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

        static private bool SimpleParserTest(string inFile, string outFile) =>
            ParserTest(inFile, outFile, "-sp");

        static private bool ParserTest(string inFile, string outFile) =>
            ParserTest(inFile, outFile, "-p");

        static private bool SemanticTest(string inFile, string outFile) =>
            ParserTest(inFile, outFile, "-s");

        static public void RunTests(string mode)
        {
            Func<string, string, bool> TestFile = mode switch
            {
                "-l" => LexerTest,
                "-sp" => SimpleParserTest,
                "-p" => ParserTest,
                "-s" => SemanticTest,
                _ => LexerTest,
            };

            var path = "./tests" + mode switch
            {
                "-l" => "/lexer",
                "-sp" => "/simple_parser",
                "-p" => "/parser",
                "-s" => "/semantics",
                _ => "/lexer",
            };

            var files = Directory.GetFiles(path, "*.in").Select((f) => Path.GetFileName(f)[..^3]);

            foreach (var file in files.ToList())
            {
                if (TestFile($"{path}/{file}.in", $"{path}/{file}.out"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Test {file[..2]} OK");
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