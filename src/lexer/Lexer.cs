using PascalCompiler.Enums;
using PascalCompiler.Exceptions;
using PascalCompiler.Extensions;

namespace PascalCompiler.LexicalAnalyzer
{
    class Lexer : LexerBuffer
    {
        private TokenType _lexemeType;
        private Token _lexemeToken;

        public Lexer() : base() { }
        public Lexer(string path) : base(path) { }

        private void GetBase(char ch, out int @base) =>
            @base = ch == '%' ? 2 : ch == '&' ? 8 : ch == '$' ? 16 : 10;

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
        private Token Switch2(Token tok0, Token tok1) =>
            TryNext('=') ? tok1 : tok0;

        private Token Switch3(Token tok0, Token tok1, char ch, Token tok2) =>
            TryNext('=') ? tok1 : TryNext(ch) ? tok2 : tok0;

        /*
        Digits scans the maximum digit sequence taking into account the base.
        */
        private string Digits(int @base)
        {
            string digitSequence = "";
            if (@base <= 10)
                while (((char)Peek()).IsDigit())
                {
                    if (Peek() >= '0' + @base)
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
            (_lexemeType, _lexemeToken) = (TokenType.String, Token.L_STRING);
            while (true)
            {
                while (!TryNext('\'') || Peek() == '\'')
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
                string digitChar = Digits(10);
                WriteToBuffer(digitChar.Length > 0 ? digitChar :
                    throw new LexemeException(LexemePos, "Illegal char constant"));
                TryNext('#');
            }
            (_lexemeType, _lexemeToken) = Buffer.Count(c => c == '#') > 1 ?
                (TokenType.String, Token.L_STRING) : (TokenType.Char, Token.L_CHAR);
            if (TryNext('\''))
                ScanString();
        }

        private void ScanNumber()
        {
            GetBase(CurrentChar, out int @base);
            // integer part
            (_lexemeType, _lexemeToken) = (TokenType.Integer, Token.L_INTEGER);
            WriteToBuffer(Digits(@base));
            if (Buffer.Length == 1 && @base != 10)
                throw new LexemeException(LexemePos, "Invalid integer expression");

            // fractional part
            string fractionalDigits = "";
            if (TryNext('.'))
            {
                if (Peek() == '.')
                {
                    Back();
                    return;
                }
                (_lexemeType, _lexemeToken) = (TokenType.Double, Token.L_DOUBLE);
                if (@base == 10)
                    fractionalDigits = Digits(10);
                WriteToBuffer(fractionalDigits);
            }

            // exponent part
            string exponentDigits = "";
            if (char.ToLower((char)Peek()) == 'e' &&
                (_lexemeToken == Token.L_DOUBLE || @base == 10))
            {
                (_lexemeType, _lexemeToken) = (TokenType.Double, Token.L_DOUBLE);
                WriteToBuffer();

                if (Peek() == '-' || Peek() == '+')
                    WriteToBuffer();
                exponentDigits = Digits(10);
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
            (_lexemeType, _lexemeToken) = (TokenType.Comment, Token.COMMENT);
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

            switch (CurrentChar)
            {
                case '#':
                    ScanChar();
                    break;
                case '\'':
                    ScanString();
                    break;
                case ';':
                    _lexemeToken = Token.SEMICOLOM;
                    break;
                case '.':
                    _lexemeToken = Token.DOT;
                    if (TryNext('.')) _lexemeToken = Token.ELLIPSIS;
                    break;
                case ',':
                    _lexemeToken = Token.COMMA;
                    break;
                case '(':
                    _lexemeToken = Token.LPAREN;
                    if (TryNext('*')) ScanComment();
                    break;
                case ')':
                    _lexemeToken = Token.RPAREN;
                    break;
                case '[':
                    _lexemeToken = Token.LBRACK;
                    break;
                case ']':
                    _lexemeToken = Token.RBRACK;
                    break;
                case '{':
                    ScanComment();
                    break;
                case ':':
                    _lexemeToken = Switch2(Token.COLON, Token.ASSIGN);
                    break;
                case '+':
                    _lexemeToken = Switch2(Token.ADD, Token.ADD_ASSIGN);
                    break;
                case '-':
                    _lexemeToken = Switch2(Token.SUB, Token.SUB_ASSIGN);
                    break;
                case '*':
                    _lexemeToken = Switch2(Token.MUL, Token.MUL_ASSIGN);
                    break;
                case '/':
                    _lexemeToken = Switch3(Token.O_DIV, Token.DIV_ASSIGN, '/', Token.COMMENT);
                    if (_lexemeToken == Token.COMMENT) ScanComment();
                    break;
                case '=':
                    _lexemeToken = Token.EQUAL;
                    break;
                case '<':
                    if (TryNext('<')) _lexemeToken = Token.O_SHL;
                    else
                        _lexemeToken = Switch3(Token.LESS, Token.LESS_EQUAL, '>', Token.NOT_EQUAL);
                    break;
                case '>':
                    _lexemeToken = Switch3(Token.MORE, Token.MORE_EQUAL, '>', Token.O_SHR);
                    break;
                case char ch when ch.IsLetter():
                    ScanIdentifier();
                    var keyword = LookupKeyword(Buffer);
                    (_lexemeType, _lexemeToken) = keyword.HasValue ?
                                    (TokenType.Keyword, (Token)keyword) :
                                    (TokenType.Identifire, Token.IDENTIFIRE);
                    break;
                case char ch when ch.IsDigit() || ch == '%' || ch == '&' || ch == '$':
                    ScanNumber();
                    break;
                case { } when EndOfStream:
                    (_lexemeType, _lexemeToken) = (TokenType.EOF, Token.EOF);
                    break;
                default:
                    throw new LexemeException(LexemePos, $"Illegal character '{CurrentChar}'");
            }

            _lexemeType = _lexemeToken switch
            {
                > Token.operator_begin and < Token.operator_end => TokenType.Operator,
                > Token.separator_begin and < Token.separator_end => TokenType.Separator,
                _ => _lexemeType
            };

            return new Lexeme(LexemePos, _lexemeType, _lexemeToken, Buffer);
        }
    }
}