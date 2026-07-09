using Microsoft.Win32;

namespace WinTaskIT.Startup;

/// <summary>Registers WinTaskIT.exe under the standard per-user "App Paths" key
/// so typing "wintaskit" in Win+R resolves to it, wherever it's installed. No
/// admin rights needed (HKEY_CURRENT_USER).</summary>
internal static class AppPathManager
{
    private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\App Paths\wintaskit.exe";

    public static void EnsureRegistered()
    {
        string exePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Could not determine the running executable's path.");

        using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
        key.SetValue(null, exePath);
        key.SetValue("Path", Path.GetDirectoryName(exePath) ?? "");
    }

    /// <summary>Removes the App Paths registration. Used by Uninstall.</summary>
    public static void Unregister() =>
        Registry.CurrentUser.DeleteSubKeyTree(KeyPath, throwOnMissingSubKey: false);
}
