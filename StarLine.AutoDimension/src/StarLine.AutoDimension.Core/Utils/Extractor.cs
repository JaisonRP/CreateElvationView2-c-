using System.Text.RegularExpressions;

namespace StarLine.AutoDimension.Core.Utils
{
    internal static class Extractor
    {
        private const string Pattern = "\\d+$";

        public static int ExtractIndex(string v)
        {
            var regex = new Regex(Pattern);
            var match = regex.Match(v);
            if (match.Success && int.TryParse(match.Value, out var i))
            {
                return i;
            }

            return 0;
        }
    }
}
