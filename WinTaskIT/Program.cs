using WinTaskIT.Config;
using WinTaskIT.Native;
using WinTaskIT.Startup;

namespace WinTaskIT;

static class Program
{
    // Fixed, arbitrary name -- just needs to be unlikely to collide with another app's mutex.
    private const string SingleInstanceMutexName = "WinTaskIT-SingleInstance-3F1B6C2E-9A47-4E1D-8C2B-6D7A1F0E9B5C";

    [STAThread]
    static void Main(string[] args)
    {
        // Invoked by the installer's uninstaller (before it deletes program files)
        // to undo the same registry/config state the in-app Settings "Uninstall..."
        // button removes. No UI, no mutex/single-instance dance -- this is a
        // distinct, one-shot invocation, not the normal background-app path.
        if (args.Contains("--uninstall-cleanup", StringComparer.OrdinalIgnoreCase))
        {
            RunUninstallCleanup();
            return;
        }

        using var mutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out bool createdNew);
        if (!createdNew)
        {
            // Already running in the background -- ask it to open Settings instead
            // of starting a second instance.
            IntPtr existing = IpcWindow.FindExisting();
            if (existing != IntPtr.Zero)
                NativeMethods.PostMessage(existing, IpcWindow.WM_SHOW_SETTINGS, IntPtr.Zero, IntPtr.Zero);
            return;
        }

        AppPathManager.EnsureRegistered();

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayAppContext());
    }

    private static void RunUninstallCleanup()
    {
        // Best-effort: ask a running background instance to exit first, since
        // WinTaskIT has no persistent tray icon reminding the user it's alive --
        // it's easy to go straight to "Uninstall" from Windows without noticing
        // it's still running. Not a hard guarantee (fire-and-forget message), but
        // Windows also tolerates deleting a file an old instance still has open.
        IntPtr existing = IpcWindow.FindExisting();
        if (existing != IntPtr.Zero)
        {
            NativeMethods.PostMessage(existing, IpcWindow.WM_EXIT_APP, IntPtr.Zero, IntPtr.Zero);
            Thread.Sleep(300);
        }

        StartupManager.SetEnabled(false);
        AppPathManager.Unregister();
        ConfigManager.DeleteAll();
    }
}
