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
        private string buffer = "";
        private char currentChar;

        private Position lexemePos = new Position();
        private Token lexemeToken = Token.INVALID;

        public Lexer(StreamReader fstream) => this.fstream = fstream;

        private string ScanIdentifier()
        {
            while (isLetter((char)fstream.Peek()) || isDigit((char)fstream.Peek()))
                WriteToBuffer();
            return buffer;
        }

        private string ScanString()
        {
            while (fstream.Peek() != '\'')
            {
                if (fstream.Peek() == '\n' || fstream.Peek() < 0)
                    throw new LexemeException(lexemePos, "String exceeds line");
                WriteToBuffer();
            }
            WriteToBuffer();
            return buffer;
        }

        private void ScanNumber()
        {
            string number = "";
            int notation = currentChar switch
            {
                '%' => 2,
                '&' => 8,
                '$' => 16,
                _ => 10
            };

            // integer part
            lexemeToken = Token.L_INTEGER;
            if (notation == 10) number += currentChar;
            number += digits(notation);

            if (number.Length == 0)
                throw new LexemeException(lexemePos, "Invalid integer expression");

            // fractional part
            if ((char)fstream.Peek() == '.')
            {
                if (notation != 10)
                    throw new LexemeException(lexemePos, "Invalid number");
                lexemeToken = Token.L_DOUBLE;
                number += (Next() + digits(notation));
            }

            // exponent part
            if ((char)fstream.Peek() == 'e' || (char)fstream.Peek() == 'E')
            {
                if (notation != 10)
                    throw new LexemeException(lexemePos,
                    "Exponent can't be used without decimal mantissa");
                lexemeToken = Token.L_DOUBLE;
                number += Next();

                if ((char)fstream.Peek() == '-' || (char)fstream.Peek() == '+')
                    number += Next();

                string exponentDigits = digits(notation);
                number += exponentDigits;

                if (exponentDigits.Length == 0)
                    throw new LexemeException(lexemePos, "Exponent has no digits");
            }
            // If base is 10 overwrite full number to buffer,
            // else base char in buffer + full number.
            if (notation == 10) buffer = number;
            else buffer += number;
        }

        // ScanComment starts from last character of open comment sequence
        private void ScanComment()
        {
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
            else return;
        }

        private bool isSpace(char ch) =>
            ch == ' ' || ch == '\r' || ch == '\n' || ch == '\t';

        private bool isLetter(char ch) =>
            'a' <= char.ToLower(ch) && char.ToLower(ch) <= 'z' || ch == '_';

        private bool isDigit(char ch) => '0' <= ch && ch <= '9';

        private bool isHex(char ch) =>
            isDigit(ch) || 'a' <= char.ToLower(ch) && char.ToLower(ch) <= 'f';

        private int digitValue(char digit) => digit switch
        {
            char ch when '0' <= ch && ch <= '9' => (int)(ch - '0'),
            char ch when 'a' <= char.ToLower(ch) && char.ToLower(ch) <= 'f' =>
                (int)(char.ToLower(ch) - 'a' + 10),
            _ => -1
        };

        private string digits(int notation)
        {
            string digitSequence = "";
            switch (notation)
            {
                case int x when x <= 10:
                    while (isDigit((char)fstream.Peek()))
                    {
                        if (Next() >= '0' + notation)
                            throw new LexemeException(lexemePos,
                                "The digit exceeds maximum of the base");
                        digitSequence += currentChar;
                    }
                    return digitSequence;
                default:
                    while (isHex((char)fstream.Peek()))
                        digitSequence += Next();
                    return digitSequence;
            }
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


        private void skipWhitespace()
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
            skipWhitespace();

            if (fstream.EndOfStream)
                return new Lexeme(lexemePos, Token.EOF, Token.EOF.ToString());

            buffer = "" + Next();
            lexemePos = cursor;

            switch (currentChar)
            {
                case '\'':
                    lexemeToken = Token.L_STRING;
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
                    {
                        lexemeToken = Token.COMMENT;
                        ScanComment();
                    }
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
                    lexemeToken = Token.COMMENT;
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
                    {
                        lexemeToken = Token.COMMENT;
                        ScanComment();
                    }
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
                    if (buffer.Length > 1) lexemeToken = Lexeme.LookupKeyword(buffer);
                    else lexemeToken = Token.IDENTIFIRE;
                    break;
                case char ch when isDigit(ch) || ch == '%' || ch == '&' || ch == '$':
                    ScanNumber();
                    break;
                default:
                    throw new LexemeException(lexemePos, "Invalid token");
            }

            if (lexemeToken == Token.COMMENT) return GetLexem();

            return new Lexeme(lexemePos, lexemeToken, buffer);
        }
    }
}