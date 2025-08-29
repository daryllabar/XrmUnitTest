using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IdGenerator;
public class IdFieldInfo
{
    public string? StructName { get; set; } // null if not in a struct
    public string FieldName { get; set; }
    public string IdType { get; set; }

    public IdFieldInfo(string fieldName, string idType, string? structName = null)
    {
        StructName = structName;
        FieldName = fieldName;
        IdType = idType;
    }

    public static List<IdFieldInfo> ParseIdFields(string input)
    {
        var results = new List<IdFieldInfo>();
        var structRegex = new Regex(@"public struct (\w+)\s*\{([^}]*)\}", RegexOptions.Singleline);
        var idRegex = new Regex(@"public static readonly Id<(\w+)> (\w+) = new", RegexOptions.Singleline);

        // Parse structs
        foreach (Match structMatch in structRegex.Matches(input))
        {
            var structName = structMatch.Groups[1].Value;
            var structBody = structMatch.Groups[2].Value;
            foreach (Match idMatch in idRegex.Matches(structBody))
            {
                results.Add(new IdFieldInfo(idMatch.Groups[2].Value, idMatch.Groups[1].Value, structName));
            }
        }

        // Parse Ids outside structs
        var outsideStruct = structRegex.Replace(input, ""); // Remove all structs
        foreach (Match idMatch in idRegex.Matches(outsideStruct))
        {
            results.Add(new IdFieldInfo(idMatch.Groups[2].Value, idMatch.Groups[1].Value));
        }

        return results;
    }
}
