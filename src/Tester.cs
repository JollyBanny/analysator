using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.SyntaxAnalyzer;
using PascalCompiler.Enums;
using PascalCompiler.Exceptions;

namespace PascalCompiler
{
    static class Tester
    {
        static private bool CompareAnswers(string file, string expected, string found)
        {
            if (expected == found) return true;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"File: {file}\nExpected:\n{expected}\nFound:\n{found}");
            Console.ResetColor();
            return false;
        }

        static private bool LexerTest(string inFile, string outFile)
        {
            var _lexer = new Lexer();
            var ofstream = new StreamReader(outFile);
            _lexer.ChangeFile(inFile);
            while (true)
                try
                {
                    var lexeme = _lexer.GetLexem();
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

        static private bool ParserTest(string inFile, string outFile)
        {
            // var ostrm = new FileStream(outFile, FileMode.OpenOrCreate, FileAccess.Write);
            // var writer = new StreamWriter(ostrm);
            // writer.AutoFlush = true;
            // Console.SetOut(writer);
            var ofstream = new StreamReader(outFile);
            var expected = ofstream.ReadToEnd();

            var buffer = new StringWriter();
            Console.SetOut(buffer);
            try
            {
                var _parser = new Parser(inFile);
                _parser.ParseExpression().PrintTree();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            var originOutput = new StreamWriter(Console.OpenStandardOutput());
            originOutput.AutoFlush = true;
            Console.SetOut(originOutput);

            if (!CompareAnswers(inFile, expected, buffer.ToString()))
                return false;
            return true;
        }

        static public void RunTests(string mode)
        {
            Func<string, string, bool> TestFile = mode == "-l" ?
                LexerTest : ParserTest;
            var path = "./tests" + (mode == "-l" ? "/lexer" : "/parser");
            var files = Directory.GetFiles(path, "*.in")
                .Select((f) => Path.GetFileName(f)[..^3]).ToList();

            var total = 0;
            foreach (var file in files)
            {
                if (TestFile($"{path}/{file}.in", $"{path}/{file}.out"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Test {file[..2]} OK");
                    Console.ResetColor();
                    total++;
                }
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nTotal: {total}/{files.Capacity}");
            Console.ResetColor();
        }
    }
}