using WinTaskIT.Core;

namespace WinTaskIT.UI;

/// <summary>Modal picker listing currently open windows that have an
/// AppUserModelID (only those can be matched on minimize). Returns the chosen
/// window, or null if cancelled.</summary>
internal sealed class WindowPickerForm : Form
{
    private readonly ListBox _list = new() { Dock = DockStyle.Fill, IntegralHeight = false };
    private readonly List<WindowInfo> _windows;

    private WindowPickerForm()
    {
        Text = "Add app window";
        Width = 480;
        Height = 360;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;

        _windows = WindowEnumerator.GetTopLevelWindows()
            .Where(w => w.Aumid is not null)
            .ToList();

        foreach (var w in _windows)
            _list.Items.Add($"{w.Title}  ({w.Aumid})");

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            FlowDirection = FlowDirection.RightToLeft,
        };
        var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        var okButton = new Button { Text = "Add", DialogResult = DialogResult.OK, AutoSize = true };
        buttonPanel.Controls.Add(cancelButton);
        buttonPanel.Controls.Add(okButton);

        AcceptButton = okButton;
        CancelButton = cancelButton;

        Controls.Add(_list);
        Controls.Add(buttonPanel);
    }

    public static WindowInfo? PickWindow(IWin32Window owner)
    {
        using var form = new WindowPickerForm();
        if (form._windows.Count == 0)
        {
            MessageBox.Show(owner,
                "No open windows currently expose an AppUserModelID. Open the web app " +
                "(e.g. the installed YouTube window) first, then try again.",
                "Add app window", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        form._list.SelectedIndex = 0;
        return form.ShowDialog(owner) == DialogResult.OK && form._list.SelectedIndex >= 0
            ? form._windows[form._list.SelectedIndex]
            : null;
    }
}
