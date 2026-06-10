using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace IdGenerator;

public class IdFieldInfo
{
    public string? ContainerName { get; set; }
    public string FieldName { get; set; }
    public string IdType { get; set; }

    public IdFieldInfo(string fieldName, string idType, string? containerName = null)
    {
        ContainerName = containerName;
        FieldName = fieldName;
        IdType = idType;
    }

    private static string GetContainerName(SyntaxNode? node)
    {
        if (node == null)
        {
            return string.Empty;
        }

        var names = new Stack<string>();
        var parent = node;
        while (parent is BaseTypeDeclarationSyntax syntax)
        {
            names.Push(syntax.Identifier.Text);
            parent = syntax.Parent;
        }

        var parts = new List<string>();
        while (names.Count > 0)
        {
            parts.Add(names.Pop());
        }

        return string.Join(".", parts);
    }

    private static bool IsRequestedContainer(SyntaxNode node, string? idContainerName)
    {
        if (string.IsNullOrWhiteSpace(idContainerName))
        {
            return true;
        }

        var containerName = GetContainerName(node);
        return containerName == idContainerName
            || (containerName.Length > idContainerName.Length
                && containerName.StartsWith(idContainerName, StringComparison.Ordinal)
                && containerName[idContainerName.Length] == '.');
    }

    private static string? GetOutputContainerName(SyntaxNode node, string? idContainerName)
    {
        return GetContainerName(node) == idContainerName
            ? null
            : ((BaseTypeDeclarationSyntax)node).Identifier.Text;
    }

    public static (List<IdFieldInfo> IdFieldInfos, List<Diagnostic> Issues) ParseIdFields(string input, string? idContainerName = null)
    {
        var results = new List<IdFieldInfo>();
        var tree = CSharpSyntaxTree.ParseText(input);
        var root = tree.GetRoot();

        foreach (var structDecl in root.DescendantNodes().OfType<StructDeclarationSyntax>())
        {
            if (!IsRequestedContainer(structDecl, idContainerName))
            {
                continue;
            }

            var structName = GetOutputContainerName(structDecl, idContainerName);
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

        foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (!IsRequestedContainer(classDecl, idContainerName))
            {
                continue;
            }

            var className = GetOutputContainerName(classDecl, idContainerName);
            foreach (var member in classDecl.Members)
            {
                if (member is PropertyDeclarationSyntax prop)
                {
                    var typeSyntax = prop.Type as GenericNameSyntax;
                    if (typeSyntax != null && typeSyntax.Identifier.Text == "Id" && typeSyntax.TypeArgumentList.Arguments.Count == 1)
                    {
                        var idType = typeSyntax.TypeArgumentList.Arguments[0].ToString();
                        results.Add(new IdFieldInfo(prop.Identifier.Text, idType, className));
                    }
                }
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

        foreach (var field in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            if (string.IsNullOrWhiteSpace(idContainerName)
                && field.Parent is not ClassDeclarationSyntax
                && field.Parent is not StructDeclarationSyntax)
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
            if (string.IsNullOrWhiteSpace(idContainerName)
                && prop.Parent is not ClassDeclarationSyntax
                && prop.Parent is not StructDeclarationSyntax)
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
