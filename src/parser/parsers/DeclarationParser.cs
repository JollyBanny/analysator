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
            _currentLexeme = _lexer.GetLexeme();

            var constDecls = new List<SyntaxNode>();

            while (_currentLexeme == TokenType.Identifier)
            {
                var consts = ParseConsts();
                constDecls.Add(consts);
            }

            return new ConstDeclsPartNode(lexeme, constDecls);
        }

        public SyntaxNode ParseConsts()
        {
            if (_currentLexeme != TokenType.Identifier)
                throw ExpectedException($"{Token.BEGIN}", _currentLexeme.Source);

            var constIdent = ParseIdent();
            TypeNode? type = null;

            if (_currentLexeme == Token.COLON)
            {
                _currentLexeme = _lexer.GetLexeme();
                type = ParseType();
            }

            if (_currentLexeme != Token.EQUAL)
                throw ExpectedException("=", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            ExprNode expression = ParseExpression();

            if (_currentLexeme != Token.SEMICOLOM)
                throw ExpectedException(";", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            return new ConstDeclNode(constIdent, type, expression);
        }

        public DeclsPartNode ParseVarDecls()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();

            var varDecls = new List<SyntaxNode>();

            while (_currentLexeme == TokenType.Identifier)
            {
                var vars = ParseVars();
                varDecls.Add(vars);
            }

            return new VarDeclsPartNode(lexeme, varDecls);
        }

        public SyntaxNode ParseVars()
        {
            if (_currentLexeme != TokenType.Identifier)
                throw ExpectedException($"{Token.BEGIN}", _currentLexeme.Source);

            var varIdents = ParseIdentsList();

            if (_currentLexeme != Token.COLON)
                throw ExpectedException(":", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var type = ParseType();

            if (_currentLexeme != Token.SEMICOLOM)
                throw ExpectedException(";", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            return new VarDeclNode(varIdents, type);
        }

        public DeclsPartNode ParseTypeDecls()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();

            var typeDecls = new List<SyntaxNode>();

            while (_currentLexeme == TokenType.Identifier)
            {
                var vars = ParseTypes();
                typeDecls.Add(vars);
            }

            return new TypeDeclsPartNode(lexeme, typeDecls);
        }

        public SyntaxNode ParseTypes()
        {
            if (_currentLexeme != TokenType.Identifier)
                throw ExpectedException($"{Token.BEGIN}", _currentLexeme.Source);

            var typeIdent = ParseIdent();

            if (_currentLexeme != Token.EQUAL)
                throw ExpectedException("=", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var type = ParseType();

            if (_currentLexeme != Token.SEMICOLOM)
                throw ExpectedException(";", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            return new TypeDeclNode(typeIdent, type);
        }

        public SyntaxNode ParseFuncDecl()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();

            var header = ParseFuncHeader();

            if (_currentLexeme != Token.SEMICOLOM)
                throw ExpectedException(";", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var block = ParseSubroutineBlock();

            if (_currentLexeme != Token.SEMICOLOM)
                throw ExpectedException(";", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            return new CallDeclNode(lexeme, header, block);
        }

        public SyntaxNode ParseProcDecl()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();

            var header = ParseProcHeader();

            if (_currentLexeme != Token.SEMICOLOM)
                throw ExpectedException(";", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var block = ParseSubroutineBlock();

            if (_currentLexeme != Token.SEMICOLOM)
                throw ExpectedException(";", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            return new CallDeclNode(lexeme, header, block);
        }

        public CallHeaderNode ParseFuncHeader()
        {
            var funcName = ParseIdent();
            var paramsList = ParseFormalParamsList();

            if (_currentLexeme != Token.COLON)
                throw ExpectedException(":", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

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
            if (_currentLexeme != Token.LPAREN)
                throw ExpectedException("(", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var paramsList = new List<FormalParamNode>();

            while (true)
            {
                var param = ParseFormalParam();
                paramsList.Add(param);

                if (_currentLexeme != Token.SEMICOLOM)
                    break;
                _currentLexeme = _lexer.GetLexeme();
            }

            if (_currentLexeme != Token.RPAREN)
                throw ExpectedException(")", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

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
                _currentLexeme = _lexer.GetLexeme();

            var identsList = ParseIdentsList();

            if (_currentLexeme != Token.COLON)
                throw ExpectedException(":", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

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