using System.IO;
using System.Text.Json;

namespace OmniMark;

/// <summary>
/// 水印循环调度配置
/// </summary>
public class ScheduleSettings
{
    private static readonly string SettingsPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Config",
        "schedule.json");

    /// <summary>
    /// 是否启用自动循环
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// 切换间隔（分钟）
    /// </summary>
    public int IntervalMinutes { get; set; } = 5;

    /// <summary>
    /// 循环模式：Sequential（顺序）或 Random（随机）
    /// </summary>
    public ScheduleMode Mode { get; set; } = ScheduleMode.Sequential;

    /// <summary>
    /// 参与循环的预设名称列表
    /// </summary>
    public List<string> SelectedPresets { get; set; } = new();

    /// <summary>
    /// 当前预设索引（用于顺序模式）
    /// </summary>
    public int CurrentIndex { get; set; } = 0;

    /// <summary>
    /// 上次切换的预设名称（用于随机模式避免重复）
    /// </summary>
    public string? LastPresetName { get; set; }

    public static ScheduleSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<ScheduleSettings>(json) ?? new ScheduleSettings();
            }
        }
        catch
        {
            // Ignore errors and use defaults
        }
        return new ScheduleSettings();
    }

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }

    /// <summary>
    /// 获取下一个预设名称
    /// </summary>
    public string? GetNextPreset()
    {
        if (SelectedPresets.Count == 0)
            return null;

        // 过滤掉已删除的预设
        var availablePresets = WatermarkSettings.GetPresetNames();
        var validPresets = SelectedPresets.Where(p => availablePresets.Contains(p)).ToList();

        if (validPresets.Count == 0)
            return null;

        string? nextPreset;

        if (Mode == ScheduleMode.Random)
        {
            // 随机模式：避免连续重复
            if (validPresets.Count == 1)
            {
                nextPreset = validPresets[0];
            }
            else
            {
                var random = new Random();
                do
                {
                    nextPreset = validPresets[random.Next(validPresets.Count)];
                } while (nextPreset == LastPresetName && validPresets.Count > 1);
            }
        }
        else
        {
            // 顺序模式
            CurrentIndex = CurrentIndex % validPresets.Count;
            nextPreset = validPresets[CurrentIndex];
            CurrentIndex = (CurrentIndex + 1) % validPresets.Count;
        }

        LastPresetName = nextPreset;
        Save(); // 保存状态

        return nextPreset;
    }

    /// <summary>
    /// 验证并清理无效的预设
    /// </summary>
    public void ValidatePresets()
    {
        var availablePresets = WatermarkSettings.GetPresetNames();
        SelectedPresets = SelectedPresets.Where(p => availablePresets.Contains(p)).ToList();
        
        if (CurrentIndex >= SelectedPresets.Count)
            CurrentIndex = 0;
    }
}

/// <summary>
/// 调度模式
/// </summary>
public enum ScheduleMode
{
    /// <summary>
    /// 顺序循环
    /// </summary>
    Sequential,

    /// <summary>
    /// 随机切换
    /// </summary>
    Random
}
