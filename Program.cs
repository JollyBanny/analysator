using LexicalAnalyzer.Enums;
namespace LexicalAnalyzer
{
    class Program
    {
        static void AnalyzeFile(string path)
        {
            StreamReader fstream = new StreamReader($"./tests/{path}");
            var lexer = new Lexer(fstream);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(
                $"[----------------- Start test file {path} -----------------]\n"
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
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(
                $"\n[----------------- End test file {path} -----------------]\n"
            );
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void AnalyzeTest()
        {
            var testFiles = Directory.GetFiles("./tests", "*.in")
                            .Select(f => Path.GetFileName(f));
            foreach (var file in testFiles)
                AnalyzeFile(file);
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                AnalyzeFile("01_string.in");
                return;
            }
            switch (args[0])
            {
                case "-t":
                    AnalyzeTest();
                    break;
                default:
                    Console.WriteLine("Unknown argument");
                    break;
            }

        }
    }
}

