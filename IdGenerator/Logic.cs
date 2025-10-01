using PluralizeService.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdGenerator
{
    public class Logic
    {
        private int _nameChar;
        private readonly IdGeneratorSettings _settings;
        private readonly IGuidGenerator _guidGenerator;

        public Logic(IdGeneratorSettings settings, IGuidGenerator guidGenerator)
        {
            _settings = settings;
            _guidGenerator = guidGenerator;
        }

        public Dictionary<string, IdInfo> ParseEntityTypes(string entitiesText)
        {
            _nameChar = 'a';
            var parts = entitiesText.Split([" ", Environment.NewLine], StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());
            var idsByType = new Dictionary<string, IdInfo>();
            IdInfo? previousId = null;
            foreach (var part in parts)
            {
                if (int.TryParse(part, out var intValue))
                {
                    ProcessEntityTypeCount(previousId, intValue);
                    continue;
                }
                previousId = ParseEntityType(part, idsByType);
            }

            RenameDuplicateValues(idsByType);
            OrderNames(idsByType);
            return idsByType;
        }

        public string GenerateOutput(IEnumerable<IdInfo> ids)
        {
            var output = new List<string>();
            
            if (_settings.UseClassIds)
            {
                GenerateClassOutput(ids, output);
            }
            else
            {
                GenerateStructOutput(ids, output);
            }
            
            return string.Join(Environment.NewLine, output);
        }

        private void GenerateClassOutput(IEnumerable<IdInfo> ids, List<string> output)
        {
            var classDefinitions = new List<string>();
            foreach (var id in ids.OrderBy(i => i.ContainerName))
            {
                var isMultiple = id.Names.Count > 1;
                if (isMultiple)
                {
                    var propertyName = id.ContainerName;
                    var className = PluralizationProvider.Singularize(id.ContainerName!) + "Ids";
                    output.Add($"public {className} {propertyName} {{ get; }} = new();");

                    // Generate nested class
                    classDefinitions.Add("");
                    classDefinitions.Add($"public class {className}");
                    classDefinitions.Add("{");
                        
                    foreach (var name in id.Names)
                    {
                        var newStatement = _settings.UseTargetTypedNew ? "new" : $"new Id<{id.EntityType}>";
                        classDefinitions.Add($"    public Id<{id.EntityType}> {name} {{ get; }} = {newStatement}(\"{_guidGenerator.Create().ToString().ToUpper()}\");");
                    }

                    classDefinitions.Add("}");
                }
                else
                {
                    // Single ID - generate as a property
                    var name = id.Names[0];
                    var newStatement = _settings.UseTargetTypedNew ? "new" : $"new Id<{id.EntityType}>";
                    output.Add($"public Id<{id.EntityType}> {name} {{ get; }} = {newStatement}(\"{_guidGenerator.Create().ToString().ToUpper()}\");");
                }
            }
            output.AddRange(classDefinitions);
        }

        private void GenerateStructOutput(IEnumerable<IdInfo> ids, List<string> output)
        {
            foreach (var id in ids)
            {
                var isMultiple = id.Names.Count > 1;
                if (isMultiple)
                {
                    output.Add($"public struct {id.ContainerName}");
                    output.Add("{");
                }
                foreach (var name in id.Names)
                {
                    var newStatement = _settings.UseTargetTypedNew ? "new" : $"new Id<{id.EntityType}>";
                    var prefix = isMultiple ? "    " : string.Empty;
                    output.Add($"{prefix}public static readonly Id<{id.EntityType}> {name} = {newStatement}(\"{_guidGenerator.Create().ToString().ToUpper()}\");");
                }

                if (isMultiple)
                {
                    output.Add("}");
                }
            }
        }

        private void ProcessEntityTypeCount(IdInfo? previousId, int intValue)
        {
            if (previousId == null)
            {
                throw new Exception(@"Unable to determine type for count: " + intValue);
            }

            if (previousId.ContainerName == null && previousId.NameIsNameOrStructName)
            {
                previousId.ContainerName = previousId.Names[0];
                previousId.NameIsNameOrStructName = false;
                previousId.PreviousDefinedNames = 0;
            }

            if (previousId.PreviousDefinedNames == 0)
            {
                // Clear default name for single instance
                previousId.Names.RemoveAt(previousId.Names.Count - 1);
            }
            else
            {
                intValue -= previousId.PreviousDefinedNames;
            }

            for (var j = intValue; j > 0; j--)
            {
                previousId.Names.Add(IntToBase(previousId.NextAutoGeneratedIndex++));
            }

            if (string.IsNullOrWhiteSpace(previousId.ContainerName))
            {
                previousId.ContainerName = PluralizationProvider.Pluralize(GenerateNameFromEntityType(previousId.EntityType));
            }
        }

        private IdInfo ParseEntityType(string value, Dictionary<string, IdInfo> idsByType)
        {
            var definedNameParts = value.Split(',');
            var entityType = definedNameParts[0];
            if (!idsByType.TryGetValue(entityType, out var id))
            {
                id = new IdInfo();
                idsByType[entityType] = id;
            }
            var previousId = id;
            id.EntityType = entityType;

            switch (definedNameParts.Length)
            {
                case > 2:
                    id.ContainerName = definedNameParts[1];
                    id.Names.AddRange(definedNameParts.Skip(2));
                    id.PreviousDefinedNames = definedNameParts.Length - 2;
                    break;
                case 2:
                    id.NameIsNameOrStructName = true;
                    id.Names.Add(definedNameParts[1]);
                    id.PreviousDefinedNames = 1;
                    break;
                default:
                    if (id.Names.Count == 1 && id.ContainerName == null)
                    {
                        id.ContainerName = PluralizationProvider.Pluralize(GenerateNameFromEntityType(id.EntityType));
                        id.Names.Clear();
                        id.Names.Add(IntToBase(id.NextAutoGeneratedIndex++));
                        id.Names.Add(IntToBase(id.NextAutoGeneratedIndex++));
                        previousId.PreviousDefinedNames = 1;
                    }
                    else if (id.Names.Count > 1)
                    {
                        id.Names.Add(IntToBase(id.NextAutoGeneratedIndex++));
                        previousId.PreviousDefinedNames = 1;
                    }
                    else
                    { 
                        id.Names.Add(GenerateNameFromEntityType(entityType));
                        previousId.PreviousDefinedNames = 0;
                    }
                    break;
            }

            return previousId;
        }

        private static void RenameDuplicateValues(Dictionary<string, IdInfo> idsByType)
        {
            foreach (var id in idsByType.Values)
            {
                var names = new HashSet<string>();
                foreach (var name in id.Names)
                {
                    var localName = name;
                    while (names.Contains(localName))
                    {
                        localName = IntToBase(id.NextAutoGeneratedIndex++);
                    }
                    names.Add(localName);
                }
                id.Names = [.. names];
            }
        }

        private void OrderNames(Dictionary<string, IdInfo> idsByType)
        {
            foreach(var id in idsByType.Values)
            {
                id.Names = id.Names.OrderBy(n => n).ToList();
            }
        }


        private static readonly char[] BaseChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        public static string IntToBase(int value)
        {
            var targetBase = BaseChars.Length;
            // Determine exact number of characters to use.
            var buffer = new char[Math.Max((int)Math.Ceiling(Math.Log(value + 1, targetBase)), 1)];

            var i = (long)buffer.Length;
            do
            {
                buffer[--i] = BaseChars[value % targetBase];
                value /= targetBase;
            }
            while (value > 0);

            if (buffer.Length > 0)
            {
                buffer[0] = buffer[0].ToString().ToUpper()[0];
            }
            return new string(buffer);
        }

        private string GenerateNameFromEntityType(string name)
        {
            // Generate name
            name = name.Trim();
            name = name == "Entity"
                ? ((char)_nameChar++).ToString()
                : name;
            if (name.Contains("_"))
            {
                name = string.Join(string.Empty, name.Split('_').Skip(1).ToArray());
            }
            return name.ToUpper()[0] + name.Remove(0, 1);
        }

        public string CreateEntitiesText(List<IdFieldInfo> results)
        {
            var output = (from @group in results.GroupBy(r => r.ContainerName ?? r.IdType).OrderBy(g => g.Key)
                          let containerName = @group.First().ContainerName ?? string.Empty
                          // If the struct name ends with "Ids", convert it back to plural form
                          let actualStructName = containerName.EndsWith("Ids") && containerName.Length > 3
                              ? PluralizationProvider.Pluralize(containerName.Substring(0, containerName.Length - 3))
                              : containerName
                          let names = (string.IsNullOrWhiteSpace(actualStructName)
                              ? ","
                              : "," + actualStructName + ",") + string.Join(",", @group.Select(g => g.FieldName))
                          select @group.First().IdType + names).ToList();
            return string.Join(Environment.NewLine, output);
        }
    }
}
