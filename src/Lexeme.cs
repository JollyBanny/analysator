using System.Globalization;
using System.Text.RegularExpressions;
using LexicalAnalyzer.Enums;

namespace LexicalAnalyzer
{
    class Lexeme
    {
        private Position pos;
        public Position Pos { get => pos; }
        private TokenType type;
        public TokenType Type { get => type; }
        private object value;
        public object Value { get => value; }
        private string source;
        public string Source { get => source; }

        public Lexeme(Position pos, TokenType type, Token token, string source)
        {
            this.pos = pos;
            this.type = type;
            this.value = LexemeValue(type, token, source);
            this.source = source;
        }

        private object LexemeValue(TokenType type, Token token, string source) =>
        type switch
        {
            TokenType.Integer => StringToInteger(source),
            TokenType.Double => StringToDouble(source),
            TokenType.String => NormalizeString(source),
            TokenType.Char => NormalizeChar(source),
            TokenType.Identifire => source,
            TokenType.EOF => Token.EOF,
            _ => token,
        };

        private string NormalizeString(string source)
        {
            var controlStrings = Regex.Matches(source, "[0-9]+")
                                      .Select((x) => x.ToString());
            foreach (var cs in controlStrings)
                source = source.Replace(cs, NormalizeChar(cs).ToString());
            return source.Replace("#", "").Replace("'", "");
        }

        private char NormalizeChar(string source) =>
            (char)int.Parse(source.Trim('#'));

        private int DigitValue(char digit) => digit switch
        {
            char ch when '0' <= ch && ch <= '9' => (int)(ch - '0'),
            char ch when 'a' <= char.ToLower(ch) && char.ToLower(ch) <= 'f' =>
                (int)(char.ToLower(ch) - 'a' + 10),
            _ => -1
        };

        private object StringToInteger(string source)
        {
            int baseNotation = source[0] switch
            {
                '%' => 2,
                '&' => 8,
                '$' => 16,
                _ => 10
            };
            Int64 result = 0;
            for (int i = baseNotation == 10 ? 0 : 1; i < source.Length; i++)
            {
                result = result * baseNotation + DigitValue(source[i]); ;
                if (result > Int32.MaxValue)
                    throw new OverflowException(pos);
            }
            return result;
        }

        private object StringToDouble(string source)
        {
            int baseNotation = source[0] switch
            {
                '%' => 2,
                '&' => 8,
                '$' => 16,
                _ => 10
            };
            if (baseNotation != 10)
                source = StringToInteger(source.Split('.')[0]).ToString()! +
                            source.Split('.')[1];

            if (double.TryParse(source, NumberStyles.Float,
                            CultureInfo.InvariantCulture, out double result))
                return result;
            throw new OverflowException(pos);
        }

        override public string ToString()
        {

            object value_ = type switch
            {
                TokenType.Double => ((double)value).ToStringPascal(),
                TokenType.Operator or TokenType.Keyword or TokenType.Separator =>
                    value.ToString()!.Capitalize(),
                _ => value,
            };

            return $"{pos.line} \t {pos.ch} \t {type} \t {value_} \t {source}";
        }
    }
}