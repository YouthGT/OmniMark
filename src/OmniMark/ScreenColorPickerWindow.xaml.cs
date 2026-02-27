using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Point = System.Windows.Point;

namespace OmniMark;

/// <summary>
/// 屏幕取色窗口
/// </summary>
public partial class ScreenColorPickerWindow : Window
{
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

    public Color? PickedColor { get; private set; }

    private readonly DispatcherTimer _timer;

    public ScreenColorPickerWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _timer.Tick += Timer_Tick;

        Loaded += (s, e) => _timer.Start();
        Unloaded += (s, e) => _timer.Stop();

        MouseLeftButtonDown += OnMouseLeftButtonDown;
        KeyDown += OnKeyDown;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        var color = GetColorUnderCursor();
        UpdatePreview(color);
    }

    private Color GetColorUnderCursor()
    {
        GetCursorPos(out POINT point);

        IntPtr hdc = GetDC(IntPtr.Zero);
        uint pixel = GetPixel(hdc, point.X, point.Y);
        ReleaseDC(IntPtr.Zero, hdc);

        byte r = (byte)(pixel & 0x000000FF);
        byte g = (byte)((pixel & 0x0000FF00) >> 8);
        byte b = (byte)((pixel & 0x00FF0000) >> 16);

        return Color.FromRgb(r, g, b);
    }

    private void UpdatePreview(Color color)
    {
        ColorPreviewBox.Background = new SolidColorBrush(color);
        ColorHexText.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        // 更新放大镜位置
        GetCursorPos(out POINT point);
        
        // 转换为 WPF 坐标
        var source = PresentationSource.FromVisual(this);
        if (source != null)
        {
            var transform = source.CompositionTarget.TransformFromDevice;
            var wpfPoint = transform.Transform(new Point(point.X, point.Y));

            // 偏移放大镜，避免遮挡鼠标
            double offsetX = 20;
            double offsetY = 20;

            // 确保不超出屏幕边界
            double left = wpfPoint.X + offsetX;
            double top = wpfPoint.Y + offsetY;

            if (left + MagnifierBorder.Width > ActualWidth)
                left = wpfPoint.X - MagnifierBorder.Width - offsetX;
            if (top + MagnifierBorder.Height > ActualHeight)
                top = wpfPoint.Y - MagnifierBorder.Height - offsetY;

            Canvas.SetLeft(MagnifierBorder, left);
            Canvas.SetTop(MagnifierBorder, top);
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        PickedColor = GetColorUnderCursor();
        DialogResult = true;
        Close();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }
}
