using PascalCompiler.Enums;
using PascalCompiler.Exceptions;
using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer
{
    class Parser
    {
        private Lexer _lexer;
        private Lexeme _currentLexem;

        public Parser()
        {
            _lexer = new Lexer();
            _currentLexem = _lexer.GetLexem();
        }

        public Parser(string path)
        {
            _lexer = new Lexer(path);
            _currentLexem = _lexer.GetLexem();
        }

        public SyntaxNode ParseExpression()
        {
            var left = ParseTerm();
            var lexeme = _currentLexem;
            while (lexeme.Type == TokenType.Operator &&
                (lexeme.CastValue<Token>() == Token.ADD ||
                    lexeme.CastValue<Token>() == Token.SUB))
            {
                _currentLexem = _lexer.GetLexem();
                left = new BinOperNode(lexeme, left, ParseTerm());
                lexeme = _currentLexem;
            }
            return left;
        }

        private SyntaxNode ParseTerm()
        {
            SyntaxNode left = ParseFactor();
            var lexeme = _currentLexem;
            while (lexeme.Type == TokenType.Operator &&
                (lexeme.CastValue<Token>() == Token.MUL ||
                    lexeme.CastValue<Token>() == Token.O_DIV))
            {
                _currentLexem = _lexer.GetLexem();
                if (lexeme.CastValue<Token>() == Token.O_DIV)
                    left = new BinOperNode(lexeme, left, ParseFactor());
                if (lexeme.CastValue<Token>() == Token.MUL)
                    left = new BinOperNode(lexeme, left, ParseTerm());

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
                    _currentLexem = _lexer.GetLexem();
                    return new NumberNode(lexeme);
                case TokenType.Identifire:
                    _currentLexem = _lexer.GetLexem();
                    return new IdentifireNode(lexeme);
                case TokenType.Separator when lexeme.CastValue<Token>() == Token.LPAREN:
                    _currentLexem = _lexer.GetLexem();
                    var exp = ParseExpression();
                    if (_currentLexem.Type != TokenType.Separator ||
                         _currentLexem.CastValue<Token>() != Token.RPAREN)
                        throw new SyntaxException(_lexer.Cursor, "Expected right paren");
                    _currentLexem = _lexer.GetLexem();
                    return exp;
            }
            throw new SyntaxException(_lexer.Cursor, "Expected factor");
        }

        public void ChangeFile(string path)
        {
            _lexer.ChangeFile(path);
            _currentLexem = _lexer.GetLexem();
        }
    }
}