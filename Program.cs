using System.Globalization;

namespace LexicalAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader fstream = new StreamReader("./tests/01.in");
            var lexer = new Lexer(fstream);
            while (true)
            {
                try
                {
                    var lexem = lexer.GetLexem();
                    lexem.GetInfo();
                    if (lexem.Type == Token.EOF.ToString()) break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
            // float result;
            // Console.WriteLine(float.TryParse("1.234568E+002", out result));
            // Console.WriteLine(result);
            // Console.WriteLine($"Exponential: {0.1234567890123:E}");
        }
    }
}

