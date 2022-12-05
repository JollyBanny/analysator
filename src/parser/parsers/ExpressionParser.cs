using PascalCompiler.Enums;
using PascalCompiler.Extensions;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.SyntaxAnalyzer
{
    public partial class Parser
    {
        private static readonly List<Token> RelationalOperators = new List<Token>
        {
            Token.EQUAL, Token.NOT_EQUAL, Token.MORE, Token.LESS,
            Token.MORE_EQUAL, Token.LESS_EQUAL, Token.IN, Token.IS,
        };

        private static readonly List<Token> AdditionOperators = new List<Token>
        {
            Token.ADD, Token.SUB, Token.OR, Token.XOR,
        };

        private static readonly List<Token> MultiplyOperators = new List<Token>
        {
            Token.MUL, Token.DIV, Token.O_SHL, Token.O_SHR,
            Token.AND, Token.AS, Token.SHL, Token.SHR,
        };

        private static readonly List<Token> UnaryOperators = new List<Token>
        {
            Token.ADD, Token.SUB,
        };

        public ExprNode ParseRelExpression()
        {
            var left = ParseExpression();
            var lexeme = _currentLexeme;
            if (RelationalOperators.Contains(lexeme))
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
            while (AdditionOperators.Contains(lexeme))
            {
                _currentLexeme = _lexer.GetLexeme();
                left = new BinOperNode(lexeme, left, ParseTerm());
                lexeme = _currentLexeme;
            }
            return left;
        }

        private ExprNode ParseTerm()
        {
            var left = ParseSimpleTerm();
            var lexeme = _currentLexeme;
            while (MultiplyOperators.Contains(lexeme))
            {
                _currentLexeme = _lexer.GetLexeme();
                left = new BinOperNode(lexeme, left, ParseSimpleTerm());
                lexeme = _currentLexeme;
            }
            return left;
        }

        private ExprNode ParseSimpleTerm()
        {
            var lexeme = _currentLexeme;
            if (UnaryOperators.Contains(lexeme))
            {
                _currentLexeme = _lexer.GetLexeme();
                return new UnaryOperNode(lexeme, ParseSimpleTerm());
            }
            return ParseFactor();
        }

        private ExprNode ParseFactor()
        {
            var lexeme = _currentLexeme;

            switch (lexeme.Type)
            {
                case TokenType.Identifier or TokenType.Keyword:
                    return ParseVarReference();
                case TokenType.Integer:
                    _currentLexeme = _lexer.GetLexeme();
                    return new ConstIntegerLiteral(lexeme);
                case TokenType.Double:
                    _currentLexeme = _lexer.GetLexeme();
                    return new ConstDoubleLiteral(lexeme);
                case TokenType.Char:
                    _currentLexeme = _lexer.GetLexeme();
                    return new ConstCharLiteral(lexeme);
                case TokenType.String:
                    _currentLexeme = _lexer.GetLexeme();
                    return new ConstStringLiteral(lexeme);
                case TokenType.Separator when lexeme == Token.LPAREN:
                    _currentLexeme = _lexer.GetLexeme();
                    var exp = ParseRelExpression();

                    if (_currentLexeme != Token.RPAREN)
                        throw CreateException(
                            $"')' expected but '{_currentLexeme.Value}' found");

                    _currentLexeme = _lexer.GetLexeme();
                    return exp;
                default:
                    throw CreateException("Illegal expression");
            }
        }

        private ExprNode ParseVarReference()
        {
            var left = ParseIdent();
            var lexeme = _currentLexeme;

            while (true)
            {
                if (lexeme == Token.LBRACK)
                {
                    _currentLexeme = _lexer.GetLexeme();
                    var @params = ParseParamsList();

                    if (@params.Count == 0)
                        throw CreateException("indexes expected");

                    left = new ArrayAccessNode(left, @params);

                    if (_currentLexeme != Token.RBRACK)
                        throw CreateException(
                            $"']' expected but '{_currentLexeme.Source}' found");
                    else
                        _currentLexeme = _lexer.GetLexeme();

                    lexeme = _currentLexeme;
                }
                else if (lexeme == Token.DOT)
                {
                    _currentLexeme = _lexer.GetLexeme();
                    left = new RecordAccessNode(left, ParseIdent());
                    lexeme = _currentLexeme;
                }
                else if (lexeme == Token.LPAREN)
                {
                    _currentLexeme = _lexer.GetLexeme();
                    List<ExprNode> args = new List<ExprNode>();
                    if (_currentLexeme != Token.RPAREN)
                        args = ParseParamsList();

                    if (_currentLexeme != Token.RPAREN)
                        throw CreateException(
                            $"')' expected but '{_currentLexeme.Source}' found");
                    else
                        _currentLexeme = _lexer.GetLexeme();

                    if (left is not IdentNode)
                        throw CreateException(
                        $"Function name expected but '{lexeme.Source}' found");

                    left = left.Lexeme == TokenType.Write ||
                            left.Lexeme == TokenType.Writeln ?
                        new WriteCallNode((IdentNode)left, args,
                            left.Lexeme == TokenType.Writeln) :
                        new FunctionCallNode((IdentNode)left, args);

                    lexeme = _currentLexeme;
                }
                else
                    return left;
            }
        }

        public List<ExprNode> ParseParamsList()
        {
            var paramsList = new List<ExprNode>();

            while (true)
            {
                var expression = ParseExpression();
                paramsList.Add(expression);
                if (_currentLexeme != Token.COMMA)
                    break;
                _currentLexeme = _lexer.GetLexeme();
            }

            return paramsList;
        }

        public ExprNode ParseIdent()
        {
            var lexeme = _currentLexeme;
            if (!(_currentLexeme == TokenType.Identifier ||
                _currentLexeme == TokenType.Keyword))
                throw CreateException(
                    $"Identifier expected but '{lexeme.Source}' found");

            _currentLexeme = _lexer.GetLexeme();
            return new IdentNode(lexeme);
        }
    }
}