using System.Windows;
using System.Windows.Controls;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.MessageBox;

namespace OmniMark;

/// <summary>
/// Schedule Dialog for configuring preset rotation
/// </summary>
public partial class ScheduleDialog : Window
{
    private readonly PresetScheduler _scheduler;
    private readonly Dictionary<string, CheckBox> _presetCheckBoxes = new();

    public ScheduleDialog(PresetScheduler scheduler)
    {
        InitializeComponent();
        _scheduler = scheduler;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _scheduler.Settings;

        ChkEnabled.IsChecked = settings.IsEnabled;
        SliderInterval.Value = settings.IntervalMinutes;
        TxtIntervalValue.Text = settings.IntervalMinutes.ToString();

        if (settings.Mode == ScheduleMode.Random)
            RbRandom.IsChecked = true;
        else
            RbSequential.IsChecked = true;

        // Load preset list
        LoadPresetList(settings.SelectedPresets);

        // Update status
        UpdateStatus();

        // Bind enable checkbox to panel visibility
        ChkEnabled.Checked += (s, e) => SettingsPanel.IsEnabled = true;
        ChkEnabled.Unchecked += (s, e) => SettingsPanel.IsEnabled = false;
        SettingsPanel.IsEnabled = settings.IsEnabled;
    }

    private void LoadPresetList(List<string> selectedPresets)
    {
        PresetCheckList.Children.Clear();
        _presetCheckBoxes.Clear();

        var presets = WatermarkSettings.GetPresetNames();
        
        if (presets.Count == 0)
        {
            TxtNoPresets.Visibility = Visibility.Visible;
            return;
        }

        TxtNoPresets.Visibility = Visibility.Collapsed;

        foreach (var preset in presets.OrderBy(p => p))
        {
            var checkBox = new CheckBox
            {
                Content = preset,
                IsChecked = selectedPresets.Contains(preset),
                Margin = new Thickness(0, 4, 0, 4)
            };
            
            PresetCheckList.Children.Add(checkBox);
            _presetCheckBoxes[preset] = checkBox;
        }
    }

    private void UpdateStatus()
    {
        if (_scheduler.IsRunning)
        {
            TxtStatus.Text = "Running";
            TxtStatus.Foreground = System.Windows.Media.Brushes.Green;
        }
        else
        {
            TxtStatus.Text = "Not running";
            TxtStatus.Foreground = System.Windows.Media.Brushes.Gray;
        }
    }

    private void SliderInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtIntervalValue != null)
            TxtIntervalValue.Text = ((int)SliderInterval.Value).ToString();
    }

    private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var cb in _presetCheckBoxes.Values)
            cb.IsChecked = true;
    }

    private void BtnSelectNone_Click(object sender, RoutedEventArgs e)
    {
        foreach (var cb in _presetCheckBoxes.Values)
            cb.IsChecked = false;
    }

    private void BtnTestSwitch_Click(object sender, RoutedEventArgs e)
    {
        // Apply current settings temporarily and test
        var settings = BuildSettings();
        
        if (settings.SelectedPresets.Count == 0)
        {
            MessageBox.Show("Please select at least one preset to rotate.", 
                "OmniMark", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var nextPreset = settings.GetNextPreset();
        if (!string.IsNullOrEmpty(nextPreset))
        {
            var preset = WatermarkSettings.LoadPreset(nextPreset);
            if (preset != null)
            {
                _scheduler.SwitchToNext();
                MessageBox.Show($"Switched to preset: {nextPreset}", 
                    "OmniMark", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private ScheduleSettings BuildSettings()
    {
        var settings = new ScheduleSettings
        {
            IsEnabled = ChkEnabled.IsChecked == true,
            IntervalMinutes = (int)SliderInterval.Value,
            Mode = RbRandom.IsChecked == true ? ScheduleMode.Random : ScheduleMode.Sequential,
            SelectedPresets = _presetCheckBoxes
                .Where(kv => kv.Value.IsChecked == true)
                .Select(kv => kv.Key)
                .ToList(),
            CurrentIndex = _scheduler.Settings.CurrentIndex,
            LastPresetName = _scheduler.Settings.LastPresetName
        };

        return settings;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var settings = BuildSettings();

        if (settings.IsEnabled && settings.SelectedPresets.Count == 0)
        {
            MessageBox.Show("Please select at least one preset to rotate, or disable the schedule.", 
                "OmniMark", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _scheduler.UpdateSettings(settings);
        
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
