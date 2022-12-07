using PascalCompiler.Exceptions;
using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer
{
    public partial class Parser
    {
        private Lexer _lexer;
        private Lexeme _currentLexeme;

        public Parser()
        {
            _lexer = new Lexer();
            _currentLexeme = _lexer.GetLexeme();
        }

        public Parser(string path)
        {
            _lexer = new Lexer(path);
            _currentLexeme = _lexer.GetLexeme();
        }

        public void ChangeFile(string path)
        {
            _lexer.ChangeFile(path);
            _currentLexeme = _lexer.GetLexeme();
        }

        private SyntaxException ExpectedException(string expected, string found)
        {
            return new SyntaxException(_lexer.Cursor,
                        $"'{expected}' expected but '{found}' found");
        }

        private SyntaxException FatalException(string msg)
        {
            return new SyntaxException(_lexer.Cursor, $"{msg}");
        }
    }
}