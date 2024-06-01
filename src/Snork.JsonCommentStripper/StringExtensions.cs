namespace Snork.JsonCommentStripper
{
    internal static class StringExtensions
    {
        public static string Slice(this string input, int start, int? end = null)
        {
            if (end == null)
                return input.Substring(start);
            return input.Substring(start, end.Value - start);
        }
    }
}