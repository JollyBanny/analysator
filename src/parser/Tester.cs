namespace PascalCompiler.SyntaxAnalyzer
{
    static class ParserTester
    {
        static private Parser _parser = new Parser();

        static public void AnalyzeFile(string path)
        {
            _parser.ChangeFile(path);
            _parser.ParseExpression().PrintTree(0, new List<int>());
        }
    }
}