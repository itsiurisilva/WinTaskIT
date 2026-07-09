namespace WinTaskIT.Native;

/// <summary>A hidden, never-shown top-level window that exists purely so a second
/// launch of WinTaskIT can find it (via FindWindow, by its fixed title) and tell
/// it to open Settings, instead of starting a duplicate background instance.</summary>
internal sealed class IpcWindow : System.Windows.Forms.NativeWindow, IDisposable
{
    public const string WindowTitle = "WinTaskIT_IpcWindow";

    // Guaranteed to resolve to the same id in every process that registers this
    // exact string -- the standard way to define a private cross-process message.
    public static readonly uint WM_SHOW_SETTINGS = NativeMethods.RegisterWindowMessage("WinTaskIT_ShowSettings");
    public static readonly uint WM_EXIT_APP = NativeMethods.RegisterWindowMessage("WinTaskIT_Exit");

    public event Action? ShowSettingsRequested;
    public event Action? ExitRequested;

    public IpcWindow()
    {
        var cp = new System.Windows.Forms.CreateParams
        {
            Caption = WindowTitle,
        };
        CreateHandle(cp);
    }

    protected override void WndProc(ref System.Windows.Forms.Message m)
    {
        if (m.Msg == WM_SHOW_SETTINGS)
            ShowSettingsRequested?.Invoke();
        else if (m.Msg == WM_EXIT_APP)
            ExitRequested?.Invoke();
        base.WndProc(ref m);
    }

    /// <summary>Finds the running instance's IPC window, if any.</summary>
    public static IntPtr FindExisting() => NativeMethods.FindWindow(null, WindowTitle);

    public void Dispose() => DestroyHandle();
}
