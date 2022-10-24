using LexicalAnalyzer.utils;

namespace LexicalAnalyzer
{
    public enum Token
    {
        EOF = 1,
        COMMENT,
        INVALID,

        literal_begin,
        IDENTIFIRE,
        T_INT,
        T_FLOAT,
        T_STRING,
        literal_end,

        operator_begin,
        ASSIGN,
        ADD,
        SUB,
        MUL,
        DIV_REAL,
        ADD_ASSIGN,
        SUB_ASSIGN,
        MUL_ASSIGN,
        DIV_ASSIGN,
        MOD_ASSIGN,

        EQUAL,
        LESS,
        MORE,

        NOT_EQUAL,
        LESS_EQUAL,
        MORE_EQUAL,
        operator_end,

        separator_begin,
        LPAREN,
        RPAREN,
        LBRACK,
        RBRACK,

        COMMA,
        DOT,
        ELLIPSIS,
        SEMICOLOM,
        COLON,
        separator_end,

        keyword_begin,
        ARRAY,
        BEGIN,
        BREAK,
        CASE,
        CONST,
        CONTINUE,
        DEC,
        DO,
        DOWNTO,
        ELSE,
        END,
        FOR,
        GOTO,
        IF,
        IN,
        INC,
        OF,
        PROGRAM,
        READ,
        READLN,
        REPEAT,
        THEN,
        TO,
        VAR,
        WHILE,
        WRITE,
        WRITELN,

        AND,
        DIV,
        NOT,
        MOD,
        OR,
        XOR,
        SHL,
        SHR,

        INTEGER,
        REAL,
        STRING,
        keyword_end,
    }

    class Lexeme
    {
        private Position pos;
        public Position Pos { get => pos; }
        private string type;
        public string Type { get => type; }
        private object value;
        public object Value { get => value; }
        private string source;
        public string Source { get => source; }

        public Lexeme(Position pos, Token token, string source)
        {
            this.pos = pos;
            this.source = source;

            this.value = LexemeValue(token, source);
            this.type = LexemeType(token);
        }

        public void GetInfo()
        {
            Console.WriteLine($"{pos.line} \t {pos.ch} \t {type.Capitalize()} \t {value} \t {source}");
        }

        static public Token LookupKeyword(string keyword)
        {
            for (Token i = Token.keyword_begin; i < Token.keyword_end; ++i)
                if (i.ToString() == keyword.ToUpper())
                    return i;

            return Token.IDENTIFIRE;
        }

        private string LexemeType(Token token)
        {
            if (isLiteral(token))
            {
                if (token.ToString().StartsWith("T_"))
                    return token.ToString().Substring(2);
                return token.ToString();
            }
            else if (isOperator(token)) return "OPERATOR";
            else if (isSeparator(token)) return "SEPARATOR";
            else if (isKeyword(token)) return "KEYWORD";
            else return token.ToString();
        }

        private object LexemeValue(Token token, string source)
        {
            if (isLiteral(token))
                return token switch
                {
                    Token.T_INT => TryParseNumber(source, "integer"),
                    Token.T_FLOAT => TryParseNumber(source, "float"),
                    Token.T_STRING => source.Substring(1, source.Length - 2),
                    _ => source
                };
            else
                return token.ToString();
        }

        private object TryParseNumber(string source, string type)
        {
            switch (type)
            {
                case "integer":
                    if (int.TryParse(source, out var int_tmp)) return int_tmp;
                    throw new LexemException(pos, "Integer overflow");
                case "float":
                    if (float.TryParse(source, out var float_tmp)) return float_tmp;
                    throw new LexemException(pos, "Real overflow");
                default: return "undefined";
            }
        }

        private bool isLiteral(Token token) =>
            Token.literal_begin < token && token < Token.literal_end;

        private bool isOperator(Token token) =>
            Token.operator_begin < token && token < Token.operator_end;

        private bool isSeparator(Token token) =>
            Token.separator_begin < token && token < Token.separator_end;

        private bool isKeyword(Token token) =>
            Token.keyword_begin < token && token < Token.keyword_end;
    }
}