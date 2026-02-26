using System.Windows;
using System.Windows.Media;
using ColorConverter = System.Windows.Media.ColorConverter;
using MessageBox = System.Windows.MessageBox;

namespace OmniMark;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly MainWindow _mainWindow;
    private readonly WatermarkSettings _settings;

    public SettingsWindow(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _settings = WatermarkSettings.Load();
        LoadSettings();
    }

    private void LoadSettings()
    {
        TxtWatermarkText.Text = _settings.WatermarkText;
        SliderFontSize.Value = _settings.FontSize;
        SliderOpacity.Value = _settings.Opacity;
        SliderAngle.Value = _settings.RotationAngle;
        TxtColor.Text = _settings.TextColor;
        SliderSpacingX.Value = _settings.SpacingX;
        SliderSpacingY.Value = _settings.SpacingY;
        ChkCoverTaskbar.IsChecked = _settings.CoverTaskbar;
    }

    private void SliderFontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtFontSizeValue != null)
            TxtFontSizeValue.Text = $"{(int)SliderFontSize.Value}";
    }

    private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtOpacityValue != null)
            TxtOpacityValue.Text = $"{SliderOpacity.Value:P0}";
    }

    private void SliderAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtAngleValue != null)
            TxtAngleValue.Text = $"{(int)SliderAngle.Value}°";
    }

    private void SliderSpacingX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtSpacingXValue != null)
            TxtSpacingXValue.Text = $"{(int)SliderSpacingX.Value}";
    }

    private void SliderSpacingY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtSpacingYValue != null)
            TxtSpacingYValue.Text = $"{(int)SliderSpacingY.Value}";
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        // Validate hex colour
        try
        {
            ColorConverter.ConvertFromString(TxtColor.Text);
        }
        catch
        {
            MessageBox.Show("Invalid colour value. Please enter a valid hex colour (e.g. #808080).",
                "OmniMark", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _settings.WatermarkText = TxtWatermarkText.Text;
        _settings.FontSize = SliderFontSize.Value;
        _settings.Opacity = SliderOpacity.Value;
        _settings.RotationAngle = SliderAngle.Value;
        _settings.TextColor = TxtColor.Text;
        _settings.SpacingX = SliderSpacingX.Value;
        _settings.SpacingY = SliderSpacingY.Value;
        _settings.CoverTaskbar = ChkCoverTaskbar.IsChecked == true;

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
