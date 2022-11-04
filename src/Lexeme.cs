using System.Globalization;
using LexicalAnalyzer.Enums;
using LexicalAnalyzer.Extensions;
using LexicalAnalyzer.Exceptions;

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

        private char NormalizeChar(string source) => (char)int.Parse(source.Trim('#'));

        private string NormalizeString(string source)
        {
            source = source.Replace("''", $"'#{(int)'\''}'");
            string source_ = source;
            source = source.Replace("'", "");
            bool read = true;
            for (int i = 0; i < source_.Length; ++i)
            {
                if (source_[i] == '\'') read = !read;
                if (source_[i] == '#' && read)
                {
                    string str = $"{source_[i++]}";
                    while (source_[i].IsDigit())
                    {
                        str += source_[i++];
                        if (i >= source_.Length)
                            break;
                    }
                    --i;
                    var index = source.IndexOf(str);
                    source = source.Remove(index, str.Length)
                                   .Insert(index, NormalizeChar(str).ToString());
                }
            }
            return source;
        }

        private void GetBaseNotation(char ch, out int baseNotation) =>
            baseNotation = ch == '%' ? 2 : ch == '&' ? 8 : ch == '$' ? 16 : 10;

        private object StringToInteger(string source)
        {
            GetBaseNotation(source[0], out int baseNotation);
            Int64 result = 0;
            for (int i = baseNotation == 10 ? 0 : 1; i < source.Length; i++)
            {
                result = result * baseNotation + source[i].DigitValue();
                if (result > Int32.MaxValue)
                    throw new LexemeOverflowException(pos);
            }
            return result;
        }

        private object StringToDouble(string source)
        {
            double result = 0;
            GetBaseNotation(source[0], out int baseNotation);

            if (baseNotation != 10)
            {
                string[] splitDouble = source.Split('.');
                for (int i = 1; i < splitDouble[0].Length; i++)
                    result = result * baseNotation + splitDouble[0][i].DigitValue();
                source = result.ToString() + splitDouble[1];
            }

            if (double.TryParse(source, NumberStyles.Float,
                            CultureInfo.InvariantCulture, out result))
                return result;
            throw new LexemeOverflowException(pos);
        }

        override public string ToString()
        {
            object value_ = type switch
            {
                TokenType.Double => ((double)value).ToStringPascal(),
                TokenType.Operator or TokenType.Keyword or TokenType.Separator =>
                    value.ToString()!.Capitalize(),
                TokenType.String or TokenType.Char => value.ToString()!
                    .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t"),
                _ => value,
            };
            return $"{pos.line} \t {pos.ch} \t {type} \t {value_} \t {source}";
        }
    }
}