using PascalCompiler.Enums;
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
            NextLexeme();

            return new IdentTypeNode(new IdentNode(lexeme));
        }

        public TypeNode ParseArrayType()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            Require<Token>(new List<Token> { Token.LBRACK }, true, "[");

            var rangesList = ParseSubrangesList();

            Require<Token>(new List<Token> { Token.RBRACK }, true, "]");

            Require<Token>(new List<Token> { Token.OF }, true, $"{Token.OF}");

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
                NextLexeme();
            }

            return rangesList;
        }

        public SubrangeTypeNode ParseSubrange()
        {
            var leftBound = ParseSimpleExpression();
            var lexeme = _currentLexeme;

            Require<Token>(new List<Token> { Token.ELLIPSIS }, true, "..");

            var rightBound = ParseSimpleExpression();
            return new SubrangeTypeNode(lexeme, leftBound, rightBound);
        }

        public TypeNode ParseRecordType()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var fieldsList = ParseRecordFields();

            Require<Token>(new List<Token> { Token.END }, true, $"{Token.END}");

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

                NextLexeme();
            }

            return fieldsList;
        }

        public RecordFieldNode ParseRecordField()
        {
            var identsList = ParseIdentsList();
            var lexeme = _currentLexeme;

            Require<Token>(new List<Token> { Token.COLON }, true, ":");

            var type = ParseType();
            return new RecordFieldNode(lexeme, identsList, type);
        }

        public ParamArrayTypeNode ParseParamArrayType()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            Require<Token>(new List<Token> { Token.OF }, true, $"{Token.OF}");

            var type = ParseIdentType();
            return new ParamArrayTypeNode(lexeme, type);
        }
    }
}