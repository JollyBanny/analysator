using static LexicalAnalyzer.Test.Test;

namespace LexicalAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                AnalyzeFile("./tests/01_EOF.in");
                return;
            }
            switch (args[0])
            {
                case "-t":
                    if (args.Length > 1)
                        AnalyzeFile(args[1]);
                    else
                        RunTests();
                    break;
                default:
                    Console.WriteLine("Unknown argument");
                    break;
            }
        }
    }
}