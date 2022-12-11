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
                var decl = _currentLexeme.Value switch
                {
                    Token.CONST => ParseConstDecls(),
                    Token.VAR => ParseVarDecls(),
                    Token.TYPE => ParseTypeDecls(),
                    Token.PROCEDURE => ParseProcDecl(),
                    Token.FUNCTION => ParseFuncDecl(),
                    _ => null,
                };

                if (decl is null)
                    break;

                delcsList.Add(decl);
            }

            return delcsList;
        }

        public DeclsPartNode ParseConstDecls()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var constDecls = new List<SyntaxNode>();

            Require<TokenType>(new List<TokenType> { TokenType.Identifier },
                false, $"{TokenType.Identifier}");

            while (_currentLexeme == TokenType.Identifier)
            {
                var consts = ParseConsts();
                constDecls.Add(consts);
            }

            return new ConstDeclsPartNode(lexeme, constDecls);
        }

        public SyntaxNode ParseConsts()
        {
            Require<TokenType>(new List<TokenType> { TokenType.Identifier },
                false, $"{Token.BEGIN}");

            var constIdent = ParseIdent();
            TypeNode? type = null;

            if (_currentLexeme == Token.COLON)
            {
                NextLexeme();
                type = ParseType();
            }

            Require<Token>(new List<Token> { Token.EQUAL }, true, "=");

            ExprNode expression = ParseExpression();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true, ";");

            return new ConstDeclNode(constIdent, type, expression);
        }

        public DeclsPartNode ParseVarDecls()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var varDecls = new List<SyntaxNode>();

            Require<TokenType>(new List<TokenType> { TokenType.Identifier },
                false, $"{TokenType.Identifier}");

            while (_currentLexeme == TokenType.Identifier)
            {
                var vars = ParseVars();
                varDecls.Add(vars);
            }

            return new VarDeclsPartNode(lexeme, varDecls);
        }

        public SyntaxNode ParseVars()
        {
            Require<TokenType>(new List<TokenType> { TokenType.Identifier },
                false, $"{Token.BEGIN}");

            var varIdents = ParseIdentsList();

            Require<Token>(new List<Token> { Token.COLON }, true, ":");

            var type = ParseType();

            ExprNode? expression = null;
            if (_currentLexeme == Token.EQUAL)
            {
                if (varIdents.Count > 1)
                    throw FatalException("Only one var can be initalized");
                NextLexeme();
                expression = ParseExpression();
            }

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true, ";");

            return new VarDeclNode(varIdents, type, expression);
        }

        public DeclsPartNode ParseTypeDecls()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var typeDecls = new List<SyntaxNode>();

            Require<TokenType>(new List<TokenType> { TokenType.Identifier },
                false, $"{TokenType.Identifier}");

            while (_currentLexeme == TokenType.Identifier)
            {
                var vars = ParseTypes();
                typeDecls.Add(vars);
            }

            return new TypeDeclsPartNode(lexeme, typeDecls);
        }

        public SyntaxNode ParseTypes()
        {
            Require<TokenType>(new List<TokenType> { TokenType.Identifier },
                false, $"{Token.BEGIN}");

            var typeIdent = ParseIdent();

            Require<Token>(new List<Token> { Token.EQUAL }, true, "=");

            var type = ParseType();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true, ";");

            return new TypeDeclNode(typeIdent, type);
        }

        public SyntaxNode ParseFuncDecl()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var header = ParseFuncHeader();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true, ";");

            var block = ParseSubroutineBlock();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true, ";");

            return new CallDeclNode(lexeme, header, block);
        }

        public SyntaxNode ParseProcDecl()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var header = ParseProcHeader();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true, ";");

            var block = ParseSubroutineBlock();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true, ";");

            return new CallDeclNode(lexeme, header, block);
        }

        public CallHeaderNode ParseFuncHeader()
        {
            var funcName = ParseIdent();
            var paramsList = ParseFormalParamsList();

            Require<Token>(new List<Token> { Token.COLON }, true, ":");

            var resultType = ParseIdentType();

            return new CallHeaderNode(funcName, paramsList, resultType);
        }

        public CallHeaderNode ParseProcHeader()
        {
            var funcName = ParseIdent();
            var paramsList = ParseFormalParamsList();

            return new CallHeaderNode(funcName, paramsList);
        }

        public List<FormalParamNode> ParseFormalParamsList()
        {
            Require<Token>(new List<Token> { Token.LPAREN }, true, "(");

            var paramsList = new List<FormalParamNode>();

            while (true)
            {
                var param = ParseFormalParam();
                paramsList.Add(param);

                if (_currentLexeme != Token.SEMICOLOM)
                    break;
                NextLexeme();
            }

            Require<Token>(new List<Token> { Token.RPAREN }, true, ")");

            return paramsList;
        }

        public FormalParamNode ParseFormalParam()
        {
            KeywordNode? modifier = null;
            switch (_currentLexeme.Value)
            {
                case Token.VAR:
                case Token.CONST:
                case Token.OUT:
                    modifier = new KeywordNode(_currentLexeme);
                    break;
            };

            if (modifier is not null)
                NextLexeme();

            var identsList = ParseIdentsList();

            Require<Token>(new List<Token> { Token.COLON }, true, ":");

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
                    ParseIdentType(),
                TokenType.Identifier =>
                    ParseIdentType(),
                _ =>
                    throw ExpectedException("variable type", _currentLexeme.Source),
            };
        }

        public SyntaxNode ParseSubroutineBlock()
        {
            var decls = new List<DeclsPartNode>();

            while (true)
            {
                switch (_currentLexeme.Value)
                {
                    case Token.CONST:
                        var constDecls = ParseConstDecls();
                        decls.Add(constDecls);
                        break;
                    case Token.TYPE:
                        var typeDecls = ParseTypeDecls();
                        decls.Add(typeDecls);
                        break;
                    case Token.VAR:
                        var varDecls = ParseVarDecls();
                        decls.Add(varDecls);
                        break;
                    default:
                        return new SubroutineBlockNode(decls, ParseCompoundStmt());
                }
            }
        }
    }
}