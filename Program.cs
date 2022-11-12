using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.SyntaxAnalyzer;

namespace PascalCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                LexerTester.AnalyzeFile("./tests/lexer/01_EOF.in");
                return;
            }
            switch (args[0])
            {
                case "-pt":
                    ParserTester.AnalyzeFile("./tests/parser/01_simple_add.in");
                    break;
                case "-lt":
                    if (args.Length > 1)
                        LexerTester.AnalyzeFile(path: args[1]);
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