using PascalCompiler.Enums;
using PascalCompiler.Extensions;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.SyntaxAnalyzer
{
    public partial class Parser
    {
        public TypeNode ParseType()
        {
            var lexeme = _currentLexeme;
            return lexeme.Type switch
            {
                TokenType.Identifier =>
                    ParseIdentType(),
                TokenType.Keyword when lexeme == Token.STRING =>
                    ParseIdentType(),
                TokenType.Keyword when lexeme == Token.ARRAY =>
                    ParseArrayType(),
                TokenType.Keyword when lexeme == Token.RECORD =>
                    ParseRecordType(),

                _ => throw ExpectedException("variable type", lexeme.Source)
            };
        }

        public TypeNode ParseIdentType()
        {
            if (_currentLexeme != Token.STRING)
                return new IdentTypeNode(ParseIdent());

            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();
            return new IdentTypeNode(new IdentNode(lexeme));
        }

        public TypeNode ParseArrayType()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();

            if (_currentLexeme != Token.LBRACK)
                throw ExpectedException("[", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var rangesList = ParseSubrangesList();

            if (_currentLexeme != Token.RBRACK)
                throw ExpectedException("]", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            if (_currentLexeme != Token.OF)
                throw ExpectedException($"{Token.OF}", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var type = ParseType();

            return new ArrayTypeNode(lexeme, rangesList, type);
        }

        public List<SubrangeTypeNode> ParseSubrangesList()
        {
            var rangesList = new List<SubrangeTypeNode>();

            while (true)
            {
                var range = ParseSubrange();
                rangesList.Add(range);

                if (_currentLexeme != Token.COMMA)
                    break;

                _currentLexeme = _lexer.GetLexeme();
            }

            return rangesList;
        }

        public SubrangeTypeNode ParseSubrange()
        {
            var leftBound = ParseExpression();
            var lexeme = _currentLexeme;

            if (lexeme == Token.ELLIPSIS)
                _currentLexeme = _lexer.GetLexeme();

            var rightBound = ParseExpression();
            return new SubrangeTypeNode(lexeme, leftBound, rightBound);
        }

        public TypeNode ParseRecordType()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();

            var fieldsList = ParseRecordFields();

            if (_currentLexeme != Token.END)
                throw ExpectedException($"{Token.END}", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            return new RecordTypeNode(lexeme, fieldsList);
        }

        public List<RecordFieldNode> ParseRecordFields()
        {
            var fieldsList = new List<RecordFieldNode>();

            while (_currentLexeme == TokenType.Identifier)
            {
                var field = ParseRecordField();
                fieldsList.Add(field);

                if (_currentLexeme != Token.SEMICOLOM)
                    break;
                _currentLexeme = _lexer.GetLexeme();
            }

            return fieldsList;
        }

        public RecordFieldNode ParseRecordField()
        {
            var identsList = ParseIdentsList();
            var lexeme = _currentLexeme;

            if (lexeme != Token.COLON)
                throw ExpectedException(":", lexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var type = ParseType();
            return new RecordFieldNode(lexeme, identsList, type);
        }
    }
}