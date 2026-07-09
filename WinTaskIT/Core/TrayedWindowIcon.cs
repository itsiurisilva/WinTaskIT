using Windows.Media.Control;
using WinTaskIT.Native;

namespace WinTaskIT.Core;

/// <summary>One NotifyIcon + context menu wrapping a single hidden window.</summary>
internal sealed class TrayedWindowIcon : IDisposable
{
    public IntPtr Hwnd { get; }
    public string Aumid { get; }

    private readonly NotifyIcon _icon;
    private readonly ToolStripMenuItem _playPauseItem;

    public event Action<TrayedWindowIcon>? RestoreRequested;
    public event Action<TrayedWindowIcon>? RemoveRequested;

    public TrayedWindowIcon(IntPtr hwnd, string aumid, string displayName)
    {
        Hwnd = hwnd;
        Aumid = aumid;

        var menu = new ContextMenuStrip();
        menu.Items.Add("Restore", null, (_, _) => RestoreRequested?.Invoke(this));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("⏮ Previous", null, async (_, _) => await MediaSessionUtil.SkipPreviousAsync(Aumid));
        _playPauseItem = new ToolStripMenuItem("⏯ Play/Pause", null,
            async (_, _) => await MediaSessionUtil.TogglePlayPauseAsync(Aumid));
        menu.Items.Add(_playPauseItem);
        menu.Items.Add("⏭ Next", null, async (_, _) => await MediaSessionUtil.SkipNextAsync(Aumid));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Close", null, (_, _) =>
            NativeMethods.PostMessage(Hwnd, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero));
        menu.Items.Add("Remove from tray list", null, (_, _) => RemoveRequested?.Invoke(this));
        menu.Opening += OnMenuOpening;

        _icon = new NotifyIcon
        {
            Icon = WindowIconUtil.GetIconFor(hwnd),
            Text = Truncate(displayName, 63), // NotifyIcon.Text is capped at 63 chars
            Visible = true,
            ContextMenuStrip = menu,
        };
        _icon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                RestoreRequested?.Invoke(this);
        };
    }

    private void OnMenuOpening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Blocking here (not async) so the label is correct the moment the menu
        // shows, rather than flashing a stale label for a frame.
        var status = MediaSessionUtil.GetStatusAsync(Aumid).GetAwaiter().GetResult();
        _playPauseItem.Text = status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing
            ? "⏸ Pause"
            : "▶ Play";
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
