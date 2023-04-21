namespace Gamebot.Helper;

public static class TeamsHelper
{
    private static readonly Dictionary<string, List<string>> _teamAbbreviations =
        new(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Natus Vincere",
                new List<string>() { "NAVI", "Na'Vi" }
            },
            {
                "Cloud 9",
                new List<string>() { "C9" }
            },
            {
                "G2 Esports",
                new List<string>() { "G2" }
            },
            {
                "Team Vitality",
                new List<string>() { "VIT" }
            },
            {
                "Heroic",
                new List<string>() { "HEROIC" }
            },
            {
                "BIG",
                new List<string>() { "BIG" }
            },
            {
                "FURIA",
                new List<string>() { "FURIA" }
            },
            {
                "Evil Geniuses",
                new List<string>() { "EG" }
            },
            {
                "Team Liquid",
                new List<string>() { "LIQUID", "TL", "TLQ" }
            },
            {
                "OG",
                new List<string>() { "OG" }
            },
            {
                "ENCE",
                new List<string>() { "ENCE" }
            },
            {
                "FaZe Clan",
                new List<string>() { "FAZE" }
            },
            {
                "Ninjas in Pyjamas",
                new List<string>() { "NIP" }
            },
            {
                "Astralis",
                new List<string>() { "AST" }
            },
            {
                "Fnatic",
                new List<string>() { "FNATIC", "FNC" }
            },
            {
                "Virtus.pro",
                new List<string>() { "VP", "VPR" }
            },
            {
                "Team Spirit",
                new List<string>() { "SPIRIT" }
            },
            {
                "forZe",
                new List<string>() { "FORZE" }
            },
            {
                "AGO",
                new List<string>() { "AGO" }
            },
            {
                "EXTREMUM",
                new List<string>() { "EXTREMUM" }
            },
            {
                "HAVU",
                new List<string>() { "HAVU" }
            },
            {
                "Sprout",
                new List<string>() { "SPROUT" }
            }
        };

    public static string GetFullNameFromTeamCode(string name)
    {
        string fullname = string.Empty;

        var comparer = StringComparison.CurrentCultureIgnoreCase;

        if (_teamAbbreviations.TryGetValue(name, out var nameKey))
            return _teamAbbreviations.Keys.FirstOrDefault(key => key.Equals(name, comparer));

        var fullName = _teamAbbreviations
            .FirstOrDefault(x => x.Value.Contains(name, StringComparer.InvariantCultureIgnoreCase))
            .Key;

        //if (!string.IsNullOrEmpty(fullName))
        //    return fullName;

        return fullname;
    }
}
