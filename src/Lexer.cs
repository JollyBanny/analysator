using LexicalAnalyzer.Enums;

namespace LexicalAnalyzer
{
    struct Position
    {
        public int line = 1;
        public int ch = 0;

        public Position() { }
    }

    class Lexer
    {
        private StreamReader fstream;
        private Position cursor = new Position();
        private string buffer;
        private char currentChar;
        private Position lexemePos;
        private TokenType lexemeType;
        private Token lexemeToken;

        public Lexer(StreamReader fstream)
        {
            this.fstream = fstream;
            buffer = "";
        }

        private bool isSpace(char ch) =>
             ch == ' ' || ch == '\r' || ch == '\n' || ch == '\t';

        private bool isLetter(char ch) =>
            'a' <= char.ToLower(ch) && char.ToLower(ch) <= 'z' || ch == '_';

        private bool isDigit(char ch) => '0' <= ch && ch <= '9';

        private bool isHex(char ch) =>
            isDigit(ch) || 'a' <= char.ToLower(ch) && char.ToLower(ch) <= 'f';

        private string digits(int baseNotation)
        {
            string digitSequence = "";
            if (baseNotation <= 10)
                while (isDigit((char)fstream.Peek()))
                {
                    if (fstream.Peek() >= '0' + baseNotation)
                        return digitSequence;
                    digitSequence += Next();
                }
            else
                while (isHex((char)fstream.Peek()))
                    digitSequence += Next();
            return digitSequence;
        }

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

        private void ScanIdentifier()
        {
            while (isLetter((char)fstream.Peek()) || isDigit((char)fstream.Peek()))
                WriteToBuffer();
        }

        private void ScanString()
        {
            (lexemeType, lexemeToken) = (TokenType.String, Token.L_STRING);
            while (true)
            {
                while (!TryNext('\''))
                {
                    if (fstream.Peek() == '\n' || fstream.Peek() < 0)
                        throw new LexemeException(lexemePos, "String exceeds line");
                    WriteToBuffer();
                }
                while (fstream.Peek() == '#')
                    ScanChar();
                if (!TryNext('\''))
                    return;
            }
        }

        private void ScanChar()
        {
            while (TryNext('#') || currentChar == '#')
            {
                string digitChar = digits(10);
                buffer += digitChar.Length > 0 ? digitChar :
                    throw new LexemeException(lexemePos, "Illegal char constant");
            }
            (lexemeType, lexemeToken) = buffer.Count((c) => c == '#') > 1 ?
                                        (TokenType.String, Token.L_STRING) :
                                        (TokenType.Char, Token.L_CHAR);
            if (TryNext('\'')) ScanString();
        }

        private void ScanNumber()
        {
            int baseNotation = currentChar switch
            {
                '%' => 2,
                '&' => 8,
                '$' => 16,
                _ => 10
            };

            // integer part
            (lexemeType, lexemeToken) = (TokenType.Integer, Token.L_INTEGER);
            buffer += digits(baseNotation);


            if (buffer.Length == 1 && baseNotation != 10)
                throw new LexemeException(lexemePos, "Invalid integer expression");

            // fractional part
            string fractionalDigits = "";
            if ((char)fstream.Peek() == '.')
            {
                (lexemeType, lexemeToken) = (TokenType.Double, Token.L_DOUBLE);
                WriteToBuffer();
                if (baseNotation == 10)
                    fractionalDigits = digits(10);
                buffer += fractionalDigits;
            }

            // exponent part
            string exponentDigits = "";
            if (char.ToLower((char)fstream.Peek()) == 'e' &&
                (lexemeToken == Token.L_DOUBLE || baseNotation == 10))
            {
                (lexemeType, lexemeToken) = (TokenType.Double, Token.L_DOUBLE);
                WriteToBuffer();

                if ((char)fstream.Peek() == '-' || (char)fstream.Peek() == '+')
                    WriteToBuffer();
                exponentDigits = digits(10);
                buffer += exponentDigits;

                if (exponentDigits.Length == 0)
                    if (buffer.Contains('.') && fractionalDigits.Length == 0)
                        throw new LexemeException(lexemePos,
                                    "Illegal floating point constant");
                    else
                        throw new LexemeException(lexemePos,
                                    $"Illegal character '{(char)fstream.Peek()}'");
            }
        }

