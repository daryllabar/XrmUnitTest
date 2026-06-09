using System.Reflection;

namespace IdGeneratorTests;

[TestClass]
public sealed class ProgramTests
{
    [TestMethod]
    public void Program_CliOptionsParse_Should_ParseFromCSharpContainerName()
    {
        //
        // Arrange
        //
        var programType = Assembly.Load("idgen").GetType("IdGenerator.Cli.Program", throwOnError: true)!;
        var cliOptionsType = programType.GetNestedType("CliOptions", BindingFlags.NonPublic)!;
        var parse = cliOptionsType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static)!;

        //
        // Act
        //
        var options = parse.Invoke(null, new object[] { new[] { "--from-csharp", "MyTest.cs", "TestExample.TestMethodNameClass.TestIds" } })!;

        //
        // Assert
        //
        Assert.AreEqual("MyTest.cs", cliOptionsType.GetProperty("FromCSharp")!.GetValue(options));
        Assert.AreEqual("TestExample.TestMethodNameClass.TestIds", cliOptionsType.GetProperty("FromCSharpContainerName")!.GetValue(options));
    }

    [TestMethod]
    public async Task Program_Main_Should_ReturnError_When_FromCSharpContainerNameIsMissing()
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
            var programType = Assembly.Load("idgen").GetType("IdGenerator.Cli.Program", throwOnError: true)!;
            var main = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static)!;

            //
            // Act
            //
            var task = (Task<int>)main.Invoke(null, new object[] { new[] { "--from-csharp", "sample.cs" } })!;
            var exitCode = await task;

            //
            // Assert
            //
            Assert.AreEqual(1, exitCode);
            StringAssert.Contains(error.ToString(), "Missing value for --from-csharp container-name.");
            Assert.AreEqual(string.Empty, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }
}
