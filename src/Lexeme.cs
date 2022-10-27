using LexicalAnalyzer.utils;
using System.Globalization;

namespace LexicalAnalyzer
{
    public enum Token
    {
        EOF = 1,
        COMMENT,
        INVALID,

        literal_begin,
        IDENTIFIRE,
        L_INTEGER,
        L_DOUBLE,
        L_STRING,
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

        O_SHL,
        O_SHR,
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
        ASM,
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
        EXIT,
        FOR,
        FUNCTION,
        GOTO,
        IF,
        IN,
        INC,
        OF,
        ON,
        PROGRAM,
        PROCEDURE,
        READ,
        READLN,
        REPEAT,
        THEN,
        TO,
        UNTIL,
        VAR,
        WHILE,
        WITH,
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
        FALSE,
        NIL,
        TRUE,
        TRY,

        INTEGER,
        DOUBLE,
        STRING,
        keyword_end,
    }

    // "and", "array", "asm", "begin", "break", "case", "const", "constructor",
    // "continue", "destructor", "dec", "div", "do", "downto", "else", "end",
    // "false", "file", "float", "for", "function", "goto", "if", "implementation",
    // "in", "inc", "inline", "integer", "interface", "label", "mod", "nil", "not",
    // "object", "of", "on", "operator", "or", "packed", "procedure", "program",
    // "real", "record", "repeat", "set", "shl", "shr", "string", "then", "to",
    // "true", "type", "unit", "until", "uses", "var", "while", "with", "xor",
    // "as", "class", "dispose", "except", "exit", "exports", "finalization",
    // "finally", "inherited", "initialization", "is", "library", "new", "on",
    //  "out", "property", "raise", "self", "threadvar", "try"

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
            this.type = LexemeType(token);
            this.value = LexemeValue(token, source);
        }

        private string LexemeType(Token token) => token switch
        {
            // If lexeme token start with "L_" lexeme is literal <type>
            Token t when isLiteral(t) => (t.ToString().StartsWith("L_") ?
                t.ToString().Substring(2) : t.ToString()).Capitalize(),
            Token t when isOperator(t) => "Operator",
            Token t when isSeparator(t) => "Separator",
            Token t when isKeyword(t) => "Keyword",
            Token.EOF => token.ToString(),
            _ => token.ToString().Capitalize()
        };

        private object LexemeValue(Token token, string source) => token switch
        {
            Token t when isLiteral(t) => t switch
            {
                Token.L_INTEGER or Token.L_DOUBLE => NormalizeNumber(source),
                Token.L_STRING => source.Substring(1, source.Length - 2),
                _ => source
            },
            // If lexeme token start with "O_" lexeme is operator alias of keyword
            Token t when isOperator(t) => (t.ToString().StartsWith("O_") ?
                t.ToString().Substring(2) : t.ToString()).ToLower(),
            Token.EOF => token.ToString(),
            _ => token.ToString().Capitalize()
        };

        private object NormalizeNumber(string source)
        {
            if (this.type == "Integer")
            {
                Int64 result = 0;
                string hashTable = "0123456789ABCDEF";
                int notation = source[0] switch
                {
                    '%' => 2,
                    '&' => 8,
                    '$' => 16,
                    _ => 10
                };
                if (notation != 10)
                    source = source.Substring(1);
                foreach (char digit in source)
                {
                    int k = hashTable.IndexOf(digit);
                    result = result * notation + k;
                    if (result > Int32.MaxValue)
                        throw new NumberException(pos);
                }
                return result;
            }
            else
            {
                string source_ = source.Replace('.', ',');
                if (double.TryParse(source_, NumberStyles.Float, null, out double result))
                {
                    return result.ToString("E" + 16, CultureInfo.InvariantCulture);
                }
                throw new NumberException(pos);
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

        static public Token LookupKeyword(string keyword) =>
            Enum.TryParse<Token>(keyword, true, out Token result) ?
                result : Token.IDENTIFIRE;

        public void GetInfo() =>
            Console.WriteLine($"{pos.line} \t {pos.ch} \t {type} \t {value} \t {source}");
    }
}