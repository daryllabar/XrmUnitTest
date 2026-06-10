using System.Reflection;

namespace IdGeneratorTests;

[TestClass]
public sealed class ProgramTests
{
    private static Type GetProgramType()
    {
        var programType = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(a => a.GetType("IdGenerator.Cli.Program", throwOnError: false))
            .FirstOrDefault(t => t != null);

        if (programType != null)
        {
            return programType;
        }

        var candidateAssemblies = Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll")
            .Where(path =>
            {
                var fileName = Path.GetFileName(path);
                return fileName.Equals("idgen.dll", StringComparison.OrdinalIgnoreCase)
                    || fileName.Contains("IdGenerator.Cli", StringComparison.OrdinalIgnoreCase);
            });

        foreach (var assemblyPath in candidateAssemblies)
        {
            programType = Assembly.LoadFrom(assemblyPath).GetType("IdGenerator.Cli.Program", throwOnError: false);
            if (programType != null)
            {
                return programType;
            }
        }

        throw new InvalidOperationException("Unable to locate IdGenerator.Cli.Program.");
    }

    [TestMethod]
    public void Program_CliOptionsParse_Should_ParseFromCSharpContainerName()
    {
        //
        // Arrange
        //
        var programType = GetProgramType();
        var cliOptionsType = programType.GetNestedType("CliOptions", BindingFlags.NonPublic)!;
        var parse = cliOptionsType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static)!;

        //
        // Act
        //
        var options = parse.Invoke(null, new object[] { new[] { "--from-csharp", "TestExample.TestMethodNameClass.TestIds", "MyTest.cs" } })!;

        //
        // Assert
        //
        Assert.AreEqual("TestExample.TestMethodNameClass.TestIds", cliOptionsType.GetProperty("FromCSharpContainerName")!.GetValue(options));
        Assert.AreEqual("MyTest.cs", cliOptionsType.GetProperty("FromCSharp")!.GetValue(options));
    }

    [TestMethod]
    public async Task Program_Main_Should_ReturnError_When_FromCSharpPathIsMissing()
    {
        //
        // Arrange
        //
        var originalOut = Console.Out;
        var originalError = Console.Error;
        var output = new StringWriter();
        var error = new StringWriter();
        Console.SetOut(output);
        Console.SetError(error);

        try
        {
            var programType = GetProgramType();
            var main = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static)!;

            //
            // Act
            //
            var task = (Task<int>)main.Invoke(null, new object[] { new[] { "--from-csharp", "TestExample.TestMethodNameClass.TestIds" } })!;
            var exitCode = await task;

            //
            // Assert
            //
            Assert.AreEqual(1, exitCode);
            StringAssert.Contains(error.ToString(), "Missing value for --from-csharp path.");
            Assert.AreEqual(string.Empty, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }
}
