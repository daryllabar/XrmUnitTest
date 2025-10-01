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
        
        // Parse struct-based format
        var structRegex = new Regex(@"public struct (\w+)\s*\{([^}]*)\}", RegexOptions.Singleline);
        var structIdRegex = new Regex(@"public static readonly Id<(\w+)> (\w+) = new", RegexOptions.Singleline);
        
        // Parse structs
        foreach (Match structMatch in structRegex.Matches(input))
        {
            var structName = structMatch.Groups[1].Value;
            var structBody = structMatch.Groups[2].Value;
            foreach (Match idMatch in structIdRegex.Matches(structBody))
            {
                results.Add(new IdFieldInfo(idMatch.Groups[2].Value, idMatch.Groups[1].Value, structName));
            }
        }
        
        // Parse class-based format
        var classRegex = new Regex(@"public class (\w+)\s*\{([^}]*)\}", RegexOptions.Singleline);
        var classIdRegex = new Regex(@"public Id<(\w+)> (\w+) \{ get; \} = new", RegexOptions.Singleline);
        
        // Parse classes
        foreach (Match classMatch in classRegex.Matches(input))
        {
            var className = classMatch.Groups[1].Value;
            var classBody = classMatch.Groups[2].Value;
            foreach (Match idMatch in classIdRegex.Matches(classBody))
            {
                results.Add(new IdFieldInfo(idMatch.Groups[2].Value, idMatch.Groups[1].Value, className));
            }
        }

        // Parse Ids outside structs/classes
        var outsideStructOrClass = structRegex.Replace(input, ""); // Remove all structs
        outsideStructOrClass = classRegex.Replace(outsideStructOrClass, ""); // Remove all classes
        
        // Check for struct-based single IDs
        foreach (Match idMatch in structIdRegex.Matches(outsideStructOrClass))
        {
            results.Add(new IdFieldInfo(idMatch.Groups[2].Value, idMatch.Groups[1].Value));
        }
        
        // Check for class-based single IDs
        foreach (Match idMatch in classIdRegex.Matches(outsideStructOrClass))
        {
            results.Add(new IdFieldInfo(idMatch.Groups[2].Value, idMatch.Groups[1].Value));
        }

        return results;
    }
}
