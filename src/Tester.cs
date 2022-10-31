using LexicalAnalyzer.Enums;

namespace LexicalAnalyzer.Test
{
    static class Test
    {
        static private Lexer lexer = new Lexer();

        static public void AnalyzeFile(string path)
        {
            StreamReader fstream = new StreamReader($"./tests/{path}");
            lexer.ChangeFile(fstream);
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
            fstream.Close();
        }

        static private void TestFile(string testFile, string checkFile)
        {
            StreamReader fstream = new StreamReader($"./tests/{testFile}");
            StreamReader ofstream = new StreamReader($"./tests/{checkFile}");
            lexer.ChangeFile(fstream);
            while (true)
                try
                {
                    var lexem = lexer.GetLexem();
                    string expected = ofstream.ReadLine()!;
                    string found = lexem.ToString();
                    if (expected != found)
                    {
                        Console.WriteLine($"File: {testFile}\nExpected:\n{expected}\nFound:\n{found}");
                        break;
                    }
                    if (lexem.Type == TokenType.EOF) break;
                }
                catch (Exception e)
                {
                    string expected = ofstream.ReadLine()!;
                    string found = e.Message;
                    if (expected != found)
                        Console.WriteLine($"File: {testFile}\nExpected:\n{expected}\nFound:\n{found}");
                    break;
                }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Test {testFile.Substring(0, 2)} OK");
            Console.ForegroundColor = ConsoleColor.Gray;
            fstream.Close();
        }

        static public void RunTests()
        {
            var testFiles = Directory.GetFiles("./tests", "*.in")
                .Select(f => Path.GetFileName(f)).ToList();
            var checkFiles = Directory.GetFiles("./tests", "*.out")
                .Select(f => Path.GetFileName(f)).ToList();
            for (int i = 0; i < testFiles.Capacity; ++i)
                TestFile(testFiles[i], checkFiles[i]);
        }
    }
}