using Windows.Media.Control;

namespace WinTaskIT.Core;

/// <summary>Wraps GlobalSystemMediaTransportControlsSessionManager -- the same
/// system that powers media keys and the volume flyout's "now playing" widget --
/// to find the media session for a configured app (matched by AUMID) and check
/// or control its playback. This is what lets us tell a silent window apart from
/// one actually playing audio, since that isn't visible from the window itself.</summary>
internal static class MediaSessionUtil
{
    public static async Task<bool> HasPlayingSessionAsync(string aumid)
    {
        var session = await FindBestSessionAsync(aumid).ConfigureAwait(false);
        return session is not null &&
            session.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
    }

    public static async Task<GlobalSystemMediaTransportControlsSessionPlaybackStatus?> GetStatusAsync(string aumid)
    {
        var session = await FindBestSessionAsync(aumid).ConfigureAwait(false);
        return session?.GetPlaybackInfo().PlaybackStatus;
    }

    public static async Task TogglePlayPauseAsync(string aumid)
    {
        var session = await FindBestSessionAsync(aumid).ConfigureAwait(false);
        if (session is not null)
            await session.TryTogglePlayPauseAsync().AsTask().ConfigureAwait(false);
    }

    public static async Task SkipNextAsync(string aumid)
    {
        var session = await FindBestSessionAsync(aumid).ConfigureAwait(false);
        if (session is not null)
            await session.TrySkipNextAsync().AsTask().ConfigureAwait(false);
    }

    public static async Task SkipPreviousAsync(string aumid)
    {
        var session = await FindBestSessionAsync(aumid).ConfigureAwait(false);
        if (session is not null)
            await session.TrySkipPreviousAsync().AsTask().ConfigureAwait(false);
    }

    /// <summary>Finds the session for this AUMID -- prefers an actively playing
    /// one (relevant when more than one window shares the same AUMID), falling
    /// back to a paused one so Play still works from the tray menu.
    /// ConfigureAwait(false) throughout: callers on the UI thread (the WinEvent
    /// hook callback, the tray menu's Opening handler) block on this synchronously
    /// via .GetAwaiter().GetResult(), and capturing the UI SynchronizationContext
    /// here would deadlock that thread waiting on its own blocked message loop.</summary>
    private static async Task<GlobalSystemMediaTransportControlsSession?> FindBestSessionAsync(string aumid)
    {
        var manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync().AsTask().ConfigureAwait(false);
        var matches = manager.GetSessions()
            .Where(s => string.Equals(s.SourceAppUserModelId, aumid, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matches.FirstOrDefault(s =>
                s.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
            ?? matches.FirstOrDefault();
    }
}
