using PascalCompiler.Enums;
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

            var header = lexeme == Token.FUNCTION ? ParseFuncHeader() :
                                                    ParseProcHeader();

            Require<Token>(true, Token.SEMICOLOM);

            var block = ParseSubroutineBlock();

            Require<Token>(true, Token.SEMICOLOM);

            return new CallDeclNode(lexeme, header, block);
        }

        private CallHeaderNode ParseFuncHeader()
        {
            var funcName = ParseIdent();
            List<FormalParamNode>? paramsList = null;

            Require<Token>(true, Token.LPAREN);

            if (_currentLexeme != Token.RPAREN)
                paramsList = ParseFormalParamsList();

            Require<Token>(true, Token.RPAREN);
            Require<Token>(true, Token.COLON);

            var resultType = ParseSimpleType();

            return new CallHeaderNode(funcName, paramsList, resultType);
        }

        private CallHeaderNode ParseProcHeader()
        {
            var funcName = ParseIdent();
            List<FormalParamNode>? paramsList = null;

            Require<Token>(true, Token.LPAREN);

            if (_currentLexeme != Token.RPAREN)
                paramsList = ParseFormalParamsList();

            Require<Token>(true, Token.RPAREN);

            return new CallHeaderNode(funcName, paramsList);
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
            SyntaxNode? modifier = null;
            switch (_currentLexeme.Value)
            {
                case Token.VAR:
                case Token.CONST:
                case Token.OUT:
                    modifier = ParseKeywordNode();
                    break;
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

        private SyntaxNode? ParseSubroutineBlock()
        {
            if (_currentLexeme == Token.FORWARD)
            {
                NextLexeme();
                return null;
            }

            var decls = ParseDecls();
            var block = ParseCompoundStmt();

            return new SubroutineBlockNode(decls, block);
        }

        private SyntaxNode ParseKeywordNode()
        {
            var lexeme = _currentLexeme;
            NextLexeme();
            return new KeywordNode(lexeme);
        }
    }
}