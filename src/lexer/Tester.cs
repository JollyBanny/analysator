using PascalCompiler.Enums;

namespace PascalCompiler.LexicalAnalyzer
{
    static class LexerTester
    {
        static private bool TestFile(string testFile, string checkFile)
        {
            Lexer _lexer = new Lexer();
            StreamReader ofstream = new StreamReader(checkFile);
            _lexer.ChangeFile(testFile);
            while (true)
                try
                {
                    var lexeme = _lexer.GetLexem();
                    string expected = ofstream.ReadLine()!;
                    string found = lexeme.ToString();
                    if (expected != found)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"File: {testFile}\nExpected:\n{expected}\nFound:\n{found}");
                        Console.ResetColor();
                        return false;
                    }
                    if (lexeme.Type == TokenType.EOF) break;
                }
                catch (Exception e)
                {
                    string expected = ofstream.ReadLine()!;
                    string found = e.Message;
                    if (expected != found)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"File: {testFile}\nExpected:\n{expected}\nFound:\n{found}");
                        Console.ResetColor();
                        return false;
                    }
                    break;
                }
            _lexer.CloseFile();
            return true;
        }

        static public void RunTests()
        {
            var testFiles = Directory.GetFiles("./tests/lexer", "*.in");
            var checkFiles = Directory.GetFiles("./tests/lexer", "*.out");
            int total = 0;
            for (int i = 0; i < testFiles.Length; ++i)
                if (TestFile(testFiles[i], checkFiles[i]))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Test {(i + 1).ToString().PadLeft(2, '0')} OK");
                    Console.ResetColor();

                    total++;
                }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nTotal: {total}/{testFiles.Length}");
            Console.ResetColor();
        }
    }
}