using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace BackgroundChanger;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new TrayAppContext());
    }
}

sealed class TrayAppContext : ApplicationContext
{
    // Win32 API for setting wallpaper
    private const int SPI_SETDESKWALLPAPER = 20;
    private const int SPIF_UPDATEINFILE = 1;
    private const int SPIF_SENDCHANGE = 2;

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    // Image filenames for each time bracket
    private const string ImgDefault = "Default.png";       // 00:00 - 05:59
    private const string ImgDay = "BG.jpg";                     // 06:00 - 17:59
    private const string ImgNight = "BG_Night.jpg";             // 18:00 - 21:59
    private const string ImgNightLightOff = "BG_Night_LightOff.jpg"; // 22:00 - 23:59

    private const string AppName = "BackgroundChanger";
    private const string RunRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    private readonly NotifyIcon _trayIcon;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly ToolStripMenuItem _autoStartItem;
    private string _lastAppliedImage = "";

    public TrayAppContext()
    {
        // Build context menu
        _autoStartItem = new ToolStripMenuItem("Start with Windows")
        {
            Checked = IsAutoStartEnabled(),
            CheckOnClick = true,
        };
        _autoStartItem.Click += OnToggleAutoStart;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Change Now", null, OnChangeNow);
        contextMenu.Items.Add("Open Images Folder", null, OnOpenImagesFolder);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(_autoStartItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Exit", null, OnExit);

        // Create tray icon
        _trayIcon = new NotifyIcon
        {
            Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico")),
            Text = AppName,
            ContextMenuStrip = contextMenu,
            Visible = true,
        };

        // Apply wallpaper immediately on launch
        ApplyWallpaper(force: true);

        // Timer: check every 5 minutes
        _timer = new System.Windows.Forms.Timer { Interval = 5 * 60 * 1000 };
        _timer.Tick += (_, _) => ApplyWallpaper(force: false);
        _timer.Start();
    }

    /// <summary>
    /// Determines the correct wallpaper image name based on the current hour.
    /// </summary>
    private static string GetImageForCurrentTime()
    {
        int hour = DateTime.Now.Hour;
        return hour switch
        {
            >= 22 => ImgNightLightOff,
            >= 18 => ImgNight,
            >= 6 => ImgDay,
            _ => ImgDefault,
        };
    }

    /// <summary>
    /// Resolves the full path to a wallpaper image in the .bg folder next to the exe.
    /// </summary>
    private static string GetImagePath(string imageName)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(baseDir, ".bg", imageName);
    }

    /// <summary>
    /// Sets the desktop wallpaper if the time bracket has changed (or if forced).
    /// </summary>
    private void ApplyWallpaper(bool force)
    {
        string imageName = GetImageForCurrentTime();

        // Skip if we already applied this image (unless forced)
        if (!force && imageName == _lastAppliedImage)
            return;

        string imagePath = GetImagePath(imageName);

        if (!File.Exists(imagePath))
        {
            _trayIcon.ShowBalloonTip(3000, AppName,
                $"Image not found: {imagePath}", ToolTipIcon.Warning);
            return;
        }

        int result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath,
            SPIF_UPDATEINFILE | SPIF_SENDCHANGE);

        if (result != 0)
        {
            _lastAppliedImage = imageName;
        }
        else
        {
            _trayIcon.ShowBalloonTip(3000, AppName,
                "Failed to set wallpaper.", ToolTipIcon.Error);
        }
    }

    // --- Context menu handlers ---

    private void OnChangeNow(object? sender, EventArgs e)
    {
        ApplyWallpaper(force: true);
    }

    private void OnOpenImagesFolder(object? sender, EventArgs e)
    {
        string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".bg");
        if (Directory.Exists(folder))
        {
            System.Diagnostics.Process.Start("explorer.exe", folder);
        }
        else
        {
            _trayIcon.ShowBalloonTip(3000, AppName,
                $"Folder not found: {folder}\nCreate a '.bg' folder next to the exe and put your wallpaper images in it.",
                ToolTipIcon.Warning);
        }
    }

    private void OnToggleAutoStart(object? sender, EventArgs e)
    {
        if (_autoStartItem.Checked)
            EnableAutoStart();
        else
            DisableAutoStart();
    }

    private void OnExit(object? sender, EventArgs e)
    {
        _timer.Stop();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }

    // --- Auto-start (Registry) helpers ---

    private static string GetExePath()
    {
        return Environment.ProcessPath ?? Application.ExecutablePath;
    }

    private static bool IsAutoStartEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
        return key?.GetValue(AppName) is string val
            && val.Equals(GetExePath(), StringComparison.OrdinalIgnoreCase);
    }

    private static void EnableAutoStart()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
        key?.SetValue(AppName, GetExePath());
    }

    private static void DisableAutoStart()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
        key?.DeleteValue(AppName, false);
    }
}