        // ScanComment starts from last character of open comment sequence
        private void ScanComment()
        {
            (lexemeType, lexemeToken) = (TokenType.Comment, Token.COMMENT);
            // { comment style }
            if (currentChar == '{')
            {
                while (fstream.Peek() != '}')
                {
                    if (fstream.Peek() < 0)
                        throw new LexemeException(lexemePos, "Unexpected end of file");
                    WriteToBuffer();
                }
                WriteToBuffer();
                return;
            }
            // comment style
            else if (currentChar == '/')
            {
                while (fstream.Peek() != '\n')
                    WriteToBuffer();
                return;
            }
            // (* comment style *)
            else if (currentChar == '*')
            {
                while (currentChar != '*' || fstream.Peek() != ')')
                {
                    if (fstream.Peek() < 0)
                        throw new LexemeException(lexemePos, "Unexpected end of file");
                    WriteToBuffer();
                }
                WriteToBuffer();
                return;
            }
        }

        /*
        - Next takes the next character from the stream and returns it.
        - TryNext write to buffer and returns 'true' if the next character
        matches with 'ch', otherwise just returns 'false'.
        - WriteToBuffer just write to buffer next character.
        */
        private char Next()
        {
            currentChar = (char)fstream.Read();
            cursor.line = (currentChar == '\n') ? ++cursor.line : cursor.line;
            cursor.ch = (currentChar == '\n' || currentChar == '\r') ? 0 : ++cursor.ch;
            return currentChar;
        }

        private bool TryNext(char ch)
        {
            if (fstream.Peek() != ch)
                return false;
            WriteToBuffer();
            return true;
        }

        private void WriteToBuffer() =>
            buffer += Next();


        private void SkipWhitespace()
        {
            while (isSpace((char)fstream.Peek()))
                Next();
        }

        public void ChangeFile(StreamReader fstream)
        {
            this.fstream = fstream;
            lexemePos = cursor = new Position();
        }

        public Lexeme GetLexem()
        {
            SkipWhitespace();

            if (fstream.EndOfStream)
                return new Lexeme(cursor, TokenType.EOF, Token.EOF, Token.EOF.ToString());

            buffer = "" + Next();
            lexemePos = cursor;
            lexemeToken = Token.INVALID;
            lexemeType = TokenType.Invalid;

            switch (currentChar)
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
                    lexemeToken = switch2(Token.DIV_REAL, Token.DIV_ASSIGN);
                    if (TryNext('/'))
                        ScanComment();
                    break;
                case '=':
                    lexemeToken = Token.EQUAL;
                    break;
                case '<':
                    lexemeToken = switch3(Token.LESS, Token.LESS_EQUAL, '>', Token.NOT_EQUAL);
                    if (TryNext('<')) lexemeToken = Token.O_SHL;
                    break;
                case '>':
                    lexemeToken = switch3(Token.MORE, Token.MORE_EQUAL, '>', Token.O_SHR);
                    break;
                case char ch when isLetter(ch):
                    ScanIdentifier();
                    var keyword = LookupKeyword(buffer);
                    (lexemeToken, lexemeType) = keyword.HasValue ?
                                    ((Token)keyword, TokenType.Keyword) :
                                    (Token.IDENTIFIRE, TokenType.Identifire);
                    break;
                case char ch when isDigit(ch) || ch == '%' || ch == '&' || ch == '$':
                    ScanNumber();
                    break;
                default:
                    throw new LexemeException(lexemePos, $"Illegal character '{currentChar}'");
            }

            // if (lexemeToken == Token.COMMENT) return GetLexem();
            lexemeType = lexemeToken switch
            {
                Token t when Token.operator_begin < t && t < Token.operator_end =>
                    TokenType.Operator,
                Token t when Token.separator_begin < t && t < Token.separator_end =>
                    TokenType.Separator,
                _ => lexemeType
            };

            return new Lexeme(lexemePos, lexemeType, lexemeToken, buffer);
        }
    }
}