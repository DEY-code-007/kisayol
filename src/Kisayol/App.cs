using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Kisayol;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        using var mutex = new Mutex(true, "Kisayol.SingleInstance", out bool first);
        if (!first) { NativeMethods.BroadcastShow(); return; }
        var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
        app.Run(new MainWindow());
    }
}

public class Settings
{
    public bool Topmost { get; set; } = true;
    public double Left { get; set; } = double.NaN;
    public double Top { get; set; } = double.NaN;
    public double Width { get; set; } = 440;
    public double Height { get; set; } = 680;

    static string Dir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Kisayol");
    static string FilePath => Path.Combine(Dir, "settings.json");

    public static Settings Load()
    {
        try { if (File.Exists(FilePath)) return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new Settings(); }
        catch { }
        return new Settings();
    }
    public void Save()
    {
        try { Directory.CreateDirectory(Dir); File.WriteAllText(FilePath, JsonSerializer.Serialize(this)); } catch { }
    }
    public static string WebViewData => Path.Combine(Dir, "WebView2");
}

public class MainWindow : Window
{
    const int HOTKEY_ID = 0x4B53; // "KS"
    readonly WebView2 web = new();
    readonly Settings settings = Settings.Load();
    readonly DispatcherTimer watch = new() { Interval = TimeSpan.FromMilliseconds(700) };
    string lastApp = "";
    HwndSource source;
    readonly int ownPid = Environment.ProcessId;

    public MainWindow()
    {
        Title = "Kısayol";
        Width = settings.Width; Height = settings.Height;
        MinWidth = 340; MinHeight = 420;
        Topmost = settings.Topmost;
        Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x14, 0x15, 0x1a));
        if (!double.IsNaN(settings.Left) && !double.IsNaN(settings.Top) && OnScreen(settings.Left, settings.Top))
        { WindowStartupLocation = WindowStartupLocation.Manual; Left = settings.Left; Top = settings.Top; }
        else WindowStartupLocation = WindowStartupLocation.CenterScreen;
        try { Icon = BitmapFrame.Create(new Uri("pack://application:,,,/app.ico")); } catch { }
        Content = web;
        Loaded += async (_, _) => await InitWeb();
        SourceInitialized += (_, _) => HookHotkey();
        Closing += (_, _) => SaveState();
        watch.Tick += (_, _) => CheckForeground();
    }

    static bool OnScreen(double l, double t) =>
        l > SystemParameters.VirtualScreenLeft - 50 && t > SystemParameters.VirtualScreenTop - 50 &&
        l < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 100 &&
        t < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 100;

    async Task InitWeb()
    {
        try
        {
            var env = await CoreWebView2Environment.CreateAsync(null, Settings.WebViewData);
            await web.EnsureCoreWebView2Async(env);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show("Bu uygulama Microsoft Edge WebView2 bileşenine ihtiyaç duyuyor. Windows Update ile ya da Microsoft'un sitesinden yükleyip tekrar deneyin.", "Kısayol");
            Close(); return;
        }
        var core = web.CoreWebView2;
        var webDir = Path.Combine(AppContext.BaseDirectory, "web");
        core.SetVirtualHostNameToFolderMapping("kisayol.app", webDir, CoreWebView2HostResourceAccessKind.Allow);
        core.Settings.AreDevToolsEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.AreDefaultContextMenusEnabled = false;
        core.WebMessageReceived += OnMessage;
        core.NavigationCompleted += (_, _) =>
        {
            Post(new { type = "init", topmost = Topmost, hotkey = "Ctrl+Alt+K" });
            lastApp = ""; CheckForeground();
            watch.Start();
        };
        core.NewWindowRequested += (s, e) => { e.Handled = true; OpenUrl(e.Uri); };
        web.Source = new Uri("https://kisayol.app/index.html");
    }

    static void OpenUrl(string url)
    {
        if (url.StartsWith("https://")) try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
    }

    void Post(object o)
    {
        try { web.CoreWebView2?.PostWebMessageAsJson(JsonSerializer.Serialize(o)); } catch { }
    }

    void OnMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using var doc = JsonDocument.Parse(e.WebMessageAsJson);
            var type = doc.RootElement.GetProperty("type").GetString();
            if (type == "topmost")
            {
                Topmost = doc.RootElement.GetProperty("value").GetBoolean();
                settings.Topmost = Topmost; settings.Save();
            }
            else if (type == "hide") WindowState = WindowState.Minimized;
        }
        catch { }
    }

    void CheckForeground()
    {
        var h = NativeMethods.GetForegroundWindow();
        if (h == IntPtr.Zero) return;
        NativeMethods.GetWindowThreadProcessId(h, out uint pid);
        if (pid == 0 || pid == ownPid) return;
        string name;
        try { name = Process.GetProcessById((int)pid).ProcessName; } catch { return; }
        string app = name.Equals("Photoshop", StringComparison.OrdinalIgnoreCase) ? "ps"
                   : name.Equals("Illustrator", StringComparison.OrdinalIgnoreCase) ? "ai" : "";
        if (app != "" && app != lastApp) { lastApp = app; Post(new { type = "app", app }); }
    }

    void HookHotkey()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        source = HwndSource.FromHwnd(hwnd);
        source.AddHook(WndProc);
        bool ok = NativeMethods.RegisterHotKey(hwnd, HOTKEY_ID, NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT | NativeMethods.MOD_NOREPEAT, 0x4B);
        if (!ok) Dispatcher.BeginInvoke(() => Post(new { type = "hotkeyFailed" }), DispatcherPriority.ApplicationIdle);
        Closed += (_, _) => NativeMethods.UnregisterHotKey(hwnd, HOTKEY_ID);
    }

    IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID) { Toggle(); handled = true; }
        else if (msg == NativeMethods.WM_SHOWME) { ShowFront(); handled = true; }
        return IntPtr.Zero;
    }

    void Toggle()
    {
        if (WindowState == WindowState.Minimized || !IsActive) ShowFront();
        else WindowState = WindowState.Minimized;
    }

    void ShowFront()
    {
        if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
        Show(); Activate();
        var t = Topmost; Topmost = true; Topmost = t;
        Post(new { type = "focusSearch" });
    }

    void SaveState()
    {
        if (WindowState == WindowState.Normal)
        { settings.Left = Left; settings.Top = Top; settings.Width = Width; settings.Height = Height; }
        settings.Topmost = Topmost;
        settings.Save();
    }
}

static class NativeMethods
{
    public const int WM_HOTKEY = 0x0312;
    public const uint MOD_ALT = 0x1, MOD_CONTROL = 0x2, MOD_NOREPEAT = 0x4000;
    public static readonly int WM_SHOWME = RegisterWindowMessage("Kisayol.ShowMe");
    static readonly IntPtr HWND_BROADCAST = new(0xffff);

    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);
    [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint mods, uint vk);
    [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int RegisterWindowMessage(string msg);
    [DllImport("user32.dll")] static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr w, IntPtr l);
    public static void BroadcastShow() => PostMessage(HWND_BROADCAST, WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
}
