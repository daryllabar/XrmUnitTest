using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace IdGenerator;

public class IdGeneratorSettings
{
    public const string SettingsFilePath = "IdGeneratorSettings.json";

    [Category("General")]
    [DisplayName("Use Target-Typed New Expressions")]
    [Description("If true, generates statements using target-typed 'new' expressions (C# 9.0+).")]
    public bool UseTargetTypedNew { get; set; }

    [Browsable(false)]
    public int PropertyGridLabelWidth { get; set; } = 250; // Default width

    private string? _filePath;
    [Browsable(false)]
    public string FilePath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_filePath))
            {
                return _filePath;
            }
            _filePath = Path.Combine(AppContext.BaseDirectory, SettingsFilePath);
            return _filePath;
        }
        set => _filePath = value;
    }

    private static readonly JsonSerializerOptions SerializationOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public void Save()
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, SerializationOptions));
    }

    public IdGeneratorSettings Load()
    {
        if (!File.Exists(FilePath))
        {
            return new IdGeneratorSettings();
        }
        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<IdGeneratorSettings>(json, SerializationOptions) ?? new IdGeneratorSettings();

    }
}