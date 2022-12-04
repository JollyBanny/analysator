using PascalCompiler.Enums;
using PascalCompiler.Exceptions;
using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SimpleSyntaxAnalyzer
{
    class SimpleParser
    {
        private Lexer _lexer;
        private Lexeme _currentLexem;

        public SimpleParser()
        {
            _lexer = new Lexer();
            _currentLexem = _lexer.GetLexeme();
        }

        public SimpleParser(string path)
        {
            _lexer = new Lexer(path);
            _currentLexem = _lexer.GetLexeme();
        }

        public SyntaxNode ParseExpression()
        {
            var left = ParseTerm();
            var lexeme = _currentLexem;
            while (lexeme == TokenType.Operator &&
                (lexeme == Token.ADD || lexeme == Token.SUB))
            {
                _currentLexem = _lexer.GetLexeme();
                left = new BinOperNode(lexeme, left, ParseTerm());
                lexeme = _currentLexem;
            }
            return left;
        }

        private SyntaxNode ParseTerm()
        {
            var left = ParseFactor();
            var lexeme = _currentLexem;
            while (lexeme == TokenType.Operator &&
                (lexeme == Token.MUL || lexeme == Token.O_DIV))
            {
                _currentLexem = _lexer.GetLexeme();
                left = new BinOperNode(lexeme, left, ParseFactor());
                lexeme = _currentLexem;
            }
            return left;
        }

        private SyntaxNode ParseFactor()
        {
            var lexeme = _currentLexem;
            switch (lexeme.Type)
            {
                case TokenType.Integer or TokenType.Double:
                    _currentLexem = _lexer.GetLexeme();
                    return new NumberNode(lexeme);
                case TokenType.Identifire:
                    _currentLexem = _lexer.GetLexeme();
                    return new IdentifireNode(lexeme);
                case TokenType.Separator when lexeme == Token.LPAREN:
                    _currentLexem = _lexer.GetLexeme();
                    var exp = ParseExpression();
                    if (!(_currentLexem == TokenType.Separator && _currentLexem == Token.RPAREN))
                        throw new SyntaxException(_lexer.Cursor, "Expected right paren");
                    _currentLexem = _lexer.GetLexeme();
                    return exp;
            }
            throw new SyntaxException(lexeme.Pos, "Expected factor");
        }

        public void ChangeFile(string path)
        {
            _lexer.ChangeFile(path);
            _currentLexem = _lexer.GetLexeme();
        }
    }
}