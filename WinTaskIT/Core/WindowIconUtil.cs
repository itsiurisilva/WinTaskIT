using System.Text;
using WinTaskIT.Native;

namespace WinTaskIT.Core;

internal static class WindowIconUtil
{
    /// <summary>Best-effort icon for a window: prefers the window's own icon
    /// (e.g. a Chrome-installed web app's site icon), falling back to its exe's
    /// file icon, then a generic system icon. Every return path is an
    /// independently-owned Icon the caller must Dispose.</summary>
    public static Icon GetIconFor(IntPtr hwnd)
    {
        IntPtr hIcon = NativeMethods.SendMessage(hwnd, NativeMethods.WM_GETICON, (IntPtr)NativeMethods.ICON_BIG, IntPtr.Zero);
        if (hIcon == IntPtr.Zero)
            hIcon = NativeMethods.SendMessage(hwnd, NativeMethods.WM_GETICON, (IntPtr)NativeMethods.ICON_SMALL2, IntPtr.Zero);
        if (hIcon == IntPtr.Zero)
            hIcon = NativeMethods.GetClassLongPtr(hwnd, NativeMethods.GCL_HICON);
        if (hIcon == IntPtr.Zero)
            hIcon = NativeMethods.GetClassLongPtr(hwnd, NativeMethods.GCL_HICONSM);

        if (hIcon != IntPtr.Zero)
        {
            try
            {
                // Clone so the NotifyIcon owns an independent copy -- Icon.FromHandle
                // wraps the window's handle without taking ownership of it.
                return (Icon)Icon.FromHandle(hIcon).Clone();
            }
            catch (ArgumentException)
            {
                // Fall through to exe icon.
            }
        }

        string? exePath = GetExePath(hwnd);
        if (exePath is not null)
        {
            try
            {
                var exeIcon = Icon.ExtractAssociatedIcon(exePath);
                if (exeIcon is not null)
                    return exeIcon;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Fall through to generic icon.
            }
        }

        // Clone -- SystemIcons instances are process-wide shared/cached; disposing
        // the shared instance itself would invalidate it for every other caller.
        return (Icon)SystemIcons.Application.Clone();
    }

    private static string? GetExePath(IntPtr hwnd)
    {
        NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
        IntPtr hProcess = NativeMethods.OpenProcess(NativeMethods.PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
        if (hProcess == IntPtr.Zero)
            return null;

        try
        {
            var sb = new StringBuilder(1024);
            uint size = (uint)sb.Capacity;
            return NativeMethods.QueryFullProcessImageName(hProcess, 0, sb, ref size) ? sb.ToString() : null;
        }
        finally
        {
            NativeMethods.CloseHandle(hProcess);
        }
    }
}
