using LexicalAnalyzer.Enums;
using LexicalAnalyzer.Extensions;

namespace LexicalAnalyzer
{
    class Program
    {
        static Lexer lexer = new Lexer();

        static void AnalyzeFile(string path)
        {
            StreamReader fstream = new StreamReader($"./tests/{path}");
            lexer.ChangeFile(fstream);

            Console.ForegroundColor = ConsoleColor.Cyan;
            string text = $" Start test file {path} ";
            Console.WriteLine(
                $"\n[{text.PadLeft((80 - text.Length) / 2 + text.Length, '-').PadRight(80, '-')}]"
            );
            Console.ForegroundColor = ConsoleColor.Gray;
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

        static void AnalyzeTest(string? path = null)
        {
            if (path is not null)
                AnalyzeFile(path);
            else
            {
                var testFiles = Directory.GetFiles("./tests", "*.in")
                 .Select(f => Path.GetFileName(f)).ToList();
                foreach (var file in testFiles)
                    AnalyzeFile(file);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                AnalyzeFile("01_EOF.in");
                return;
            }
            switch (args[0])
            {
                case "-t":
                    if (args.Length == 2)
                        AnalyzeTest(args[1]);
                    else
                        AnalyzeTest();
                    break;
                default:
                    Console.WriteLine("Unknown argument");
                    break;
            }
        }
    }
}