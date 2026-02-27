using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Point = System.Windows.Point;
using FlowDirection = System.Windows.FlowDirection;

namespace OmniMark;

/// <summary>
/// A transparent FrameworkElement that renders a repeated, rotated watermark pattern.
/// </summary>
public class WatermarkOverlay : FrameworkElement
{
    public static readonly DependencyProperty WatermarkTextProperty =
        DependencyProperty.Register(nameof(WatermarkText), typeof(string), typeof(WatermarkOverlay),
            new FrameworkPropertyMetadata("CONFIDENTIAL", FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontFamilyNameProperty =
        DependencyProperty.Register(nameof(FontFamilyName), typeof(string), typeof(WatermarkOverlay),
            new FrameworkPropertyMetadata("Segoe UI", FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(WatermarkOverlay),
            new FrameworkPropertyMetadata(36.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty OpacityValueProperty =
        DependencyProperty.Register(nameof(OpacityValue), typeof(double), typeof(WatermarkOverlay),
            new FrameworkPropertyMetadata(0.15, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TextColorProperty =
        DependencyProperty.Register(nameof(TextColor), typeof(System.Windows.Media.Color), typeof(WatermarkOverlay),
            new FrameworkPropertyMetadata(Colors.Gray, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SpacingXProperty =
        DependencyProperty.Register(nameof(SpacingX), typeof(double), typeof(WatermarkOverlay),
            new FrameworkPropertyMetadata(260.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SpacingYProperty =
        DependencyProperty.Register(nameof(SpacingY), typeof(double), typeof(WatermarkOverlay),
            new FrameworkPropertyMetadata(180.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RotationAngleProperty =
        DependencyProperty.Register(nameof(RotationAngle), typeof(double), typeof(WatermarkOverlay),
            new FrameworkPropertyMetadata(-45.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public string WatermarkText
    {
        get => (string)GetValue(WatermarkTextProperty);
        set => SetValue(WatermarkTextProperty, value);
    }

    public string FontFamilyName
    {
        get => (string)GetValue(FontFamilyNameProperty);
        set => SetValue(FontFamilyNameProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }
    public double OpacityValue
    {
        get => (double)GetValue(OpacityValueProperty);
        set => SetValue(OpacityValueProperty, value);
    }

    public System.Windows.Media.Color TextColor
    {
        get => (System.Windows.Media.Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public double SpacingX
    {
        get => (double)GetValue(SpacingXProperty);
        set => SetValue(SpacingXProperty, value);
    }

    public double SpacingY
    {
        get => (double)GetValue(SpacingYProperty);
        set => SetValue(SpacingYProperty, value);
    }

    public double RotationAngle
    {
        get => (double)GetValue(RotationAngleProperty);
        set => SetValue(RotationAngleProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        double width = ActualWidth;
        double height = ActualHeight;

        if (width <= 0 || height <= 0)
            return;

        var alpha = (byte)Math.Clamp(OpacityValue * 255, 0, 255);
        var color = TextColor;
        var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(alpha, color.R, color.G, color.B));
        brush.Freeze();

        var typeface = new Typeface(
            new FontFamily(FontFamilyName),
            FontStyles.Normal,
            FontWeights.Bold,
            FontStretches.Normal);

        double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var formattedText = new FormattedText(
            WatermarkText,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            brush,
            pixelsPerDip);

        double textWidth = formattedText.Width;
        double textHeight = formattedText.Height;
        double spacingX = SpacingX;
        double spacingY = SpacingY;
        double angle = RotationAngle;

        // Extend the grid far enough so rotated tiles cover the whole screen
        double margin = Math.Max(width, height);

        for (double x = -margin; x < width + margin; x += spacingX)
        {
            for (double y = -margin; y < height + margin; y += spacingY)
            {
                // Rotate around the center of each text instance
                double cx = x + textWidth / 2;
                double cy = y + textHeight / 2;

                drawingContext.PushTransform(new RotateTransform(angle, cx, cy));
                drawingContext.DrawText(formattedText, new Point(x, y));
                drawingContext.Pop();
            }
        }
    }
}
