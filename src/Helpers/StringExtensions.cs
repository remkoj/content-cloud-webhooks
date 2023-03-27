using System.Text.RegularExpressions;

namespace DeaneBarker.Optimizely.Webhooks
{
    public static class StringExtensions
    {
        public static string Quoted(this string input)
        {
            return string.Concat("\"", input, "\"");
        }

        public static string TrimEnd(this string input, string remove)
        {
            if (string.IsNullOrEmpty(remove))
                return input;

            if (input.EndsWith(remove) && input != remove)
                input = input[..^remove.Length];

            return input;
        }

        public static string PascalCaseToSpaced(this string input)
        {
            var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            return r.Replace(input, " ");
        }
    }
}