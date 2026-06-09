namespace IdGenerator.Cli;

internal static class Program
{
    private const int ExitSuccess = 0;
    private const int ExitError = 1;
    private const int ExitHelp = 2;

    public static async Task<int> Main(string[] args)
    {
        args = "--from-csharp C:\\_dev\\Bowdark\\CNP.PowerSyncPortal.AF\\CNP.PowerSyncPortal.Tests\\JT\\ProjectAndTaskForPartnerProjectCounterDependencyNotMetTests.cs".Split(" ");
        var options = CliOptions.Parse(args);
        if (options.ShowHelp)
        {
            Console.Write(HelpText);
            return ExitHelp;
        }

        try
        {
            var settings = LoadSettings(options);
            var guidGenerator = CreateGuidGenerator(options);
            var logic = new Logic(settings, guidGenerator);
            var entityInput = await ResolveEntityInputAsync(options, logic);
            if (!string.IsNullOrWhiteSpace(options.FromCSharp))
            {
                Console.Write(entityInput);
                return ExitSuccess;
            }
            var idsByType = logic.ParseEntityTypes(entityInput);
            Console.Write(logic.GenerateOutput(idsByType.Values));
            return ExitSuccess;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return ExitError;
        }
    }

    private static IdGeneratorSettings LoadSettings(CliOptions options)
    {
        var settings = new IdGeneratorSettings();
        if (!string.IsNullOrWhiteSpace(options.SettingsFile))
        {
            settings.FilePath = options.SettingsFile!;
        }

        settings = settings.Load();

        if (options.UseClassIds.HasValue)
        {
            settings.UseClassIds = options.UseClassIds.Value;
        }

        if (options.UseTargetTypedNew.HasValue)
        {
            settings.UseTargetTypedNew = options.UseTargetTypedNew.Value;
        }

        return settings;
    }

    private static IGuidGenerator CreateGuidGenerator(CliOptions options)
    {
        if (options.Seed.HasValue)
        {
            return new SeededGuidGenerator(options.Seed.Value);
        }

        return new GuidGenerator();
    }

    private static async Task<string> ResolveEntityInputAsync(CliOptions options, Logic logic)
    {
        if (!string.IsNullOrWhiteSpace(options.FromCSharp))
        {
            var csharp = await ReadTextSourceAsync(options.FromCSharp!);
            var parsed = IdFieldInfo.ParseIdFields(csharp);
            if (parsed.Issues.Count > 0)
            {
                var issues = string.Join(Environment.NewLine, parsed.Issues.Select(i => i.ToString()));
                throw new InvalidOperationException("Invalid C# input:" + Environment.NewLine + issues);
            }

            if (parsed.IdFieldInfos.Count == 0)
            {
                throw new InvalidOperationException("No Id<T> definitions were found in the C# input.");
            }

            return logic.CreateEntitiesText(parsed.IdFieldInfos, "|");
        }

        if (!string.IsNullOrWhiteSpace(options.InputFile))
        {
            return await File.ReadAllTextAsync(options.InputFile!);
        }

        if (!string.IsNullOrWhiteSpace(options.Input))
        {
            return options.Input!;
        }

        if (Console.IsInputRedirected)
        {
            return await Console.In.ReadToEndAsync();
        }

        throw new InvalidOperationException("Entity input is required. Pass input text, --input, --input-file, --from-csharp, or --help.");
    }

    private static async Task<string> ReadTextSourceAsync(string pathOrDash)
    {
        if (pathOrDash == "-")
        {
            return await Console.In.ReadToEndAsync();
        }

        return await File.ReadAllTextAsync(pathOrDash);
    }

    private static string HelpText =>
        """
        DataverseUnitTest Id Generator CLI

        Usage:
          idgen [input] [options]

        Input:
          [input]                   Entity definitions. Use | instead of newlines on the command line.
          -i, --input <text>        Entity input text.
          -f, --input-file <path>   Read entity input from a file.

        Modes:
          --from-csharp <path|->    Parse existing C# Id definitions and output entity input text.
                                    IDs are not regenerated. Use - for stdin.

        Options:
          --seed [int]              Use deterministic GUIDs (default seed: 1 when omitted).
          --settings-file <path>    Load IdGeneratorSettings.json from the given path.
          --use-class-ids           Generate class-based IDs (default).
          --use-struct-ids          Generate struct-based IDs.
          --use-target-typed-new    Use target-typed new expressions (default).
          --use-explicit-new        Use explicit new Id<T>(...) expressions.
          -h, --help                Show help.

        Examples:
          idgen "Account 2|Contact,Partners,Jim,Bob"
          idgen --seed 42 "Account|Contact 2"
          idgen --from-csharp MyTest.cs
          idgen --from-csharp - < existing-ids.cs

        """;

    private sealed class CliOptions
    {
        public string? Input { get; init; }
        public string? InputFile { get; init; }
        public string? FromCSharp { get; init; }
        public string? SettingsFile { get; init; }
        public int? Seed { get; init; }
        public bool? UseClassIds { get; init; }
        public bool? UseTargetTypedNew { get; init; }
        public bool ShowHelp { get; init; }

        public static CliOptions Parse(string[] args)
        {
            string? input = null;
            string? inputFile = null;
            string? fromCSharp = null;
            string? settingsFile = null;
            int? seed = null;
            bool? useClassIds = null;
            bool? useTargetTypedNew = null;
            var showHelp = false;
            var positionalInput = new List<string>();

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "-h":
                    case "--help":
                        showHelp = true;
                        break;
                    case "-i":
                    case "--input":
                        input = RequireValue(args, ref i, arg);
                        break;
                    case "-f":
                    case "--input-file":
                        inputFile = RequireValue(args, ref i, arg);
                        break;
                    case "--from-csharp":
                        fromCSharp = RequireValue(args, ref i, arg);
                        break;
                    case "--settings-file":
                        settingsFile = RequireValue(args, ref i, arg);
                        break;
                    case "--seed":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out var explicitSeed))
                        {
                            seed = explicitSeed;
                            i++;
                        }
                        else
                        {
                            seed = 1;
                        }
                        break;
                    case "--use-class-ids":
                        useClassIds = true;
                        break;
                    case "--use-struct-ids":
                        useClassIds = false;
                        break;
                    case "--use-target-typed-new":
                        useTargetTypedNew = true;
                        break;
                    case "--use-explicit-new":
                        useTargetTypedNew = false;
                        break;
                    default:
                        if (arg.StartsWith('-'))
                        {
                            throw new InvalidOperationException($"Unknown option: {arg}");
                        }

                        positionalInput.Add(arg);
                        break;
                }
            }

            if (positionalInput.Count > 0 && string.IsNullOrWhiteSpace(input))
            {
                input = string.Join(' ', positionalInput);
            }

            return new CliOptions
            {
                Input = input,
                InputFile = inputFile,
                FromCSharp = fromCSharp,
                SettingsFile = settingsFile,
                Seed = seed,
                UseClassIds = useClassIds,
                UseTargetTypedNew = useTargetTypedNew,
                ShowHelp = showHelp
            };
        }

        private static string RequireValue(string[] args, ref int index, string optionName)
        {
            if (index + 1 >= args.Length)
            {
                throw new InvalidOperationException($"Missing value for {optionName}.");
            }

            index++;
            return args[index];
        }
    }
}
