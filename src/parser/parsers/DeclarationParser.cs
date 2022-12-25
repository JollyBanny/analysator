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
            _symStack.Push();

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

            var varsTable = _symStack.Pop();

            foreach (DictionaryEntry ident in varsTable)
                _symStack.AddVar(ident.Key.ToString()!, symType);

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
            var callName = header.Name.Lexeme.Value.ToString()!;
            var symType = header.Type is not null ? _symStack.GetSymType(header.Type) : null;

            Require<Token>(true, Token.SEMICOLOM);

            if (lexeme == Token.PROCEDURE)
                _symStack.Remove(callName);

            var block = ParseSubroutineBlock();

            _symStack.Remove(callName);

            Require<Token>(true, Token.SEMICOLOM);

            var symCallTable = _symStack.Pop();
            var symParams = new SymTable();
            var symLocals = new SymTable();

            foreach (DictionaryEntry item in symCallTable)
            {
                if (item.Value is SymParameter)
                    symParams.Add((item.Value as SymParameter)!);
                else
                    symLocals.Add((item.Value as Symbol)!);
            }

            _symStack.AddCall(callName, symParams, symLocals, block?.CompoundStmt, symType);

            return new CallDeclNode(lexeme, header, block);
        }

        private CallHeaderNode ParseFuncHeader()
        {
            var funcIdent = ParseIdent();
            var funcName = funcIdent.Lexeme.Value.ToString()!;

            _symStack.CheckCallNameDuplicate(funcName);
            _symStack.AddEmptySym(funcName);

            var paramsList = new List<FormalParamNode>();

            Require<Token>(true, Token.LPAREN);

            if (_currentLexeme != Token.RPAREN)
                paramsList = ParseFormalParamsList();

            Require<Token>(true, Token.RPAREN);
            Require<Token>(true, Token.COLON);

            var resultType = ParseSimpleType();

            return new CallHeaderNode(funcIdent, paramsList, resultType);
        }

        private CallHeaderNode ParseProcHeader()
        {
            var procIdent = ParseIdent();
            var procName = procIdent.Lexeme.Value.ToString()!;

            _symStack.CheckCallNameDuplicate(procName);
            _symStack.AddEmptySym(procName);

            var paramsList = new List<FormalParamNode>();

            Require<Token>(true, Token.LPAREN);

            if (_currentLexeme != Token.RPAREN)
                paramsList = ParseFormalParamsList();

            Require<Token>(true, Token.RPAREN);

            return new CallHeaderNode(procIdent, paramsList);
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
            _symStack.Push();

            SyntaxNode? modifier = _currentLexeme.Value switch
            {
                Token.VAR or Token.CONST or Token.OUT => ParseKeywordNode(),
                _ => null
            };

            var identsList = ParseIdentsList();

            Require<Token>(true, Token.COLON);

            var paramType = ParseParamsType();

            var paramsTable = _symStack.Pop();
            var symModifier = modifier is null ? "" : modifier.ToString()!.ToLower();
            var symTypeParam = _symStack.GetSymType(paramType);

            foreach (DictionaryEntry ident in paramsTable)
                _symStack.AddParameter(ident.Key.ToString()!, symTypeParam, symModifier);

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