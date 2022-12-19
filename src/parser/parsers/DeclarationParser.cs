using System.Collections;
using PascalCompiler.Exceptions;
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
            var identName = constIdent.Lexeme.Value.ToString()!;

            TypeNode? type = null;

            if (_currentLexeme == Token.COLON)
            {
                NextLexeme();
                type = ParseType();
            }

            Require<Token>(true, Token.EQUAL);

            ExprNode expression = ParseExpression();

            _symStack.Add(identName, new SymConstant(identName, expression.SymType));

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

            var symType = GetSymType(type);

            ExprNode? expression = null;
            if (_currentLexeme == Token.EQUAL)
            {
                if (varIdents.Count > 1)
                    throw FatalException("Only one var can be initalized");
                NextLexeme();
                expression = ParseExpression();
            }

            Require<Token>(true, Token.SEMICOLOM);

            var identsTable = _symStack.Pop();

            foreach (DictionaryEntry ident in identsTable)
            {
                var identName = ident.Key.ToString()!;
                _symStack.Add(identName, new SymParameter(identName, symType));
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
            if (_symStack.Contains(typeName))
                throw new SemanticException($"Duplicate identifier {typeName}");

            Require<Token>(true, Token.EQUAL);

            var type = ParseType();

            Require<Token>(true, Token.SEMICOLOM);

            var symType = GetSymType(type);
            _symStack.Add(typeName, new SymAliasType(typeName, symType));

            return new TypeDeclNode(typeIdent, type);
        }

        private SyntaxNode ParseCallDecl()
        {
            _symStack.Push();

            var lexeme = _currentLexeme;
            NextLexeme();

            var header = lexeme == Token.FUNCTION ? ParseFuncHeader() : ParseProcHeader();
            var callName = header.Name.Lexeme.Value.ToString()!;
            var symTypeFunc = lexeme == Token.FUNCTION ? GetSymType(header.Type!) : null;

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
                {
                    var symbol = (item.Value as SymParameter)!;
                    symParams.Add(item.Key.ToString()!, symbol);
                }
                else
                {
                    var symbol = (item.Value as SymVar)!;
                    symLocals.Add(item.Key.ToString()!, symbol);
                }
            }

            if (lexeme == Token.PROCEDURE)
                _symStack.Add(callName, new SymProc(callName, symParams, symLocals));
            else
                _symStack.Add(callName, new SymFunc(callName, symParams, symLocals, symTypeFunc!));

            return new CallDeclNode(lexeme, header, block);
        }

        private CallHeaderNode ParseFuncHeader()
        {
            var funcIdent = ParseIdent();
            var funcName = funcIdent.Lexeme.Value.ToString()!;

            if (_symStack.Contains(funcName))
                throw new SemanticException($"Duplicate identifier {funcName}");
            _symStack.Add(funcName, null!);

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

            if (_symStack.Contains(procName))
                throw new SemanticException($"Duplicate identifier {procName}");
            _symStack.Add(procName, null!);

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

            var identsTable = _symStack.Pop();
            var symModifier = modifier is null ? "" : modifier.ToString()!.ToLower();
            var symTypeParam = GetSymType(paramType);

            foreach (DictionaryEntry ident in identsTable)
            {
                var identName = ident.Key.ToString()!;
                _symStack.Add(identName, new SymParameter(identName, symTypeParam, symModifier));
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