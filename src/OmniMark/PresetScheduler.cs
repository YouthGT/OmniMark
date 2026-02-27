using System.Windows.Threading;

namespace OmniMark;

/// <summary>
/// 水印预设循环调度管理器
/// </summary>
public class PresetScheduler : IDisposable
{
    private readonly MainWindow _mainWindow;
    private readonly DispatcherTimer _timer;
    private ScheduleSettings _settings;
    private bool _disposed;

    public event EventHandler<PresetChangedEventArgs>? PresetChanged;

    public bool IsRunning => _timer.IsEnabled;
    public ScheduleSettings Settings => _settings;

    public PresetScheduler(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        _settings = ScheduleSettings.Load();
        
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(_settings.IntervalMinutes)
        };
        _timer.Tick += Timer_Tick;
    }

    /// <summary>
    /// 启动调度器
    /// </summary>
    public void Start()
    {
        if (_settings.IsEnabled && _settings.SelectedPresets.Count > 0)
        {
            _settings.ValidatePresets();
            _timer.Interval = TimeSpan.FromMinutes(Math.Max(1, _settings.IntervalMinutes));
            _timer.Start();
        }
    }

    /// <summary>
    /// 停止调度器
    /// </summary>
    public void Stop()
    {
        _timer.Stop();
    }

    /// <summary>
    /// 重新加载配置并重启
    /// </summary>
    public void Reload()
    {
        Stop();
        _settings = ScheduleSettings.Load();
        Start();
    }

    /// <summary>
    /// 更新配置
    /// </summary>
    public void UpdateSettings(ScheduleSettings settings)
    {
        _settings = settings;
        _settings.Save();
        
        Stop();
        if (_settings.IsEnabled && _settings.SelectedPresets.Count > 0)
        {
            _timer.Interval = TimeSpan.FromMinutes(Math.Max(1, _settings.IntervalMinutes));
            _timer.Start();
        }
    }

    /// <summary>
    /// 立即切换到下一个预设
    /// </summary>
    public void SwitchToNext()
    {
        ApplyNextPreset();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        ApplyNextPreset();
    }

    private void ApplyNextPreset()
    {
        var presetName = _settings.GetNextPreset();
        if (string.IsNullOrEmpty(presetName))
            return;

        var preset = WatermarkSettings.LoadPreset(presetName);
        if (preset == null)
            return;

        // 应用预设
        _mainWindow.ApplySettings(preset);

        // 同时保存为当前设置
        preset.Save();

        // 触发事件
        PresetChanged?.Invoke(this, new PresetChangedEventArgs(presetName, preset));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _timer.Stop();
        _disposed = true;
    }
}

/// <summary>
/// 预设切换事件参数
/// </summary>
public class PresetChangedEventArgs : EventArgs
{
    public string PresetName { get; }
    public WatermarkSettings Settings { get; }

    public PresetChangedEventArgs(string presetName, WatermarkSettings settings)
    {
        PresetName = presetName;
        Settings = settings;
    }
}
