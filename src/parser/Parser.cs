using PascalCompiler.Enums;
using PascalCompiler.Exceptions;
using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer
{
    class Parser
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

        // public StmtNode ParseStatement()
        // {
        //     var lexeme = _currentLexem;
        // }

        // public StmtNode ParseVariables()
        // {
        // }

        public StmtNode ParseWhile()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();
            var cond = ParseRelExpression();
            if (_currentLexeme != Token.DO)
                throw new SyntaxException(_currentLexeme.Pos,
                    $"'{Token.DO}' expected but '{_currentLexeme.Value}' found");
            _currentLexeme = _lexer.GetLexeme();
            StmtNode body = null;
            return new WhileStmtNode(lexeme, cond, body);
        }

        public ExprNode ParseRelExpression()
        {
            var left = ParseExpression();
            var lexeme = _currentLexeme;
            if (lexeme == TokenType.Operator &&
                    (lexeme == Token.EQUAL || lexeme == Token.NOT_EQUAL ||
                    lexeme == Token.MORE || lexeme == Token.MORE_EQUAL ||
                    lexeme == Token.LESS || lexeme == Token.LESS_EQUAL) ||
                lexeme == TokenType.Keyword &&
                    (lexeme == Token.IN || lexeme == Token.IS)
                )
            {
                _currentLexeme = _lexer.GetLexeme();
                left = new RelOperNode(lexeme, left, ParseExpression());
            }
            return left;
        }

        public ExprNode ParseExpression()
        {
            var left = ParseTerm();
            var lexeme = _currentLexeme;
            while (lexeme == TokenType.Operator &&
                    (lexeme == Token.ADD || lexeme == Token.SUB) ||
                  lexeme == TokenType.Keyword &&
                    (lexeme == Token.OR || lexeme == Token.XOR))
            {
                _currentLexeme = _lexer.GetLexeme();
                left = new BinOperNode(lexeme, left, ParseTerm());
                lexeme = _currentLexeme;
            }
            return left;
        }

        private ExprNode ParseTerm()
        {
            var left = ParseFactor();
            var lexeme = _currentLexeme;
            while (lexeme == TokenType.Operator &&
                    (lexeme == Token.MUL || lexeme == Token.O_DIV ||
                    lexeme == Token.O_SHL || lexeme == Token.O_SHR) ||
                  lexeme == TokenType.Keyword &&
                    (lexeme == Token.DIV || lexeme == Token.MOD ||
                    lexeme == Token.AND || lexeme == Token.AS ||
                    lexeme == Token.SHL || lexeme == Token.SHR))
            {
                _currentLexeme = _lexer.GetLexeme();
                left = new BinOperNode(lexeme, left, ParseFactor());
                lexeme = _currentLexeme;
            }
            return left;
        }

        private ExprNode ParseSimpleTerm() 
        {
            var lexeme = _currentLexeme;
        }

        private ExprNode ParseFactor()
        {
            var lexeme = _currentLexeme;
            switch (lexeme.Type)
            {
                case TokenType.Integer or TokenType.Double:
                    _currentLexeme = _lexer.GetLexeme();
                    return new NumberNode(lexeme);
                case TokenType.Identifire:
                    _currentLexeme = _lexer.GetLexeme();
                    return new IdentifireNode(lexeme);
                case TokenType.Separator when lexeme == Token.LPAREN:
                    _currentLexeme = _lexer.GetLexeme();
                    var exp = ParseRelExpression();
                    if (!(_currentLexeme == TokenType.Separator && _currentLexeme == Token.RPAREN))
                        throw new SyntaxException(_lexer.Cursor, "Expected right paren");
                    _currentLexeme = _lexer.GetLexeme();
                    return exp;
            }
            throw new SyntaxException(lexeme.Pos, "Expected factor");
        }

        public void ChangeFile(string path)
        {
            _lexer.ChangeFile(path);
            _currentLexeme = _lexer.GetLexeme();
        }
    }
}