using System.IO;
using System.Text.Json;

namespace OmniMark;

/// <summary>
/// Persisted watermark configuration.
/// </summary>
public class WatermarkSettings
{
    private static readonly string AppDataPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Config");

    private static readonly string SettingsPath = Path.Combine(AppDataPath, "settings.json");
    private static readonly string PresetsPath = Path.Combine(AppDataPath, "Presets");

    public string WatermarkText { get; set; } = "CONFIDENTIAL";
    public string FontFamily { get; set; } = "Segoe UI";
    public double FontSize { get; set; } = 36.0;
    public double Opacity { get; set; } = 0.15;
    public string TextColor { get; set; } = "#808080";
    public double SpacingX { get; set; } = 260.0;
    public double SpacingY { get; set; } = 180.0;
    public double RotationAngle { get; set; } = -45.0;
    public bool CoverTaskbar { get; set; } = false;

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

    /// <summary>
    /// 保存为预设配置
    /// </summary>
    public void SaveAsPreset(string presetName)
    {
        try
        {
            Directory.CreateDirectory(PresetsPath);
            var fileName = SanitizeFileName(presetName) + ".json";
            var filePath = Path.Combine(PresetsPath, fileName);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }

    /// <summary>
    /// 加载预设配置
    /// </summary>
    public static WatermarkSettings? LoadPreset(string presetName)
    {
        try
        {
            var fileName = SanitizeFileName(presetName) + ".json";
            var filePath = Path.Combine(PresetsPath, fileName);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<WatermarkSettings>(json);
            }
        }
        catch
        {
            // Ignore errors
        }
        return null;
    }

    /// <summary>
    /// 获取所有预设配置名称
    /// </summary>
    public static List<string> GetPresetNames()
    {
        var presets = new List<string>();
        try
        {
            if (Directory.Exists(PresetsPath))
            {
                var files = Directory.GetFiles(PresetsPath, "*.json");
                foreach (var file in files)
                {
                    presets.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
        }
        catch
        {
            // Ignore errors
        }
        return presets;
    }

    /// <summary>
    /// 删除预设配置
    /// </summary>
    public static bool DeletePreset(string presetName)
    {
        try
        {
            var fileName = SanitizeFileName(presetName) + ".json";
            var filePath = Path.Combine(PresetsPath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
        }
        catch
        {
            // Ignore errors
        }
        return false;
    }

    /// <summary>
    /// 清理文件名中的非法字符
    /// </summary>
    private static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(sanitized) ? "Preset" : sanitized;
    }

    /// <summary>
    /// 复制当前设置
    /// </summary>
    public WatermarkSettings Clone()
    {
        return new WatermarkSettings
        {
            WatermarkText = WatermarkText,
            FontFamily = FontFamily,
            FontSize = FontSize,
            Opacity = Opacity,
            TextColor = TextColor,
            SpacingX = SpacingX,
            SpacingY = SpacingY,
            RotationAngle = RotationAngle,
            CoverTaskbar = CoverTaskbar
        };
    }
}
