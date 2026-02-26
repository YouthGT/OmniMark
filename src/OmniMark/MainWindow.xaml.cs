using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace OmniMark;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    public MainWindow(WatermarkSettings settings)
    {
        InitializeComponent();
        ApplySettings(settings);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        MakeClickThrough();
    }

    private void MakeClickThrough()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        int extStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
    }

    public void ApplySettings(WatermarkSettings settings)
    {
        Overlay.WatermarkText = settings.WatermarkText;
        Overlay.FontSize = settings.FontSize;
        Overlay.OpacityValue = settings.Opacity;
        Overlay.TextColor = (Color)ColorConverter.ConvertFromString(settings.TextColor);
        Overlay.SpacingX = settings.SpacingX;
        Overlay.SpacingY = settings.SpacingY;
        Overlay.RotationAngle = settings.RotationAngle;

        ApplyCoverTaskbar(settings.CoverTaskbar);
    }

    private void ApplyCoverTaskbar(bool coverTaskbar)
    {
        if (coverTaskbar)
        {
            // 覆盖整个屏幕（包括任务栏）
            WindowState = WindowState.Normal;
            Left = 0;
            Top = 0;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
        }
        else
        {
            // 使用最大化模式（不覆盖任务栏）
            WindowState = WindowState.Maximized;
        }
    }
}
