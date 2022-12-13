using PascalCompiler.Enums;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.SyntaxAnalyzer
{
    public partial class Parser
    {
        private TypeNode ParseType()
        {
            var lexeme = _currentLexeme;
            return lexeme.Type switch
            {
                TokenType.Identifier =>
                    ParseSimpleType(),
                TokenType.Keyword when lexeme == Token.STRING =>
                    ParseSimpleType(),
                TokenType.Keyword when lexeme == Token.ARRAY =>
                    ParseArrayType(),
                TokenType.Keyword when lexeme == Token.RECORD =>
                    ParseRecordType(),
                _ => throw ExpectedException("variable type", lexeme.Source)
            };
        }

        private TypeNode ParseSimpleType()
        {
            if (_currentLexeme == Token.STRING)
                return new SimpleTypeNode(ParseKeywordNode());
            else
                return new SimpleTypeNode(ParseIdent());
        }

        private TypeNode ParseArrayType()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            Require<Token>(new List<Token> { Token.LBRACK }, true);

            var rangesList = ParseSubrangesList();

            Require<Token>(new List<Token> { Token.RBRACK }, true);

            Require<Token>(new List<Token> { Token.OF }, true);

            var type = ParseType();

            return new ArrayTypeNode(lexeme, rangesList, type);
        }

        private List<SubrangeTypeNode> ParseSubrangesList()
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

        private SubrangeTypeNode ParseSubrange()
        {
            var leftBound = ParseSimpleExpression();
            var lexeme = _currentLexeme;

            Require<Token>(new List<Token> { Token.ELLIPSIS }, true);

            var rightBound = ParseSimpleExpression();
            return new SubrangeTypeNode(lexeme, leftBound, rightBound);
        }

        private TypeNode ParseRecordType()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var fieldsList = ParseRecordFields();

            Require<Token>(new List<Token> { Token.END }, true);

            return new RecordTypeNode(lexeme, fieldsList);
        }

        private List<RecordFieldNode> ParseRecordFields()
        {
            var fieldsList = new List<RecordFieldNode>();

            while (_currentLexeme == TokenType.Identifier)
            {
                var field = ParseRecordField();
                fieldsList.Add(field);

                if (_currentLexeme != Token.SEMICOLOM)
                {
                    if (_currentLexeme == TokenType.Identifier)
                        Require<Token>(new List<Token> { Token.SEMICOLOM }, false);
                    break;
                }

                NextLexeme();
            }

            return fieldsList;
        }

        private RecordFieldNode ParseRecordField()
        {
            var identsList = ParseIdentsList();
            var lexeme = _currentLexeme;

            Require<Token>(new List<Token> { Token.COLON }, true);

            var type = ParseType();
            return new RecordFieldNode(lexeme, identsList, type);
        }

        private ParamArrayTypeNode ParseParamArrayType()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            Require<Token>(new List<Token> { Token.OF }, true);

            var type = ParseSimpleType();
            return new ParamArrayTypeNode(lexeme, type);
        }
    }
}