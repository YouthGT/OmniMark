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
        Overlay.FontFamilyName = settings.FontFamily;
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
        // 必须先设置为 Normal 状态才能手动调整位置和大小
        WindowState = WindowState.Normal;

        if (coverTaskbar)
        {
            // 覆盖整个屏幕（包括任务栏）
            Left = 0;
            Top = 0;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
        }
        else
        {
            // 仅覆盖工作区域（不覆盖任务栏）
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Left;
            Top = workArea.Top;
            Width = workArea.Width;
            Height = workArea.Height;
        }
    }
}
