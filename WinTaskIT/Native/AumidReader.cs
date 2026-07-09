using System.Runtime.InteropServices;
using System.Text;

namespace WinTaskIT.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct PROPERTYKEY
{
    public Guid fmtid;
    public uint pid;
}

// Sized to match native PROPVARIANT (24 bytes on x64 / 16 bytes on x86) so the
// native GetValue call never writes past the end of this struct on the stack.
[StructLayout(LayoutKind.Sequential)]
internal struct PROPVARIANT
{
    public ushort vt;
    public ushort wReserved1;
    public ushort wReserved2;
    public ushort wReserved3;
    public IntPtr data1;
    public IntPtr data2;
}

// Vtable order must exactly match the native IPropertyStore (Propsys.h) past the
// implicit IUnknown slots: GetCount, GetAt, GetValue, SetValue, Commit. Only
// GetValue is actually called, but the earlier slots must still be declared in
// order or GetValue would be invoked through the wrong vtable offset.
[ComImport]
[Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPropertyStore
{
    void GetCount(out uint cProps);
    void GetAt(uint iProp, out PROPERTYKEY pkey);
    void GetValue(ref PROPERTYKEY key, out PROPVARIANT pv);
    void SetValue(ref PROPERTYKEY key, ref PROPVARIANT propvar);
    void Commit();
}

/// <summary>Reads a window's AppUserModelID (the id Windows/Chrome use to tell an
/// installed web app's window apart from a plain browser window) via the Shell
/// property system. No process injection or elevation required.</summary>
internal static class AumidReader
{
    private static readonly Guid IID_IPropertyStore = new("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99");

    private static readonly PROPERTYKEY PKEY_AppUserModel_ID = new()
    {
        fmtid = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"),
        pid = 5,
    };

    private const ushort VT_EMPTY = 0;

    [DllImport("shell32.dll")]
    private static extern int SHGetPropertyStoreForWindow(IntPtr hwnd, ref Guid riid, out IPropertyStore? propertyStore);

    [DllImport("propsys.dll", CharSet = CharSet.Unicode)]
    private static extern int PropVariantToString(ref PROPVARIANT propvar, StringBuilder psz, int cch);

    [DllImport("ole32.dll")]
    private static extern int PropVariantClear(ref PROPVARIANT pvar);

    /// <summary>Returns the window's AppUserModelID, or null if it doesn't have one.</summary>
    public static string? GetAppUserModelId(IntPtr hwnd)
    {
        IPropertyStore? store = null;
        try
        {
            var riid = IID_IPropertyStore;
            int hr = SHGetPropertyStoreForWindow(hwnd, ref riid, out store);
            if (hr != 0 || store is null)
                return null;

            var key = PKEY_AppUserModel_ID;
            PROPVARIANT value;
            try
            {
                store.GetValue(ref key, out value);
            }
            catch (COMException)
            {
                return null;
            }

            try
            {
                if (value.vt == VT_EMPTY)
                    return null;

                var sb = new StringBuilder(512);
                int strHr = PropVariantToString(ref value, sb, sb.Capacity);
                return strHr == 0 ? sb.ToString() : null;
            }
            finally
            {
                PropVariantClear(ref value);
            }
        }
        finally
        {
            if (store is not null)
                Marshal.ReleaseComObject(store);
        }
    }
}
