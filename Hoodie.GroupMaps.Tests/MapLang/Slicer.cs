using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hoodie.GroupMaps.Tests.MapLang
{
    public static class Slicer
    {
        public static IEnumerable<string[]> Slice(string input)
        {
            var matches = Regex
                .Matches(input, @"^(?: +([\w\.\|_\#\^\<\>\=\*]+))+", RegexOptions.Multiline);

            return matches
                .SelectMany((m, y) => m.Groups[1].Captures.Select((c, x) => (x, y, c.Value)))
                .GroupBy(t => t.x, t => t.Value)
                .Select(g => g
                    .Select(r => Regex.Replace(r, @"[_\.\|]", ""))
                    .ToArray());
        }
        
    }
}