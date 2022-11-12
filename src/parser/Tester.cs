namespace PascalCompiler.SyntaxAnalyzer
{
    static class ParserTester
    {
        static private Parser _parser = new Parser();

        static public void AnalyzeFile(string path)
        {
            _parser.ChangeFile(path);
            Console.WriteLine(_parser.ParseExpression());
        }
    }
}