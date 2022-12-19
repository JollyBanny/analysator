using PascalCompiler.Enums;
using PascalCompiler.Exceptions;
using PascalCompiler.Extensions;
using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.Semantics;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.SyntaxAnalyzer
{
    public partial class Parser
    {
        private Lexer _lexer;
        private Lexeme _currentLexeme;
        private SymTable _symTable = new SymTable();
        private SymTableStack _symStack = new SymTableStack();

        public Parser()
        {
            _lexer = new Lexer();
            _currentLexeme = _lexer.GetLexeme();
        }

        public Parser(string path)
        {
            _lexer = new Lexer(path);
            _currentLexeme = _lexer.GetLexeme();
        }

        public SyntaxNode Parse()
        {
            try
            {
                return ParseProgram();
            }
            catch (SemanticException e)
            {
                throw new SemanticException(_lexer.Cursor, e.Message);
            }
        }

        private void NextLexeme() =>
            _currentLexeme = _lexer.GetLexeme();

        private void Require<T>(bool getNext, params T[] tokens)
        {
            foreach (var token in tokens)
                if (_currentLexeme.Equals(token))
                {
                    if (getNext)
                        NextLexeme();
                    return;
                }
            if (tokens[0] is Token)
            {
                Token tok = (Token)(object)tokens[0]!;
                throw ExpectedException(tok.Stringify()!, _currentLexeme.Source);
            }

            throw ExpectedException(tokens[0]!.ToString()!, _currentLexeme.Source);
        }

        private SymType GetSymType(TypeNode typeNode)
        {
            SymType? symType;

            switch (typeNode)
            {
                case RecordTypeNode type:
                    var table = new SymTable();
                    foreach (var field in type.FieldsList)
                    {
                        symType = GetSymType(field.Type);
                        foreach (var ident in field.IdentsList)
                            table.Add(ident.Lexeme.ToString()!, symType);
                    }

                    return new SymRecordType(table);

                case ArrayTypeNode type:
                    var elemType = GetSymType(type.Type);
                    var ranges = new List<Tuple<ExprNode, ExprNode>>();

                    foreach (var range in type.Ranges)
                    {
                        var range_ = new Tuple<ExprNode, ExprNode>(range.LeftBound, range.RightBound);
                        ranges.Add(range_);
                    }

                    return new SymArrayType(ranges, elemType);

                default:
                    var typeName = typeNode.Lexeme.Value.ToString()!;
                    symType = _symStack.FindType(typeName);

                    if (symType is not null)
                        return symType;
                    else
                        throw new SemanticException($"type '{typeName}' not found");
            }
        }

        private SyntaxException ExpectedException(string expected, string found) =>
            new SyntaxException(_lexer.Cursor,
                        $"'{expected}' expected but '{found}' found");

        private SyntaxException FatalException(string msg) =>
            new SyntaxException(_lexer.Cursor, msg);

        public void ChangeFile(string path)
        {
            _lexer.ChangeFile(path);
            _currentLexeme = _lexer.GetLexeme();
        }
    }
}