using System.IO;
using System.Text.Json;

namespace OmniMark;

/// <summary>
/// Persisted watermark configuration.
/// </summary>
public class WatermarkSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OmniMark",
        "settings.json");

    public string WatermarkText { get; set; } = "CONFIDENTIAL";
    public double FontSize { get; set; } = 36.0;
    public double Opacity { get; set; } = 0.15;
    public string TextColor { get; set; } = "#808080";
    public double SpacingX { get; set; } = 260.0;
    public double SpacingY { get; set; } = 180.0;
    public double RotationAngle { get; set; } = -45.0;

    public static WatermarkSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<WatermarkSettings>(json) ?? new WatermarkSettings();
            }
        }
        catch
        {
            // Ignore errors and use defaults
        }

        return new WatermarkSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }
}
