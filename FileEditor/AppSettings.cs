using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Editor;
public class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlurFileEditor\\settings.json");

    public static AppSettings Instance { get; private set; } =
        JsonSerializer.Deserialize(ReadAllTextOrEmpty(SettingsPath), AppSettingsJsonContext.Default.AppSettings);
    public List<string> RecentProjects { get; set; } = new();
    public bool OpenLastProjectAutomatically { get; set; }
    public void Save()
    {
        var dir = Path.GetDirectoryName(SettingsPath)!;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, AppSettingsJsonContext.Default.AppSettings));
    }
    static string ReadAllTextOrEmpty(string filePath)
    {
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }
        else
        {
            return "{}";
        }
    }
}
[JsonSerializable(typeof(AppSettings))]
internal partial class AppSettingsJsonContext : JsonSerializerContext
{
}