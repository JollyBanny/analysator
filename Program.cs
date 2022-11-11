using PascalCompiler.Lexer.Test;

namespace PascalCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                LexerTester.AnalyzeFile("./tests/01_EOF.in");
                return;
            }
            switch (args[0])
            {
                case "-t":
                    if (args.Length > 1)
                        LexerTester.AnalyzeFile(args[1]);
                    else
                        LexerTester.RunTests();
                    break;
                default:
                    Console.WriteLine("Unknown argument");
                    break;
            }
        }
    }
}