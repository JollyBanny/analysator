using LexicalAnalyzer.Enums;

namespace LexicalAnalyzer.Test
{
    static class Test
    {
        static private Lexer lexer = new Lexer();

        static public void AnalyzeFile(string path)
        {
            lexer.ChangeFile(path);
            while (true)
            {
                try
                {
                    var lexem = lexer.GetLexem();
                    Console.WriteLine(lexem.ToString());
                    if (lexem.Type == TokenType.EOF) break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
            lexer.CloseFile();
        }

        static private bool TestFile(string testFile, string checkFile)
        {
            StreamReader ofstream = new StreamReader($"./tests/{checkFile}");
            lexer.ChangeFile(path: $"./tests/{testFile}");
            while (true)
                try
                {
                    var lexem = lexer.GetLexem();
                    string expected = ofstream.ReadLine()!;
                    string found = lexem.ToString();
                    if (expected != found)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"File: {testFile}\nExpected:\n{expected}\nFound:\n{found}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        return false;
                    }
                    if (lexem.Type == TokenType.EOF) break;
                }
                catch (Exception e)
                {
                    string expected = ofstream.ReadLine()!;
                    string found = e.Message;
                    if (expected != found)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"File: {testFile}\nExpected:\n{expected}\nFound:\n{found}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        return false;
                    }
                    break;
                }
            lexer.CloseFile();
            return true;
        }

        static public void RunTests()
        {
            var testFiles = Directory.GetFiles("./tests", "*.in")
                .Select(f => Path.GetFileName(f)).ToList();
            var checkFiles = Directory.GetFiles("./tests", "*.out")
                .Select(f => Path.GetFileName(f)).ToList();
            for (int i = 0; i < testFiles.Capacity; ++i)
                if (TestFile(testFiles[i], checkFiles[i]))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Test {(i + 1).ToString().PadLeft(2, '0')} OK");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else return;
        }
    }
}