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
        private char currentChar;
        private string buffer = "";

        private Position lexemPos = new Position();
        private Token lexemToken = Token.INVALID;
        private string lexemSource = "";

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
                    throw new LexemException(lexemPos, "String exceeds line");
                WriteToBuffer();
            }
            WriteToBuffer();
            return buffer;
        }

        private string ScanNumber()
        {
            return buffer;
        }

        private void ScanComment()
        {
            if (currentChar == '{')
            {
                while (fstream.Peek() != '}')
                {
                    if (fstream.Peek() < 0)
                        throw new LexemException(lexemPos, "Unexpected end of file");
                    WriteToBuffer();
                }
                WriteToBuffer();
                return;
            }
            WriteToBuffer();
            if (currentChar == '/')
            {
                while (fstream.Peek() != '\n')
                    WriteToBuffer();
                return;
            }

            if (currentChar == '*')
            {
                while (currentChar != '*' || fstream.Peek() != ')')
                {
                    if (fstream.Peek() < 0)
                        throw new LexemException(lexemPos, "Unexpected end of file");
                    WriteToBuffer();
                }
                WriteToBuffer();
                return;
            }
        }

        private Token switch2(Token tok0, Token tok1)
        {
            if (fstream.Peek() == '=')
            {
                WriteToBuffer();
                return tok1;
            }
            return tok0;
        }

        private Token switch3(Token tok0, Token tok1, char ch, Token tok2)
        {
            if (fstream.Peek() == '=')
            {
                WriteToBuffer();
                return tok1;
            }
            if (fstream.Peek() == ch)
            {
                WriteToBuffer();
                return tok2;
            }
            return tok0;
        }

        private bool isSpace(char ch) =>
            ch == ' ' || ch == '\r' || ch == '\n' || ch == '\t';

        private bool isLetter(char ch) =>
            'a' <= char.ToLower(ch) && char.ToLower(ch) <= 'z' || ch == '_';

        private bool isDigit(char ch) =>
            '0' <= ch && ch <= '9';

        private bool isHex(char ch) =>
            isDigit(ch) || 'a' <= char.ToLower(ch) && char.ToLower(ch) <= 'f';

        private int digitValue(char digit)
        {
            return digit switch
            {
                char ch when '0' <= ch && ch <= '9' =>
                    (int)(ch - '0'),
                char ch when 'a' <= char.ToLower(ch) && char.ToLower(ch) <= 'f' =>
                    (int)(char.ToLower(ch) - 'a' + 10),
                _ => 16
            };
        }

        private char getNextChar()
        {
            char ch = (char)fstream.Read();
            cursor.line = (ch == '\n') ? ++cursor.line : cursor.line;
            cursor.ch = (ch == '\n' || ch == '\r') ? 0 : ++cursor.ch;
            currentChar = ch;
            return ch;
        }

        private void WriteToBuffer()
        {
            buffer += getNextChar();
        }

        private void skipWhitespace()
        {
            while (isSpace((char)fstream.Peek()))
                getNextChar();
        }

        public void ChangeFile(StreamReader fstream)
        {
            this.fstream = fstream;
            lexemPos = cursor = new Position();
        }

        public Lexeme GetLexem()
        {
            skipWhitespace();

            if (fstream.EndOfStream)
                return new Lexeme(lexemPos, Token.EOF, "");

            buffer = "" + getNextChar();
            lexemPos = cursor;
            lexemToken = Token.INVALID;

            switch (currentChar)
            {
                case '\'':
                    lexemToken = Token.T_STRING;
                    ScanString();
                    break;
                case ';':
                    lexemToken = Token.SEMICOLOM;
                    break;
                case '.':
                    lexemToken = Token.DOT;
                    if (fstream.Peek() == '.')
                    {
                        WriteToBuffer();
                        lexemToken = Token.ELLIPSIS;
                    }
                    break;
                case ',':
                    lexemToken = Token.COMMA;
                    break;
                case '(':
                    lexemToken = Token.LPAREN;
                    if (fstream.Peek() == '*')
                    {
                        lexemToken = Token.COMMENT;
                        ScanComment();
                    }
                    break;
                case ')':
                    lexemToken = Token.RPAREN;
                    break;
                case '[':
                    lexemToken = Token.LBRACK;
                    break;
                case ']':
                    lexemToken = Token.RBRACK;
                    break;
                case '{':
                    lexemToken = Token.COMMENT;
                    ScanComment();
                    break;
                case ':':
                    lexemToken = switch2(Token.COLON, Token.ASSIGN);
                    break;
                case '+':
                    lexemToken = switch2(Token.ADD, Token.ADD_ASSIGN);
                    break;
                case '-':
                    lexemToken = switch2(Token.SUB, Token.SUB_ASSIGN);
                    break;
                case '*':
                    lexemToken = switch2(Token.MUL, Token.MUL_ASSIGN);
                    break;
                case '/':
                    lexemToken = switch2(Token.DIV_REAL, Token.DIV_ASSIGN);
                    if (fstream.Peek() == '/')
                    {
                        lexemToken = Token.COMMENT;
                        ScanComment();
                    }
                    break;
                case '=':
                    lexemToken = Token.EQUAL;
                    break;
                case '<':
                    lexemToken = switch3(Token.LESS, Token.LESS_EQUAL, '>', Token.NOT_EQUAL);
                    break;
                case '>':
                    lexemToken = switch2(Token.MORE, Token.MORE_EQUAL);
                    break;
                case char ch when isLetter(ch):
                    ScanIdentifier();
                    if (buffer.Length > 1)
                        lexemToken = Lexeme.LookupKeyword(buffer);
                    else
                        lexemToken = Token.IDENTIFIRE;
                    break;
                default:
                    lexemToken = Token.INVALID;
                    break;
            }

            if (lexemToken == Token.COMMENT) return GetLexem();

            return new Lexeme(lexemPos, lexemToken, buffer);
        }
    }
}