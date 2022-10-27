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
            // float.TryParse("£1,097.63", NumberStyles.Float | NumberStyles.AllowExponent, CultureInfo.CreateSpecificCulture("en-GB"),
            //        out float result);
            // Console.WriteLine(result);
            // double.TryParse("1,2345E-02", NumberStyles.Float | NumberStyles.AllowExponent, null, out double x);
            // .ToString("E", CultureInfo.InvariantCulture);
            // Console.WriteLine(x); // Prints 0.012345
        }
    }
}

