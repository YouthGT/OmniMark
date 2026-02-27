using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using FontFamily = System.Windows.Media.FontFamily;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace OmniMark;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly MainWindow _mainWindow;
    private readonly WatermarkSettings _settings;
    private bool _isUpdatingSlider;
    private bool _isLoadingPreset;
    private string? _currentPresetName;
    private int _presetCounter = 1;

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("gdi32.dll")]
    private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    public SettingsWindow(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _settings = WatermarkSettings.Load();
        LoadFontFamilies();
        LoadPresets();
        LoadSettings();
    }

    private void LoadFontFamilies()
    {
        var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.Source).ToList();
        CmbFontFamily.ItemsSource = fonts;
        CmbFontFamily.DisplayMemberPath = "Source";
    }

    private void LoadPresets()
    {
        var presets = WatermarkSettings.GetPresetNames();
        CmbPresets.Items.Clear();
        CmbPresets.Items.Add("(Current Settings)");
        foreach (var preset in presets.OrderBy(p => p))
        {
            CmbPresets.Items.Add(preset);
        }
        CmbPresets.SelectedIndex = 0;
        _currentPresetName = null;
        UpdatePresetButtons();
    }

    private void UpdatePresetButtons()
    {
        bool hasPresetSelected = _currentPresetName != null;
        BtnSavePreset.IsEnabled = hasPresetSelected;
        BtnDeletePreset.IsEnabled = hasPresetSelected;
    }

    private void LoadSettings()
    {
        LoadSettingsFromObject(_settings);
    }

    private void LoadSettingsFromObject(WatermarkSettings settings)
    {
        _isLoadingPreset = true;

        TxtWatermarkText.Text = settings.WatermarkText;
        
        var fontFamily = Fonts.SystemFontFamilies.FirstOrDefault(f => 
            f.Source.Equals(settings.FontFamily, StringComparison.OrdinalIgnoreCase));
        CmbFontFamily.SelectedItem = fontFamily ?? Fonts.SystemFontFamilies.FirstOrDefault();

        SliderFontSize.Value = settings.FontSize;
        SliderOpacity.Value = settings.Opacity;
        SliderAngle.Value = settings.RotationAngle;
        TxtColor.Text = settings.TextColor;
        UpdateColorPreview();
        SliderSpacingX.Value = settings.SpacingX;
        SliderSpacingY.Value = settings.SpacingY;
        ChkCoverTaskbar.IsChecked = settings.CoverTaskbar;

        _isLoadingPreset = false;
    }

    private WatermarkSettings GetCurrentSettingsFromUI()
    {
        return new WatermarkSettings
        {
            WatermarkText = TxtWatermarkText.Text,
            FontFamily = (CmbFontFamily.SelectedItem as FontFamily)?.Source ?? "Segoe UI",
            FontSize = SliderFontSize.Value,
            Opacity = SliderOpacity.Value,
            RotationAngle = SliderAngle.Value,
            TextColor = TxtColor.Text,
            SpacingX = SliderSpacingX.Value,
            SpacingY = SliderSpacingY.Value,
            CoverTaskbar = ChkCoverTaskbar.IsChecked == true
        };
    }

    private void UpdateColorPreview()
    {
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(TxtColor.Text);
            ColorPreview.Background = new SolidColorBrush(color);
        }
        catch
        {
            ColorPreview.Background = System.Windows.Media.Brushes.Transparent;
        }
    }

    #region Preset Management

    private void CmbPresets_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isLoadingPreset || CmbPresets.SelectedIndex < 0)
            return;

        if (CmbPresets.SelectedIndex == 0)
        {
            // Load current settings
            _currentPresetName = null;
            LoadSettingsFromObject(_settings);
        }
        else
        {
            var presetName = CmbPresets.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(presetName))
            {
                var preset = WatermarkSettings.LoadPreset(presetName);
                if (preset != null)
                {
                    _currentPresetName = presetName;
                    LoadSettingsFromObject(preset);
                }
            }
        }
        UpdatePresetButtons();
    }

    private void BtnSavePreset_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPresetName))
            return;

        if (!ValidateColorInput())
            return;

        var settings = GetCurrentSettingsFromUI();
        settings.SaveAsPreset(_currentPresetName);
        MessageBox.Show($"Preset '{_currentPresetName}' saved successfully.", 
            "OmniMark", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnSaveAsPreset_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateColorInput())
            return;

        var dialog = new SavePresetDialog(GenerateDefaultPresetName());
        dialog.Owner = this;
        
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.PresetName))
        {
            var presetName = dialog.PresetName.Trim();
            var settings = GetCurrentSettingsFromUI();
            settings.SaveAsPreset(presetName);
            
            _currentPresetName = presetName;
            LoadPresets();
            
            // Select the newly saved preset
            for (int i = 0; i < CmbPresets.Items.Count; i++)
            {
                if (CmbPresets.Items[i]?.ToString() == presetName)
                {
                    _isLoadingPreset = true;
                    CmbPresets.SelectedIndex = i;
                    _isLoadingPreset = false;
                    break;
                }
            }
            UpdatePresetButtons();

            MessageBox.Show($"Preset '{presetName}' saved successfully.", 
                "OmniMark", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void BtnDeletePreset_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPresetName))
            return;

        var result = MessageBox.Show($"Are you sure you want to delete preset '{_currentPresetName}'?",
            "OmniMark", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            if (WatermarkSettings.DeletePreset(_currentPresetName))
            {
                _currentPresetName = null;
                LoadPresets();
                LoadSettingsFromObject(_settings);
            }
        }
    }

    private string GenerateDefaultPresetName()
    {
        var existingPresets = WatermarkSettings.GetPresetNames();
        string name;
        do
        {
            name = $"Preset {_presetCounter++}";
        } while (existingPresets.Contains(name));
        return name;
    }

    private bool ValidateColorInput()
    {
        try
        {
            ColorConverter.ConvertFromString(TxtColor.Text);
            return true;
        }
        catch
        {
            MessageBox.Show("Invalid color value. Please enter a valid hex color (e.g. #808080).",
                "OmniMark", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
    }

    #endregion

    #region Slider ValueChanged Events

    private void SliderFontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtFontSizeValue != null && !_isUpdatingSlider)
            TxtFontSizeValue.Text = $"{(int)SliderFontSize.Value}";
    }

    private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtOpacityValue != null && !_isUpdatingSlider)
            TxtOpacityValue.Text = $"{(int)(SliderOpacity.Value * 100)}";
    }

    private void SliderAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtAngleValue != null && !_isUpdatingSlider)
            TxtAngleValue.Text = $"{(int)SliderAngle.Value}";
    }

    private void SliderSpacingX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtSpacingXValue != null && !_isUpdatingSlider)
            TxtSpacingXValue.Text = $"{(int)SliderSpacingX.Value}";
    }

    private void SliderSpacingY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtSpacingYValue != null && !_isUpdatingSlider)
            TxtSpacingYValue.Text = $"{(int)SliderSpacingY.Value}";
    }

    #endregion

    #region TextBox Input Events

    private void TxtFontSizeValue_LostFocus(object sender, RoutedEventArgs e) => ApplyFontSizeFromTextBox();
    private void TxtFontSizeValue_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) ApplyFontSizeFromTextBox();
    }
    private void ApplyFontSizeFromTextBox()
    {
        if (int.TryParse(TxtFontSizeValue.Text, out int value))
        {
            value = Math.Clamp(value, (int)SliderFontSize.Minimum, (int)SliderFontSize.Maximum);
            _isUpdatingSlider = true;
            SliderFontSize.Value = value;
            _isUpdatingSlider = false;
            TxtFontSizeValue.Text = value.ToString();
        }
    }

    private void TxtOpacityValue_LostFocus(object sender, RoutedEventArgs e) => ApplyOpacityFromTextBox();
    private void TxtOpacityValue_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) ApplyOpacityFromTextBox();
    }
    private void ApplyOpacityFromTextBox()
    {
        if (int.TryParse(TxtOpacityValue.Text, out int value))
        {
            value = Math.Clamp(value, 1, 100);
            _isUpdatingSlider = true;
            SliderOpacity.Value = value / 100.0;
            _isUpdatingSlider = false;
            TxtOpacityValue.Text = value.ToString();
        }
    }

    private void TxtAngleValue_LostFocus(object sender, RoutedEventArgs e) => ApplyAngleFromTextBox();
    private void TxtAngleValue_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) ApplyAngleFromTextBox();
    }
    private void ApplyAngleFromTextBox()
    {
        if (int.TryParse(TxtAngleValue.Text, out int value))
        {
            value = Math.Clamp(value, (int)SliderAngle.Minimum, (int)SliderAngle.Maximum);
            _isUpdatingSlider = true;
            SliderAngle.Value = value;
            _isUpdatingSlider = false;
            TxtAngleValue.Text = value.ToString();
        }
    }

    private void TxtSpacingXValue_LostFocus(object sender, RoutedEventArgs e) => ApplySpacingXFromTextBox();
    private void TxtSpacingXValue_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) ApplySpacingXFromTextBox();
    }
    private void ApplySpacingXFromTextBox()
    {
        if (int.TryParse(TxtSpacingXValue.Text, out int value))
        {
            value = Math.Clamp(value, (int)SliderSpacingX.Minimum, (int)SliderSpacingX.Maximum);
            _isUpdatingSlider = true;
            SliderSpacingX.Value = value;
            _isUpdatingSlider = false;
            TxtSpacingXValue.Text = value.ToString();
        }
    }

    private void TxtSpacingYValue_LostFocus(object sender, RoutedEventArgs e) => ApplySpacingYFromTextBox();
    private void TxtSpacingYValue_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) ApplySpacingYFromTextBox();
    }
    private void ApplySpacingYFromTextBox()
    {
        if (int.TryParse(TxtSpacingYValue.Text, out int value))
        {
            value = Math.Clamp(value, (int)SliderSpacingY.Minimum, (int)SliderSpacingY.Maximum);
            _isUpdatingSlider = true;
            SliderSpacingY.Value = value;
            _isUpdatingSlider = false;
            TxtSpacingYValue.Text = value.ToString();
        }
    }

    #endregion

    #region Color Picker

    private void TxtColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        UpdateColorPreview();
    }

    private void BtnPickColor_Click(object sender, RoutedEventArgs e)
    {
        using var colorDialog = new System.Windows.Forms.ColorDialog();
        
        try
        {
            var currentColor = (Color)ColorConverter.ConvertFromString(TxtColor.Text);
            colorDialog.Color = System.Drawing.Color.FromArgb(currentColor.R, currentColor.G, currentColor.B);
        }
        catch { }

        colorDialog.FullOpen = true;
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var color = colorDialog.Color;
            TxtColor.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }

    private void BtnScreenPick_Click(object sender, RoutedEventArgs e)
    {
        var picker = new ScreenColorPickerWindow();
        picker.Owner = this;
        if (picker.ShowDialog() == true && picker.PickedColor.HasValue)
        {
            var color = picker.PickedColor.Value;
            TxtColor.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }

    #endregion

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateColorInput())
            return;

        var settings = GetCurrentSettingsFromUI();
        
        // Copy to main settings
        _settings.WatermarkText = settings.WatermarkText;
        _settings.FontFamily = settings.FontFamily;
        _settings.FontSize = settings.FontSize;
        _settings.Opacity = settings.Opacity;
        _settings.RotationAngle = settings.RotationAngle;
        _settings.TextColor = settings.TextColor;
        _settings.SpacingX = settings.SpacingX;
        _settings.SpacingY = settings.SpacingY;
        _settings.CoverTaskbar = settings.CoverTaskbar;

        _settings.Save();
        _mainWindow.ApplySettings(_settings);

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
