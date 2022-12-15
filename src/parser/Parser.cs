using PascalCompiler.Enums;
using PascalCompiler.Exceptions;
using PascalCompiler.Extensions;
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

        private void NextLexeme()
        {
            _currentLexeme = _lexer.GetLexeme();
        }

        private void Require<T>(bool getNext, params T[] tokens)
        {
            foreach (var token in tokens)
                if (_currentLexeme.Equals(token))
                {
                    if (getNext)
                        NextLexeme();
                    return;
                }
            if (tokens[0] is Token)
            {
                Token tok = (Token)(object)tokens[0]!;
                throw ExpectedException(tok.Stringify()!, _currentLexeme.Source);
            }

            throw ExpectedException(tokens[0]!.ToString()!, _currentLexeme.Source);
        }

        private SyntaxException ExpectedException(string expected, string found)
        {
            return new SyntaxException(_lexer.Cursor,
                        $"'{expected}' expected but '{found}' found");
        }

        private SyntaxException FatalException(string msg)
        {
            return new SyntaxException(_lexer.Cursor, msg);
        }

        public void ChangeFile(string path)
        {
            _lexer.ChangeFile(path);
            _currentLexeme = _lexer.GetLexeme();
        }
    }
}