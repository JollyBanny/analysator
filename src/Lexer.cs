using LexicalAnalyzer.Enums;
using LexicalAnalyzer.Exceptions;
using LexicalAnalyzer.Extensions;

namespace LexicalAnalyzer
{
    class Lexer : LexerBuffer
    {
        private TokenType lexemeType;
        private Token lexemeToken;

        public Lexer(StreamReader fstream) : base(fstream) { }
        public Lexer() : base() { }

        private void GetBaseNotation(char ch, out int baseNotation) =>
            baseNotation = ch == '%' ? 2 : ch == '&' ? 8 : ch == '$' ? 16 : 10;

        private Token? LookupKeyword(string keyword)
        {
            Enum.TryParse<Token>(keyword, true, out Token result);
            return Token.keyword_begin < result && result < Token.keyword_end ?
                result : null;
        }

        /*
        Helpers switch functions work as follows:
        If the token ends with '=', then return tok1,
        otherwise return tok0, or if the token ends with ch, then tok2
        */
        private Token switch2(Token tok0, Token tok1)
        {
            if (TryNext('=')) return tok1;
            return tok0;
        }

        private Token switch3(Token tok0, Token tok1, char ch, Token tok2)
        {
            if (TryNext('=')) return tok1;
            if (TryNext(ch)) return tok2;
            return tok0;
        }

        private string digits(int baseNotation)
        {
            string digitSequence = "";
            if (baseNotation <= 10)
                while (((char)Peek()).IsDigit())
                {
                    if (Peek() >= '0' + baseNotation)
                        return digitSequence;
                    digitSequence += Next();
                }
            else
                while (((char)Peek()).IsHex())
                    digitSequence += Next();
            return digitSequence;
        }

        private void ScanIdentifier()
        {
            while (((char)Peek()).IsLetter() || ((char)Peek()).IsDigit())
                WriteToBuffer();
        }

        private void ScanString()
        {
            (lexemeType, lexemeToken) = (TokenType.String, Token.L_STRING);
            while (true)
            {
                while (!TryNext('\''))
                {
                    if (Peek() == '\n' || Peek() < 0)
                        throw new LexemeException(LexemePos, "String exceeds line");
                    WriteToBuffer();
                }
                if (TryNext('#'))
                    ScanChar();
                if (!TryNext('\''))
                    return;
            }
        }

        private void ScanChar()
        {
            while (CurrentChar == '#')
            {
                string digitChar = digits(10);
                WriteToBuffer(digitChar.Length > 0 ? digitChar :
                    throw new LexemeException(LexemePos, "Illegal char constant"));
                TryNext('#');
            }
            (lexemeType, lexemeToken) = Buffer.Count((c) => c == '#') > 1 ?
                (TokenType.String, Token.L_STRING) : (TokenType.Char, Token.L_CHAR);
            if (TryNext('\''))
                ScanString();
        }

        private void ScanNumber()
        {

            GetBaseNotation(CurrentChar, out int baseNotation);
            // integer part
            (lexemeType, lexemeToken) = (TokenType.Integer, Token.L_INTEGER);
            WriteToBuffer(digits(baseNotation));

            if (Buffer.Length == 1 && baseNotation != 10)
                throw new LexemeException(LexemePos, "Invalid integer expression");

            // fractional part
            string fractionalDigits = "";
            if ((char)Peek() == '.')
            {
                (lexemeType, lexemeToken) = (TokenType.Double, Token.L_DOUBLE);
                WriteToBuffer();
                if (baseNotation == 10)
                    fractionalDigits = digits(10);
                WriteToBuffer(fractionalDigits);
            }

            // exponent part
            string exponentDigits = "";
            if (char.ToLower((char)Peek()) == 'e' &&
                (lexemeToken == Token.L_DOUBLE || baseNotation == 10))
            {
                (lexemeType, lexemeToken) = (TokenType.Double, Token.L_DOUBLE);
                WriteToBuffer();

                if ((char)Peek() == '-' || (char)Peek() == '+')
                    WriteToBuffer();
                exponentDigits = digits(10);
                WriteToBuffer(exponentDigits);

                if (exponentDigits.Length == 0)
                    if (Buffer.Contains('.') && fractionalDigits.Length == 0)
                        throw new LexemeException(LexemePos,
                                "Illegal floating point constant");
                    else
                    {
                        string text = 6 < Peek() && Peek() < 14 ? $"#{Peek()}" :
                            ((char)Peek()).ToString();
                        throw new LexemeException(LexemePos, $"Illegal character '{text}'");
                    }
            }
        }

