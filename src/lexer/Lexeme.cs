using PascalCompiler.Enums;
using PascalCompiler.Exceptions;
using PascalCompiler.Extensions;

using System.Globalization;
using System.Text;

namespace PascalCompiler.Lexer
{
    class Lexeme
    {
        private Position _pos;
        private TokenType _type;
        private object _value;
        private string _source;

        public Lexeme(Position pos, TokenType type, Token token, string source)
        {
            _pos = pos;
            _type = type;
            _value = LexemeValue(type, token, source);
            _source = source;
        }

        public Position Pos { get => _pos; }
        public TokenType Type { get => _type; }
        public object Value { get => _value; }
        public string Source { get => _source; }

        public T CastValue<T>() => (T)Value;

        private object LexemeValue(TokenType type, Token token, string source) =>
        type switch
        {
            TokenType.Integer => StringToInteger(source),
            TokenType.Double => StringToDouble(source),
            TokenType.String => NormalizeString(source.ToCharArray()),
            TokenType.Char => NormalizeChar(source),
            TokenType.Identifire => source,
            _ => token,
        };

        private char NormalizeChar(string source) => (char)int.Parse(source.Trim('#'));

        private string NormalizeString(char[] source)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; ;)
            {
                if (source.Length == 2)
                    return "";
                if (source[i] == '\'')
                {
                    ++i;
                    string strPart = "";
                    while (source[i] != '\'' || source[i + 1] == '\'')
                    {
                        if (source[i] == '\'' && source[i + 1] == '\'')
                            strPart += source[i++];
                        strPart += source[i++];
                        if (i + 1 >= source.Length)
                            break;
                    }
                    result.Append(strPart.Replace("''", "'"));
                    if (++i >= source.Length)
                        return result.ToString();
                }

                while (source[i] == '#')
                {
                    string specialChar = "";
                    while (source[++i].IsDigit())
                    {
                        specialChar += source[i];
                        if (i + 1 >= source.Length)
                            break;
                    }
                    result.Append(NormalizeChar(specialChar));
                }
                if (source[i] != '\'')
                    break;
            }
            return result.ToString();
        }

        private void GetBase(char ch, out int @base) =>
            @base = ch == '%' ? 2 : ch == '&' ? 8 : ch == '$' ? 16 : 10;

        private object StringToInteger(string source)
        {
            GetBase(source[0], out int @base);
            Int64 result = 0;
            for (int i = @base == 10 ? 0 : 1; i < source.Length; i++)
            {
                result = result * @base + source[i].DigitValue();
                if (result > Int32.MaxValue)
                    throw new LexemeOverflowException(_pos);
            }
            return result;
        }

        private object StringToDouble(string source)
        {
            double result = 0;
            GetBase(source[0], out int @base);

            if (@base != 10)
            {
                string[] splitDouble = source.Split('.');
                for (int i = 1; i < splitDouble[0].Length; i++)
                    result = result * @base + splitDouble[0][i].DigitValue();
                source = result.ToString() + splitDouble[1];
            }

            if (double.TryParse(source, NumberStyles.Float,
                            CultureInfo.InvariantCulture, out result))
                return result;
            throw new LexemeOverflowException(_pos);
        }

        override public string ToString()
        {
            object value = _type switch
            {
                TokenType.Double => ((double)_value).ToStringPascal(),
                TokenType.Operator or TokenType.Keyword or TokenType.Separator =>
                    _value.ToString()!.Capitalize(),
                TokenType.String or TokenType.Char => _value.ToString()!
                    .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t"),
                _ => _value,
            };
            return $"{Pos.Line}\t{Pos.Ch}\t{Type}" +
                    (Type == TokenType.EOF ? "" : $"\t{value}\t{Source}");
        }
    }
}