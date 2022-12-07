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
            Token.MUL, Token.O_DIV, Token.O_SHL, Token.O_SHR,
            Token.AND, Token.DIV, Token.SHL, Token.SHR, Token.AS,
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
                left = new BinOperNode(lexeme, left, ParseExpression());
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
                case TokenType.Identifier:
                    return ParseVarReference();
                case TokenType.Integer:
                    return ParseConstIntegerLiteral();
                case TokenType.Double:
                    return ParseConstDoubleLiteral();
                case TokenType.Char:
                    return ParseConstCharLiteral();
                case TokenType.String:
                    return ParseConstStringLiteral();
                case TokenType.Separator when lexeme == Token.LPAREN:
                    _currentLexeme = _lexer.GetLexeme();
                    var exp = ParseRelExpression();
                    if (_currentLexeme != Token.RPAREN)
                        throw ExpectedException(")", _currentLexeme.Source);

                    _currentLexeme = _lexer.GetLexeme();
                    return exp;
                default:
                    throw FatalException("Illegal expression");
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
                        throw ExpectedException("indexes expected", "no indexes");

                    left = new ArrayAccessNode(left, @params);

                    if (_currentLexeme != Token.RBRACK)
                        throw ExpectedException("]", _currentLexeme.Source);
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
                    if (left is not IdentNode)
                        throw ExpectedException("Function name", left.Lexeme.Source);

                    _currentLexeme = _lexer.GetLexeme();
                    List<ExprNode> args = new List<ExprNode>();
                    if (_currentLexeme != Token.RPAREN)
                        args = ParseParamsList();

                    if (_currentLexeme != Token.RPAREN)
                        throw ExpectedException(")", _currentLexeme.Source);
                    else
                        _currentLexeme = _lexer.GetLexeme();

                    var identName = left.Lexeme.Value.ToString()!;

                    if (Token.WRITE.ToString() == identName.ToUpper())
                        left = new WriteCallNode((IdentNode)left, args, false);
                    else if (Token.WRITELN.ToString() == identName.ToUpper())
                        left = new WriteCallNode((IdentNode)left, args, true);
                    else
                        left = new FunctionCallNode((IdentNode)left, args);

                    lexeme = _currentLexeme;
                }
                else
                    return left;
            }
        }

        private List<ExprNode> ParseParamsList()
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

        private ExprNode ParseIdent()
        {
            var lexeme = _currentLexeme;
            if (_currentLexeme != TokenType.Identifier)
                throw ExpectedException("Identifier", lexeme.Source);

            _currentLexeme = _lexer.GetLexeme();
            return new IdentNode(lexeme);
        }

        public List<IdentNode> ParseIdentsList()
        {
            var idents = new List<IdentNode>();

            while (true)
            {
                var ident = ParseIdent() as IdentNode;
                idents.Add(ident!);
                if (_currentLexeme != Token.COMMA)
                    break;
                _currentLexeme = _lexer.GetLexeme();
            }

            return idents;
        }

        private ExprNode ParseConstIntegerLiteral()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();
            return new ConstIntegerLiteral(lexeme);
        }

        private ExprNode ParseConstDoubleLiteral()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();
            return new ConstDoubleLiteral(lexeme);
        }

        private ExprNode ParseConstStringLiteral()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();
            return new ConstStringLiteral(lexeme);
        }

        private ExprNode ParseConstCharLiteral()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();
            return new ConstCharLiteral(lexeme);
        }
    }
}