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

            Require<Token>(true, Token.LBRACK);

            var rangesList = ParseSubrangesList();

            Require<Token>(true, Token.RBRACK);

            Require<Token>(true, Token.OF);

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

            Require<Token>(true, Token.ELLIPSIS);

            var rightBound = ParseSimpleExpression();
            return new SubrangeTypeNode(lexeme, leftBound, rightBound);
        }

        private TypeNode ParseRecordType()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var fieldsList = ParseRecordFields();

            Require<Token>(true, Token.END);

            return new RecordTypeNode(lexeme, fieldsList);
        }

        private List<RecordFieldNode> ParseRecordFields()
        {
            _symStack.Push();

            var fieldsList = new List<RecordFieldNode>();

            while (_currentLexeme == TokenType.Identifier)
            {
                var field = ParseRecordField();
                fieldsList.Add(field);

                if (_currentLexeme != Token.SEMICOLOM)
                {
                    if (_currentLexeme == TokenType.Identifier)
                        Require<Token>(false, Token.SEMICOLOM);
                    break;
                }

                NextLexeme();
            }

            _symStack.Pop();

            return fieldsList;
        }

        private RecordFieldNode ParseRecordField()
        {
            var identsList = ParseIdentsList();
            var lexeme = _currentLexeme;

            Require<Token>(true, Token.COLON);

            var type = ParseType();
            return new RecordFieldNode(lexeme, identsList, type);
        }

        private ConformatArrayTypeNode ParseParamArrayType()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            Require<Token>(true, Token.OF);

            var type = ParseSimpleType();
            return new ConformatArrayTypeNode(lexeme, type);
        }
    }
}