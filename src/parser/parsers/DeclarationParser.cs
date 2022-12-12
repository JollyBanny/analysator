using PascalCompiler.Enums;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.SyntaxAnalyzer
{
    public partial class Parser
    {
        public List<SyntaxNode> ParseDecls()
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
                        delcsList.Add(ParseFuncDecl());
                        break;
                    case Token.PROCEDURE:
                        delcsList.Add(ParseProcDecl());
                        break;
                    default:
                        return delcsList;
                }
            }
        }

        public DeclsPartNode ParseConstDecls()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var constDecls = new List<SyntaxNode>();

            Require<TokenType>(new List<TokenType> { TokenType.Identifier }, false);

            while (_currentLexeme == TokenType.Identifier)
            {
                var consts = ParseConsts();
                constDecls.Add(consts);
            }

            return new ConstDeclsPartNode(lexeme, constDecls);
        }

        public SyntaxNode ParseConsts()
        {
            Require<TokenType>(new List<TokenType> { TokenType.Identifier }, false);

            var constIdent = ParseIdent();
            TypeNode? type = null;

            if (_currentLexeme == Token.COLON)
            {
                NextLexeme();
                type = ParseType();
            }

            Require<Token>(new List<Token> { Token.EQUAL }, true);

            ExprNode expression = ParseExpression();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true);

            return new ConstDeclNode(constIdent, type, expression);
        }

        public DeclsPartNode ParseVarDecls()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var varDecls = new List<SyntaxNode>();

            Require<TokenType>(new List<TokenType> { TokenType.Identifier }, false);

            while (_currentLexeme == TokenType.Identifier)
            {
                var vars = ParseVars();
                varDecls.Add(vars);
            }

            return new VarDeclsPartNode(lexeme, varDecls);
        }

        public SyntaxNode ParseVars()
        {
            Require<TokenType>(new List<TokenType> { TokenType.Identifier }, false);

            var varIdents = ParseIdentsList();

            Require<Token>(new List<Token> { Token.COLON }, true);

            var type = ParseType();

            ExprNode? expression = null;
            if (_currentLexeme == Token.EQUAL)
            {
                if (varIdents.Count > 1)
                    throw FatalException("Only one var can be initalized");
                NextLexeme();
                expression = ParseExpression();
            }

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true);

            return new VarDeclNode(varIdents, type, expression);
        }

        public DeclsPartNode ParseTypeDecls()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var typeDecls = new List<SyntaxNode>();

            Require<TokenType>(new List<TokenType> { TokenType.Identifier }, false);

            while (_currentLexeme == TokenType.Identifier)
            {
                var vars = ParseTypes();
                typeDecls.Add(vars);
            }

            return new TypeDeclsPartNode(lexeme, typeDecls);
        }

        public SyntaxNode ParseTypes()
        {
            Require<TokenType>(new List<TokenType> { TokenType.Identifier }, false);

            var typeIdent = ParseIdent();

            Require<Token>(new List<Token> { Token.EQUAL }, true);

            var type = ParseType();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true);

            return new TypeDeclNode(typeIdent, type);
        }

        public SyntaxNode ParseFuncDecl()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var header = ParseFuncHeader();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true);

            var block = ParseSubroutineBlock();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true);

            return new CallDeclNode(lexeme, header, block);
        }

        public SyntaxNode ParseProcDecl()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var header = ParseProcHeader();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true);

            var block = ParseSubroutineBlock();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true);

            return new CallDeclNode(lexeme, header, block);
        }

        public CallHeaderNode ParseFuncHeader()
        {
            var funcName = ParseIdent();
            List<FormalParamNode>? paramsList = null;

            Require<Token>(new List<Token> { Token.LPAREN }, true);

            if (_currentLexeme != Token.RPAREN)
                paramsList = ParseFormalParamsList();

            Require<Token>(new List<Token> { Token.RPAREN }, true);
            Require<Token>(new List<Token> { Token.COLON }, true);

            var resultType = ParseSimpleType();

            return new CallHeaderNode(funcName, paramsList, resultType);
        }

        public CallHeaderNode ParseProcHeader()
        {
            var funcName = ParseIdent();
            List<FormalParamNode>? paramsList = null;

            Require<Token>(new List<Token> { Token.LPAREN }, true);

            if (_currentLexeme != Token.RPAREN)
                paramsList = ParseFormalParamsList();

            Require<Token>(new List<Token> { Token.RPAREN }, true);

            return new CallHeaderNode(funcName, paramsList);
        }

        public List<FormalParamNode> ParseFormalParamsList()
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

        public FormalParamNode ParseFormalParam()
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

            Require<Token>(new List<Token> { Token.COLON }, true);

            var paramType = ParseParamsType();

            return new FormalParamNode(identsList, paramType, modifier);
        }

        public TypeNode ParseParamsType()
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

        public SyntaxNode ParseSubroutineBlock()
        {
            var decls = ParseDecls();
            var block = ParseCompoundStmt();

            return new SubroutineBlockNode(decls, block);
        }

        public SyntaxNode ParseKeywordNode()
        {
            var lexeme = _currentLexeme;
            NextLexeme();
            return new KeywordNode(lexeme);
        }
    }
}