using PascalCompiler.Enums;
using PascalCompiler.Exceptions;
using PascalCompiler.Extensions;
using PascalCompiler.Semantics;
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
            Token.MUL, Token.O_DIV, Token.O_SHL, Token.O_SHR, Token.MOD,
            Token.AND, Token.DIV, Token.SHL, Token.SHR, Token.AS,
        };

        private static readonly List<Token> UnaryOperators = new List<Token>
        {
            Token.ADD, Token.SUB, Token.NOT,
        };

        private ExprNode ParseExpression()
        {
            var left = ParseSimpleExpression();
            var lexeme = _currentLexeme;

            if (RelationalOperators.Contains(lexeme))
            {
                NextLexeme();
                left = new BinOperNode(lexeme, left, ParseSimpleExpression());
            }

            return left;
        }

        private ExprNode ParseSimpleExpression()
        {
            var left = ParseTerm();
            var lexeme = _currentLexeme;

            while (AdditionOperators.Contains(lexeme))
            {
                NextLexeme();
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
                NextLexeme();
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
                NextLexeme();
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
                case TokenType.Keyword when lexeme == Token.TRUE || lexeme == Token.FALSE:
                    return ParseConstBooleanLiteral();
                case TokenType.Separator when lexeme == Token.LPAREN:
                    NextLexeme();
                    var exp = ParseExpression();
                    Require<Token>(true, Token.RPAREN);
                    return exp;
                default:
                    throw FatalException("Illegal expression");
            }
        }

        private ExprNode ParseVarReference()
        {
            var left = ParseIdent() as ExprNode;
            var lexeme = _currentLexeme;

            while (true)
            {
                if (lexeme == Token.LBRACK)
                {
                    NextLexeme();

                    left = new ArrayAccessNode(left, ParseParamsList());

                    Require<Token>(true, Token.RBRACK);

                    lexeme = _currentLexeme;
                }
                else if (lexeme == Token.DOT)
                {
                    NextLexeme();
                    left = new RecordAccessNode(left, ParseIdent());
                    lexeme = _currentLexeme;
                }
                else if (lexeme == Token.LPAREN)
                {
                    if (left is not IdentNode)
                        throw ExpectedException("Identifier", left.Lexeme.Source);
                    NextLexeme();

                    List<ExprNode> args = new List<ExprNode>();

                    if (_currentLexeme != Token.RPAREN)
                        args = ParseParamsList();

                    Require<Token>(true, Token.RPAREN);

                    var identName = left.Lexeme.Value.ToString()!.ToUpper();

                    if (Token.WRITE.ToString() == identName)
                        left = new WriteCallNode((IdentNode)left, args, false);
                    else if (Token.WRITELN.ToString() == identName)
                        left = new WriteCallNode((IdentNode)left, args, true);
                    else
                        left = new UserCallNode((IdentNode)left, args);

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
                NextLexeme();
            }

            return paramsList;
        }

        private IdentNode ParseIdent()
        {
            var lexeme = _currentLexeme;

            Require<TokenType>(true, TokenType.Identifier);

            return new IdentNode(lexeme);
        }

        private List<IdentNode> ParseIdentsList()
        {
            var idents = new List<IdentNode>();

            while (true)
            {
                var ident = ParseIdent();
                var identName = ident.Lexeme.Value.ToString()!;

                _symStack.CheckDuplicate(identName);
                _symStack.AddEmptySym(identName);

                idents.Add(ident!);

                if (_currentLexeme != Token.COMMA)
                    break;
                NextLexeme();
            }

            return idents;
        }

        private ConstantNode ParseConstIntegerLiteral()
        {
            var lexeme = _currentLexeme;
            NextLexeme();
            return new ConstIntegerLiteral(lexeme);
        }

        private ConstantNode ParseConstDoubleLiteral()
        {
            var lexeme = _currentLexeme;
            NextLexeme();
            return new ConstDoubleLiteral(lexeme);
        }

        private ConstantNode ParseConstStringLiteral()
        {
            var lexeme = _currentLexeme;
            NextLexeme();
            return new ConstStringLiteral(lexeme);
        }

        private ConstantNode ParseConstCharLiteral()
        {
            var lexeme = _currentLexeme;
            NextLexeme();
            return new ConstCharLiteral(lexeme);
        }

        private ConstantNode ParseConstBooleanLiteral()
        {
            var lexeme = _currentLexeme;
            NextLexeme();
            return new ConstBooleanLiteral(lexeme);
        }
    }
}