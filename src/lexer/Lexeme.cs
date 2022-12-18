using PascalCompiler.Enums;
using PascalCompiler.Exceptions;
using PascalCompiler.Extensions;

using System.Globalization;
using System.Text;

namespace PascalCompiler.LexicalAnalyzer
{
    public class Lexeme
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

        private object LexemeValue(TokenType type, Token token, string source) =>
        type switch
        {
            TokenType.Integer => StringToInteger(source),
            TokenType.Double => StringToDouble(source),
            TokenType.String => NormalizeString(source.ToCharArray()),
            TokenType.Char => NormalizeChar(source),
            TokenType.Identifier => source,
            _ => token,
        };

        private char NormalizeChar(string source) => (char)int.Parse(source.Trim('#'));

        private string NormalizeString(char[] source)
        {
            var result = new StringBuilder();
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
                    var specialChar = "";
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
                var splitDouble = source.Split('.');
                for (int i = 1; i < splitDouble[0].Length; i++)
                    result = result * @base + splitDouble[0][i].DigitValue();
                source = result.ToString() + splitDouble[1];
            }

            if (double.TryParse(source, NumberStyles.Float,
                            CultureInfo.InvariantCulture, out result))
                return result;
            throw new LexemeOverflowException(_pos);
        }

        public override string ToString()
        {
            var value = _type switch
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

        public static bool operator ==(Lexeme lexeme, Token token)
        {
            try
            {
                return (Token)lexeme.Value == token;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public static bool operator !=(Lexeme lexeme, Token token)
        {
            try
            {
                return (Token)lexeme.Value != token;
            }
            catch (System.Exception)
            {
                return true;
            }
        }

        public static bool operator ==(Lexeme lexeme, TokenType tokenType)
            => lexeme.Type == tokenType;

        public static bool operator !=(Lexeme lexeme, TokenType tokenType)
            => lexeme.Type != tokenType;

        public override bool Equals(Object? obj)
        {
            if (obj is null)
                return false;
            else if (obj is Token)
                return this == (Token)obj;
            else if (obj is TokenType)
                return this == (TokenType)obj;
            else
                return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Pos.GetHashCode();
                hashCode = 32 * hashCode + Type.GetHashCode();
                hashCode = 32 * hashCode + Value.GetHashCode();
                hashCode = 32 * hashCode + Source.GetHashCode();
                return hashCode;
            }
        }
    }
}