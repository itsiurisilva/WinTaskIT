namespace WinTaskIT.Native;

/// <summary>Wraps SetWinEventHook/UnhookWinEvent to raise events for two things
/// happening to any top-level window, in any process: starting to minimize, and
/// being destroyed. Out-of-process (WINEVENT_OUTOFCONTEXT) -- no DLL is injected
/// into the target process.</summary>
internal sealed class WinEventWatcher : IDisposable
{
    // Must be kept alive for the hooks' lifetime: if this were only a local
    // delegate, the GC could collect it and the native callback pointer would
    // become invalid.
    private readonly WinEventProc _proc;
    private readonly IntPtr _minimizeHook;
    private readonly IntPtr _destroyHook;

    public event Action<IntPtr>? WindowMinimizeStarted;
    public event Action<IntPtr>? WindowDestroyed;

    public WinEventWatcher()
    {
        _proc = OnWinEvent;
        _minimizeHook = NativeMethods.SetWinEventHook(
            NativeMethods.EVENT_SYSTEM_MINIMIZESTART,
            NativeMethods.EVENT_SYSTEM_MINIMIZESTART,
            IntPtr.Zero,
            _proc,
            0,
            0,
            NativeMethods.WINEVENT_OUTOFCONTEXT | NativeMethods.WINEVENT_SKIPOWNPROCESS);
        _destroyHook = NativeMethods.SetWinEventHook(
            NativeMethods.EVENT_OBJECT_DESTROY,
            NativeMethods.EVENT_OBJECT_DESTROY,
            IntPtr.Zero,
            _proc,
            0,
            0,
            NativeMethods.WINEVENT_OUTOFCONTEXT | NativeMethods.WINEVENT_SKIPOWNPROCESS);
    }

    private void OnWinEvent(
        IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
    {
        if (idObject != NativeMethods.OBJID_WINDOW || idChild != NativeMethods.CHILDID_SELF)
            return;
        if (hwnd == IntPtr.Zero)
            return;

        if (eventType == NativeMethods.EVENT_SYSTEM_MINIMIZESTART)
            WindowMinimizeStarted?.Invoke(hwnd);
        else if (eventType == NativeMethods.EVENT_OBJECT_DESTROY)
            WindowDestroyed?.Invoke(hwnd);
    }

    public void Dispose()
    {
        if (_minimizeHook != IntPtr.Zero)
            NativeMethods.UnhookWinEvent(_minimizeHook);
        if (_destroyHook != IntPtr.Zero)
            NativeMethods.UnhookWinEvent(_destroyHook);
    }
}
