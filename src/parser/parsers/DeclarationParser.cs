using System.Collections;
using PascalCompiler.Enums;
using PascalCompiler.Semantics;
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

            var constName = constIdent.Lexeme.Value.ToString()!;
            _symStack.CheckDuplicate(constName);

            TypeNode? type = null;

            if (_currentLexeme == Token.COLON)
            {
                NextLexeme();
                type = ParseType();
            }

            Require<Token>(true, Token.EQUAL);

            ExprNode expression = ParseExpression();

            _symStack.AddConst(constName, expression.SymType!);

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

            var symType = _symStack.GetSymType(type);

            ExprNode? expression = null;
            if (_currentLexeme == Token.EQUAL)
            {
                if (varIdents.Count > 1)
                    throw FatalException("Only one var can be initalized");
                NextLexeme();
                expression = ParseExpression();
            }

            Require<Token>(true, Token.SEMICOLOM);

            foreach (var ident in varIdents)
            {
                var sym = _symStack.Find(ident.ToString(), true) as SymVar;
                sym!.Type = symType;
            }

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

            var typeName = typeIdent.Lexeme.Value.ToString()!;
            _symStack.CheckDuplicate(typeName);

            Require<Token>(true, Token.EQUAL);

            var type = ParseType();

            Require<Token>(true, Token.SEMICOLOM);

            var symType = _symStack.GetSymType(type);
            _symStack.AddAliasType(typeName, symType);

            return new TypeDeclNode(typeIdent, type);
        }

        private SyntaxNode ParseCallDecl()
        {
            _symStack.Push();

            var lexeme = _currentLexeme;
            NextLexeme();

            var header = lexeme == Token.FUNCTION ? ParseFuncHeader() : ParseProcHeader();

            Require<Token>(true, Token.SEMICOLOM);

            var block = ParseSubroutineBlock(header);

            Require<Token>(true, Token.SEMICOLOM);

            var symCallTable = _symStack.Pop();

            foreach (DictionaryEntry item in symCallTable)
            {
                if (item.Value is SymParameter)
                    header.SymCall!.Params.Add((item.Value as Symbol)!);
                else
                    header.SymCall!.Locals.Add((item.Value as Symbol)!);
            }

            _symStack.Add(header.SymCall!);

            return new CallDeclNode(lexeme, header, block);
        }

        private CallHeaderNode ParseFuncHeader()
        {
            var funcIdent = ParseIdent();
            var funcName = funcIdent.Lexeme.Value.ToString()!;

            _symStack.CheckPreLastScopeDuplicate(funcName);
            _symStack.AddEmptySym(funcName);

            var paramsList = new List<FormalParamNode>();

            Require<Token>(true, Token.LPAREN);

            if (_currentLexeme != Token.RPAREN)
                paramsList = ParseFormalParamsList();

            Require<Token>(true, Token.RPAREN);
            Require<Token>(true, Token.COLON);

            var returnType = ParseSimpleType();
            var symType = _symStack.GetSymType(returnType);

            _symStack.Remove(funcName);
            var SymFunc = new SymFunc(funcName, new SymTable(), new SymTable(), symType, null);

            return new CallHeaderNode(funcIdent, paramsList, SymFunc, returnType);
        }

        private CallHeaderNode ParseProcHeader()
        {
            var procIdent = ParseIdent();
            var procName = procIdent.Lexeme.Value.ToString()!;

            _symStack.CheckPreLastScopeDuplicate(procName);

            var paramsList = new List<FormalParamNode>();

            Require<Token>(true, Token.LPAREN);

            if (_currentLexeme != Token.RPAREN)
                paramsList = ParseFormalParamsList();

            Require<Token>(true, Token.RPAREN);

            var SymProc = new SymProc(procName, new SymTable(), new SymTable(), null);

            return new CallHeaderNode(procIdent, paramsList, SymProc);
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

            var symModifier = modifier is null ? "" : modifier.ToString()!.ToLower();
            var symTypeParam = _symStack.GetSymType(paramType);

            foreach (var ident in identsList)
            {
                _symStack.Remove(ident.ToString());
                _symStack.AddParameter(ident.ToString()!, symTypeParam, symModifier);
            }

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

        private SubroutineBlockNode? ParseSubroutineBlock(CallHeaderNode header)
        {
            if (_currentLexeme == Token.FORWARD)
            {
                NextLexeme();
                return null;
            }

            if (header.Type is not null)
                _symStack.Add(header.SymCall!);

            var decls = ParseDecls();

            if (header.Type is null && !_symStack.Contains(header.Name.ToString()))
                _symStack.Add(header.SymCall!);

            var block = ParseCompoundStmt();
            header.SymCall!.Block = block;

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