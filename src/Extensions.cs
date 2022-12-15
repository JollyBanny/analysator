using System.Globalization;
using PascalCompiler.Enums;
using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.Extensions
{
    public static class DoubleExtension
    {
        public static string ToStringPascal(this double value) =>
            value.ToString("E16", CultureInfo.InvariantCulture);
    }

    public static class StringExtension
    {
        public static string Capitalize(this string str) =>
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
    }

    public static class CharExtension
    {
        public static bool IsSpace(this char ch) =>
            ch == ' ' || ch == '\r' || ch == '\n' || ch == '\t';

        public static bool IsLetter(this char ch) =>
            'a' <= char.ToLower(ch) && char.ToLower(ch) <= 'z' || ch == '_';

        public static bool IsDigit(this char ch) => '0' <= ch && ch <= '9';

        public static bool IsHex(this char ch) =>
            IsDigit(ch) || 'a' <= char.ToLower(ch) && char.ToLower(ch) <= 'f';

        public static int DigitValue(this char digit) => digit switch
        {
            char ch when '0' <= ch && ch <= '9' => (int)(ch - '0'),
            char ch when 'a' <= char.ToLower(ch) && char.ToLower(ch) <= 'f' =>
                (int)(char.ToLower(ch) - 'a' + 10),
            _ => -1
        };
    }

    public static class ListExtension
    {
        public static bool Contains(this List<Token> list, Lexeme toCheck)
        {
            foreach (var item in list)
            {
                if (toCheck == item)
                    return true;
            }
            return false;
        }
    }

    public static class TokenExtension
    {
        public static Dictionary<Token, string> StringTokens = new Dictionary<Token, string>
        {
            {Token.O_DIV, "/"},
            {Token.ADD_ASSIGN, "+="},
            {Token.SUB_ASSIGN, "-="},
            {Token.MUL_ASSIGN, "*="},
            {Token.DIV_ASSIGN, "/="},
            {Token.EQUAL, "="},
            {Token.NOT_EQUAL, "<>"},
            {Token.LESS, "<"},
            {Token.LESS_EQUAL, "<="},
            {Token.MORE, ">"},
            {Token.MORE_EQUAL, ">="},
            {Token.O_SHL, "<<"},
            {Token.O_SHR, ">>"},

            {Token.LPAREN, "("},
            {Token.RPAREN, ")"},
            {Token.LBRACK, "["},
            {Token.RBRACK, "]"},
            {Token.COMMA, ","},
            {Token.DOT, "."},
            {Token.ELLIPSIS, ".."},
            {Token.SEMICOLOM, ";"},
            {Token.COLON, ":"},
        };

        public static string Stringify(this Token token)
        {
            if (StringTokens.Any(st => st.Key == token))
                return StringTokens.First(st => st.Key == token).Value;
            else
                return token.ToString();
        }
    }
}
