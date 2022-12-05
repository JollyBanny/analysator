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
}
