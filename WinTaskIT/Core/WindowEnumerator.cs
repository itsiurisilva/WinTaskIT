using System.Text;
using WinTaskIT.Native;

namespace WinTaskIT.Core;

internal static class WindowEnumerator
{
    /// <summary>Lists visible, non-owned top-level windows with a title, each with
    /// its resolved AppUserModelID (null if it doesn't have one).</summary>
    public static List<WindowInfo> GetTopLevelWindows()
    {
        var results = new List<WindowInfo>();

        bool Callback(IntPtr hwnd, IntPtr lParam)
        {
            if (!NativeMethods.IsWindowVisible(hwnd))
                return true;
            if (NativeMethods.GetWindow(hwnd, NativeMethods.GW_OWNER) != IntPtr.Zero)
                return true;

            int len = NativeMethods.GetWindowTextLength(hwnd);
            if (len == 0)
                return true;

            var sb = new StringBuilder(len + 1);
            NativeMethods.GetWindowText(hwnd, sb, sb.Capacity);

            string? aumid = AumidReader.GetAppUserModelId(hwnd);
            results.Add(new WindowInfo(hwnd, sb.ToString(), aumid));
            return true;
        }

        NativeMethods.EnumWindows(Callback, IntPtr.Zero);
        return results;
    }
}
