namespace DisplayRotationTray;

using System.Text.Json;

public class AppSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public static readonly int[] RotationCycle = [0, 1, 2, 3];

    public bool LeftClickRotationEnabled { get; set; } = true;
    public string? LeftClickTargetDeviceName { get; set; }
    public Dictionary<string, List<int>> LeftClickRotationTargets { get; set; } = [];

    public static AppSettings Load()
    {
        try
        {
            var path = GetSettingsPath();
            if (!File.Exists(path))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        var path = GetSettingsPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions));
    }

    public List<int> GetEnabledRotations(string deviceName)
    {
        if (!LeftClickRotationTargets.TryGetValue(deviceName, out var rotations))
        {
            return [];
        }

        return RotationCycle
            .Where(rotation => rotations.Contains(rotation))
            .ToList();
    }

    private static string GetSettingsPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "DisplayRotationTray", "settings.json");
    }
}
