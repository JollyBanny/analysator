using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.SyntaxAnalyzer
{
    public partial class Parser
    {
        private List<SyntaxNode> ParseDecls()
        {
            var delcsList = new List<SyntaxNode>();

            while (true)
            {
                switch (_currentLexeme.Value)
                {
                    case Token.CONST:
                        delcsList.Add(ParseConstDecls());
                        break;
                    case Token.VAR:
                        delcsList.Add(ParseVarDecls());
                        break;
                    case Token.TYPE:
                        delcsList.Add(ParseTypeDecls());
                        break;
                    case Token.FUNCTION:
                    case Token.PROCEDURE:
                        delcsList.Add(ParseCallDecl());
                        break;
                    default:
                        return delcsList;
                }
            }
        }

        private DeclsPartNode ParseConstDecls()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var constDecls = new List<SyntaxNode>();

            Require<TokenType>(false, TokenType.Identifier);

            while (_currentLexeme == TokenType.Identifier)
            {
                var consts = ParseConsts();
                constDecls.Add(consts);
            }

            return new ConstDeclsPartNode(lexeme, constDecls);
        }

        private SyntaxNode ParseConsts()
        {
            Require<TokenType>(false, TokenType.Identifier);

            var constIdent = ParseIdent();

            TypeNode? type = null;

            if (_currentLexeme == Token.COLON)
            {
                NextLexeme();
                type = ParseType();
            }

            Require<Token>(true, Token.EQUAL);

            ExprNode expression = ParseExpression();

            Require<Token>(true, Token.SEMICOLOM);

            return new ConstDeclNode(constIdent, type, expression);
        }

        private DeclsPartNode ParseVarDecls()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var varDecls = new List<SyntaxNode>();

            Require<TokenType>(false, TokenType.Identifier);

            while (_currentLexeme == TokenType.Identifier)
            {
                var vars = ParseVars();
                varDecls.Add(vars);
            }

            return new VarDeclsPartNode(lexeme, varDecls);
        }

        private SyntaxNode ParseVars()
        {
            Require<TokenType>(false, TokenType.Identifier);

            var varIdents = ParseIdentsList();

            Require<Token>(true, Token.COLON);

            var type = ParseType();

            ExprNode? expression = null;
            if (_currentLexeme == Token.EQUAL)
            {
                if (varIdents.Count > 1)
                    throw FatalException("Only one var can be initalized");
                NextLexeme();
                expression = ParseExpression();
            }

            Require<Token>(true, Token.SEMICOLOM);

            return new VarDeclNode(varIdents, type, expression);
        }

        private DeclsPartNode ParseTypeDecls()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var typeDecls = new List<SyntaxNode>();

            Require<TokenType>(false, TokenType.Identifier);

            while (_currentLexeme == TokenType.Identifier)
            {
                var vars = ParseTypes();
                typeDecls.Add(vars);
            }

            return new TypeDeclsPartNode(lexeme, typeDecls);
        }

        private SyntaxNode ParseTypes()
        {
            Require<TokenType>(false, TokenType.Identifier);

            var typeIdent = ParseIdent();

            Require<Token>(true, Token.EQUAL);

            var type = ParseType();

            Require<Token>(true, Token.SEMICOLOM);

            return new TypeDeclNode(typeIdent, type);
        }

        private SyntaxNode ParseCallDecl()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var header = ParseHeader(lexeme == Token.FUNCTION);

            Require<Token>(true, Token.SEMICOLOM);

            var block = ParseSubroutineBlock();

            Require<Token>(true, Token.SEMICOLOM);

            return new CallDeclNode(lexeme, header, block);
        }

        private CallHeaderNode ParseHeader(bool isFunc)
        {
            var callIdent = ParseIdent();

            var paramsList = new List<FormalParamNode>();

            Require<Token>(true, Token.LPAREN);

            if (_currentLexeme != Token.RPAREN)
                paramsList = ParseFormalParamsList();

            Require<Token>(true, Token.RPAREN);

            if (!isFunc)
                return new CallHeaderNode(callIdent, paramsList);

            Require<Token>(true, Token.COLON);

            var returnType = ParseSimpleType();

            return new CallHeaderNode(callIdent, paramsList, returnType);
        }

        private List<FormalParamNode> ParseFormalParamsList()
        {
            var paramsList = new List<FormalParamNode>();

            while (true)
            {
                var param = ParseFormalParam();
                paramsList.Add(param);

                if (_currentLexeme != Token.SEMICOLOM)
                    break;
                NextLexeme();
            }

            return paramsList;
        }

        private FormalParamNode ParseFormalParam()
        {
            SyntaxNode? modifier = _currentLexeme.Value switch
            {
                Token.VAR or Token.CONST or Token.OUT => ParseKeywordNode(),
                _ => null
            };

            var identsList = ParseIdentsList();

            Require<Token>(true, Token.COLON);

            var paramType = ParseParamsType();

            return new FormalParamNode(identsList, paramType, modifier);
        }

        private TypeNode ParseParamsType()
        {
            return _currentLexeme.Type switch
            {
                TokenType.Keyword when _currentLexeme == Token.ARRAY =>
                    ParseParamArrayType(),
                TokenType.Keyword when _currentLexeme == Token.STRING =>
                    ParseSimpleType(),
                TokenType.Identifier =>
                    ParseSimpleType(),
                _ =>
                    throw ExpectedException("variable type", _currentLexeme.Source),
            };
        }

        private SubroutineBlockNode? ParseSubroutineBlock()
        {
            if (_currentLexeme != Token.FORWARD)
                return new SubroutineBlockNode(ParseDecls(), ParseCompoundStmt());

            NextLexeme();
            return null;
        }

        private SyntaxNode ParseKeywordNode()
        {
            var lexeme = _currentLexeme;
            NextLexeme();
            return new KeywordNode(lexeme);
        }
    }
}