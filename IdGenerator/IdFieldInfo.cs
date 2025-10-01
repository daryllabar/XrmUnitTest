using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace IdGenerator;

public class IdFieldInfo
{
    public string? ContainerName { get; set; } // null if not in a struct
    public string FieldName { get; set; }
    public string IdType { get; set; }

    public IdFieldInfo(string fieldName, string idType, string? containerName = null)
    {
        ContainerName = containerName;
        FieldName = fieldName;
        IdType = idType;
    }

    public static (List<IdFieldInfo> IdFieldInfos, List<Diagnostic> Issues) ParseIdFields(string input)
    {
        var results = new List<IdFieldInfo>();
        var tree = CSharpSyntaxTree.ParseText(input);
        var root = tree.GetRoot();

        // Parse structs
        foreach (var structDecl in root.DescendantNodes().OfType<StructDeclarationSyntax>())
        {
            var structName = structDecl.Identifier.Text;
            foreach (var field in structDecl.Members.OfType<FieldDeclarationSyntax>())
            {
                foreach (var variable in field.Declaration.Variables)
                {
                    var typeSyntax = field.Declaration.Type as GenericNameSyntax;
                    if (typeSyntax != null && typeSyntax.Identifier.Text == "Id" && typeSyntax.TypeArgumentList.Arguments.Count == 1)
                    {
                        var idType = typeSyntax.TypeArgumentList.Arguments[0].ToString();
                        results.Add(new IdFieldInfo(variable.Identifier.Text, idType, structName));
                    }
                }
            }
        }

        // Parse classes
        foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            var className = classDecl.Identifier.Text;
            foreach (var member in classDecl.Members)
            {
                // Property: public Id<Contact> A { get; } = new(...);
                if (member is PropertyDeclarationSyntax prop)
                {
                    var typeSyntax = prop.Type as GenericNameSyntax;
                    if (typeSyntax != null && typeSyntax.Identifier.Text == "Id" && typeSyntax.TypeArgumentList.Arguments.Count == 1)
                    {
                        var idType = typeSyntax.TypeArgumentList.Arguments[0].ToString();
                        results.Add(new IdFieldInfo(prop.Identifier.Text, idType, className));
                    }
                }
                // Field: public static readonly Id<Contact> A = new(...);
                else if (member is FieldDeclarationSyntax field)
                {
                    foreach (var variable in field.Declaration.Variables)
                    {
                        var typeSyntax = field.Declaration.Type as GenericNameSyntax;
                        if (typeSyntax != null && typeSyntax.Identifier.Text == "Id" && typeSyntax.TypeArgumentList.Arguments.Count == 1)
                        {
                            var idType = typeSyntax.TypeArgumentList.Arguments[0].ToString();
                            results.Add(new IdFieldInfo(variable.Identifier.Text, idType, className));
                        }
                    }
                }
            }
        }

        // Parse Ids outside structs/classes
        foreach (var field in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            // Only top-level fields (not inside class/struct)
            if (field.Parent is not ClassDeclarationSyntax && field.Parent is not StructDeclarationSyntax)
            {
                foreach (var variable in field.Declaration.Variables)
                {
                    var typeSyntax = field.Declaration.Type as GenericNameSyntax;
                    if (typeSyntax != null && typeSyntax.Identifier.Text == "Id" && typeSyntax.TypeArgumentList.Arguments.Count == 1)
                    {
                        var idType = typeSyntax.TypeArgumentList.Arguments[0].ToString();
                        results.Add(new IdFieldInfo(variable.Identifier.Text, idType));
                    }
                }
            }
        }
        foreach (var prop in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
        {
            if (prop.Parent is not ClassDeclarationSyntax && prop.Parent is not StructDeclarationSyntax)
            {
                var typeSyntax = prop.Type as GenericNameSyntax;
                if (typeSyntax != null && typeSyntax.Identifier.Text == "Id" && typeSyntax.TypeArgumentList.Arguments.Count == 1)
                {
                    var idType = typeSyntax.TypeArgumentList.Arguments[0].ToString();
                    results.Add(new IdFieldInfo(prop.Identifier.Text, idType));
                }
            }
        }

        return (results, tree.GetDiagnostics().Where(c => c.Severity == DiagnosticSeverity.Error).ToList());
    }
}