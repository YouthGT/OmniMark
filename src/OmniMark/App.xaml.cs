using System.Windows;

namespace OmniMark;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var settings = WatermarkSettings.Load();
        _mainWindow = new MainWindow(settings);
        _mainWindow.Show();

        InitializeTrayIcon();
    }

    private void InitializeTrayIcon()
    {
        var iconStream = GetResourceStream(new Uri("pack://application:,,,/Resources/app.ico"))?.Stream;
        var icon = iconStream != null
            ? new System.Drawing.Icon(iconStream)
            : System.Drawing.SystemIcons.Information;

        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = icon,
            Visible = true,
            Text = "OmniMark – Desktop Watermark"
        };

        var contextMenu = new System.Windows.Forms.ContextMenuStrip();

        var settingsItem = new System.Windows.Forms.ToolStripMenuItem("Settings...");
        settingsItem.Click += (s, e) => ShowSettings();

        var exitItem = new System.Windows.Forms.ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => Shutdown();

        contextMenu.Items.Add(settingsItem);
        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (s, e) => ShowSettings();
    }

    private void ShowSettings()
    {
        if (_mainWindow == null)
            return;

        var settingsWindow = new SettingsWindow(_mainWindow)
        {
            Owner = _mainWindow
        };
        settingsWindow.ShowDialog();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _notifyIcon?.Dispose();
        base.OnExit(e);
    }
}

