namespace WinTaskIT.Config;

internal sealed class AppSettings
{
    public int Version { get; set; } = 1;
    public bool RunAtStartup { get; set; }
    public List<ConfiguredApp> Apps { get; set; } = new();
}

/// <summary>First member must stay the default (0) so configs saved before this
/// setting existed -- which have no "mode" key at all -- deserialize to today's
/// original behavior with no migration step needed.</summary>
internal enum TrayMode
{
    OnlyWhilePlayingAudio,
    Always,
}

internal sealed class ConfiguredApp
{
    public string Aumid { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public TrayMode Mode { get; set; } = TrayMode.OnlyWhilePlayingAudio;
}
