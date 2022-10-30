using System.Globalization;

namespace LexicalAnalyzer
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
}
