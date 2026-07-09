using WinTaskIT.Core;
using WinTaskIT.Native;
using WinTaskIT.UI;

namespace WinTaskIT;

/// <summary>Runs invisibly in the background -- no persistent tray icon.
/// Per-app tray icons still appear via TrayedWindowManager when a configured
/// window is minimized. Settings is reached by re-launching the exe (Win+R
/// "wintaskit"), which IpcWindow picks up as a request to open it here instead
/// of starting a second background instance.</summary>
sealed class TrayAppContext : ApplicationContext
{
    private readonly TrayedWindowManager _trayedWindowManager;
    private readonly IpcWindow _ipcWindow;

    public TrayAppContext()
    {
        _trayedWindowManager = new TrayedWindowManager();

        _ipcWindow = new IpcWindow();
        _ipcWindow.ShowSettingsRequested += OnShowSettingsRequested;
        _ipcWindow.ExitRequested += () => Application.Exit();
    }

    private void OnShowSettingsRequested()
    {
        using var form = new SettingsForm();
        form.ShowDialog();
        _trayedWindowManager.ReloadConfig();
    }

    protected override void ExitThreadCore()
    {
        _ipcWindow.Dispose();
        _trayedWindowManager.Dispose();
        base.ExitThreadCore();
    }
}