        // ScanComment starts from last character of open comment sequence
        private void ScanComment()
        {
            (lexemeType, lexemeToken) = (TokenType.Comment, Token.COMMENT);
            // { comment style }
            if (CurrentChar == '{')
            {
                while (Peek() != '}')
                {
                    if (Peek() < 0)
                        throw new LexemeException(LexemePos, "Unexpected end of file");
                    WriteToBuffer();
                }
                WriteToBuffer();
                return;
            }
            // comment style
            else if (CurrentChar == '/')
            {
                while (Peek() != '\n' && Peek() > 0)
                {
                    WriteToBuffer();
                }
                return;
            }
            // (* comment style *)
            else if (CurrentChar == '*')
            {
                while (CurrentChar != '*' || Peek() != ')')
                {
                    if (Peek() < 0)
                        throw new LexemeException(LexemePos, "Unexpected end of file");
                    WriteToBuffer();
                }
                WriteToBuffer();
                return;
            }
        }

        private void SkipWhitespace()
        {
            while (((char)Peek()).IsSpace())
                Next();
        }

        private void SkipCommentAndWhiteSpace()
        {
            while (true)
            {
                if (CurrentChar == '(')
                {
                    if (TryNext('*'))
                        ScanComment();
                    else break;
                }
                else if (CurrentChar == '/')
                {
                    if (TryNext('/'))
                        ScanComment();
                    else break;
                }
                else if (CurrentChar == '{')
                    ScanComment();
                else if (CurrentChar.IsSpace())
                    SkipWhitespace();
                else
                    break;
                WriteToBuffer(null, true);
            }
        }

        public Lexeme GetLexem()
        {
            WriteToBuffer(null, true);
            SkipCommentAndWhiteSpace();
            LexemePos = Cursor;
            (lexemeType, lexemeToken) = (TokenType.Invalid, Token.INVALID);

            switch (CurrentChar)
            {
                case '#':
                    ScanChar();
                    break;
                case '\'':
                    ScanString();
                    break;
                case ';':
                    lexemeToken = Token.SEMICOLOM;
                    break;
                case '.':
                    lexemeToken = Token.DOT;
                    if (TryNext('.')) lexemeToken = Token.ELLIPSIS;
                    break;
                case ',':
                    lexemeToken = Token.COMMA;
                    break;
                case '(':
                    lexemeToken = Token.LPAREN;
                    if (TryNext('*'))
                        ScanComment();
                    break;
                case ')':
                    lexemeToken = Token.RPAREN;
                    break;
                case '[':
                    lexemeToken = Token.LBRACK;
                    break;
                case ']':
                    lexemeToken = Token.RBRACK;
                    break;
                case '{':
                    ScanComment();
                    break;
                case ':':
                    lexemeToken = switch2(Token.COLON, Token.ASSIGN);
                    break;
                case '+':
                    lexemeToken = switch2(Token.ADD, Token.ADD_ASSIGN);
                    break;
                case '-':
                    lexemeToken = switch2(Token.SUB, Token.SUB_ASSIGN);
                    break;
                case '*':
                    lexemeToken = switch2(Token.MUL, Token.MUL_ASSIGN);
                    break;
                case '/':
                    if (TryNext('/')) ScanComment();
                    else
                        lexemeToken = switch2(Token.DIV_REAL, Token.DIV_ASSIGN);
                    break;
                case '=':
                    lexemeToken = Token.EQUAL;
                    break;
                case '<':
                    if (TryNext('<')) lexemeToken = Token.O_SHL;
                    else
                        lexemeToken = switch3(Token.LESS, Token.LESS_EQUAL, '>', Token.NOT_EQUAL);
                    break;
                case '>':
                    lexemeToken = switch3(Token.MORE, Token.MORE_EQUAL, '>', Token.O_SHR);
                    break;
                case char ch when ch.IsLetter():
                    ScanIdentifier();
                    var keyword = LookupKeyword(Buffer);
                    (lexemeType, lexemeToken) = keyword.HasValue ?
                                    (TokenType.Keyword, (Token)keyword) :
                                    (TokenType.Identifire, Token.IDENTIFIRE);
                    break;
                case char ch when ch.IsDigit() || ch == '%' || ch == '&' || ch == '$':
                    ScanNumber();
                    break;
                case { } when EndOfStream:
                    (lexemeType, lexemeToken) = (TokenType.EOF, Token.EOF);
                    WriteToBuffer(Token.EOF.ToString(), true);
                    break;
                default:
                    throw new LexemeException(LexemePos, $"Illegal character '{CurrentChar}'");
            }

            lexemeType = lexemeToken switch
            {
                Token t when Token.operator_begin < t && t < Token.operator_end =>
                    TokenType.Operator,
                Token t when Token.separator_begin < t && t < Token.separator_end =>
                    TokenType.Separator,
                _ => lexemeType
            };

            return new Lexeme(LexemePos, lexemeType, lexemeToken, Buffer);
        }
    }
}