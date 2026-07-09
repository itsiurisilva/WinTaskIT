using WinTaskIT.Config;
using WinTaskIT.Native;

namespace WinTaskIT.Core;

/// <summary>Orchestrator: on minimize, hides windows matching the configured app
/// list and shows a tray icon for each one hidden.</summary>
internal sealed class TrayedWindowManager : IDisposable
{
    private readonly WinEventWatcher _watcher;
    private readonly Dictionary<IntPtr, TrayedWindowIcon> _hidden = new();
    private AppSettings _settings;

    public TrayedWindowManager()
    {
        _settings = ConfigManager.Load();
        _watcher = new WinEventWatcher();
        _watcher.WindowMinimizeStarted += OnWindowMinimizeStarted;
        _watcher.WindowDestroyed += OnWindowDestroyed;
    }

    /// <summary>Re-reads config.json; call after Settings changes are saved.</summary>
    public void ReloadConfig() => _settings = ConfigManager.Load();

    private void OnWindowMinimizeStarted(IntPtr hwnd)
    {
        if (_hidden.ContainsKey(hwnd))
            return;

        string? aumid = AumidReader.GetAppUserModelId(hwnd);
        var match = _settings.Apps.FirstOrDefault(a =>
            a.Enabled && string.Equals(a.Aumid, aumid, StringComparison.OrdinalIgnoreCase));
        if (match is null)
            return;

        // Only ever tray one window per configured app at a time (e.g. only one
        // YouTube tray icon even if two YouTube windows are open).
        if (_hidden.Values.Any(t => string.Equals(t.Aumid, match.Aumid, StringComparison.OrdinalIgnoreCase)))
            return;

        // Audio-gated apps only tray while actually producing sound -- if there
        // are two windows for the same app and this one is silent (paused/muted),
        // leave it to minimize normally instead of hijacking it. "Always" apps
        // skip this check entirely and tray unconditionally.
        if (match.Mode == TrayMode.OnlyWhilePlayingAudio)
        {
            bool isPlaying = MediaSessionUtil.HasPlayingSessionAsync(match.Aumid).GetAwaiter().GetResult();
            if (!isPlaying)
                return;
        }

        NativeMethods.ShowWindow(hwnd, NativeMethods.SW_HIDE);

        var trayed = new TrayedWindowIcon(hwnd, match.Aumid, match.DisplayName);
        trayed.RestoreRequested += OnRestoreRequested;
        trayed.RemoveRequested += OnRemoveRequested;
        _hidden[hwnd] = trayed;
    }

    private void OnRestoreRequested(TrayedWindowIcon trayed)
    {
        NativeMethods.ShowWindow(trayed.Hwnd, NativeMethods.SW_RESTORE);
        NativeMethods.SetForegroundWindow(trayed.Hwnd);
        RemoveTracking(trayed);
    }

    private void OnRemoveRequested(TrayedWindowIcon trayed)
    {
        // Restore visibility first so the user doesn't lose access to the
        // window, then drop it from the configured app list.
        NativeMethods.ShowWindow(trayed.Hwnd, NativeMethods.SW_RESTORE);

        var entry = _settings.Apps.FirstOrDefault(a =>
            string.Equals(a.Aumid, trayed.Aumid, StringComparison.OrdinalIgnoreCase));
        if (entry is not null)
        {
            _settings.Apps.Remove(entry);
            ConfigManager.Save(_settings);
        }

        RemoveTracking(trayed);
    }

    private void OnWindowDestroyed(IntPtr hwnd)
    {
        if (_hidden.TryGetValue(hwnd, out var trayed))
            RemoveTracking(trayed);
    }

    private void RemoveTracking(TrayedWindowIcon trayed)
    {
        _hidden.Remove(trayed.Hwnd);
        trayed.Dispose();
    }

    public void Dispose()
    {
        _watcher.Dispose();
        foreach (var trayed in _hidden.Values)
            trayed.Dispose();
        _hidden.Clear();
    }
}
