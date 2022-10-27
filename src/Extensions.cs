namespace LexicalAnalyzer.utils
{
    public static class StringExtension
    {
        public static string Capitalize(this string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                throw new ArgumentException("String is mull or empty");
            }
            return string.Concat(s[0].ToString().ToUpper(), s.ToLower().AsSpan(1));
        }
    }
}
