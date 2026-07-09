using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinTaskIT.Config;

internal static class ConfigManager
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinTaskIT");
    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static AppSettings Load()
    {
        if (!File.Exists(ConfigPath))
            return new AppSettings();

        try
        {
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        Directory.CreateDirectory(ConfigDir);

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        var tempPath = ConfigPath + ".tmp";
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, ConfigPath, overwrite: true);
    }

    /// <summary>Deletes config.json and its containing folder. Used by Uninstall
    /// so no trace is left under %AppData%.</summary>
    public static void DeleteAll()
    {
        if (Directory.Exists(ConfigDir))
            Directory.Delete(ConfigDir, recursive: true);
    }
}
