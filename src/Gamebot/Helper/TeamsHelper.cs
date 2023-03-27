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
                "FURIA Esports",
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
                "Gambit Esports",
                new List<string>() { "GAMBIT" }
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
                new List<string>() { "FAZE", "FCN" }
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
                "mousesports",
                new List<string>() { "MOUZ", "MOUSE" }
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
                "Sinners Esports",
                new List<string>() { "SINNERS" }
            },
            {
                "Renegades",
                new List<string>() { "RNG" }
            },
            {
                "Gambit Youngsters",
                new List<string>() { "GMB.YNG" }
            },
            {
                "SKADE",
                new List<string>() { "SKADE" }
            },
            {
                "Entropiq",
                new List<string>() { "ENTROPIQ" }
            },
            {
                "AGO",
                new List<string>() { "AGO" }
            },
            {
                "NASR",
                new List<string>() { "NASR" }
            },
            {
                "Fiend",
                new List<string>() { "FIEND" }
            },
            {
                "EXTREMUM",
                new List<string>() { "EXTREMUM" }
            },
            {
                "Dignitas",
                new List<string>() { "DIG" }
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

        string fullName = _teamAbbreviations
            .FirstOrDefault(x => x.Value.Contains(name, StringComparer.InvariantCulture))
            .Key;
        fullname = fullName;
        return fullname;
    }
}
