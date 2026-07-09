using WinTaskIT.Config;
using WinTaskIT.Startup;

namespace WinTaskIT.UI;

internal sealed class SettingsForm : Form
{
    private readonly ListView _list = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        CheckBoxes = true,
    };

    private readonly ContextMenuStrip _modeMenu = new();
    private readonly ToolStripMenuItem _alwaysItem = new("Always send to tray");
    private readonly ToolStripMenuItem _onlyPlayingItem = new("Only while playing audio");

    private readonly CheckBox _runAtStartupCheckBox = new()
    {
        Text = "Run at Windows startup",
        Dock = DockStyle.Top,
        AutoSize = true,
        Padding = new Padding(6),
    };

    private AppSettings _settings = ConfigManager.Load();

    public SettingsForm()
    {
        Text = "WinTaskIT settings";
        Width = 700;
        Height = 420;
        MinimumSize = new Size(600, 360);
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;

        _list.Columns.Add("App", 220);
        _list.Columns.Add("AppUserModelID", 240);
        _list.Columns.Add("Tray behavior", 160);

        _alwaysItem.Click += (_, _) => ApplyModeToSelection(TrayMode.Always);
        _onlyPlayingItem.Click += (_, _) => ApplyModeToSelection(TrayMode.OnlyWhilePlayingAudio);
        _modeMenu.Items.Add(_alwaysItem);
        _modeMenu.Items.Add(_onlyPlayingItem);
        _modeMenu.Opening += OnModeMenuOpening;
        _list.ContextMenuStrip = _modeMenu;
        _list.MouseUp += OnListMouseUp;

        // Two separate rows (not one shared row) so the button groups can never
        // compete for the same horizontal space and clip each other, regardless
        // of DPI scaling or button text length.
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
        };
        var closeButton = new Button { Text = "Close", DialogResult = DialogResult.OK, AutoSize = true };
        var removeButton = new Button { Text = "Remove", AutoSize = true };
        var addButton = new Button { Text = "Add from open windows...", AutoSize = true };
        addButton.Click += OnAddClicked;
        removeButton.Click += OnRemoveClicked;
        buttonPanel.Controls.Add(closeButton);
        buttonPanel.Controls.Add(removeButton);
        buttonPanel.Controls.Add(addButton);

        var exitPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
        };
        var exitButton = new Button { Text = "Exit WinTaskIT", AutoSize = true };
        exitButton.Click += (_, _) => Application.Exit();
        var uninstallButton = new Button { Text = "Uninstall...", AutoSize = true };
        uninstallButton.Click += OnUninstallClicked;
        exitPanel.Controls.Add(exitButton);
        exitPanel.Controls.Add(uninstallButton);

        AcceptButton = closeButton;
        CancelButton = closeButton;

        Controls.Add(_list);
        Controls.Add(_runAtStartupCheckBox);
        Controls.Add(buttonPanel);
        Controls.Add(exitPanel);

        _list.ItemChecked += OnItemChecked;
        _runAtStartupCheckBox.Checked = _settings.RunAtStartup;
        _runAtStartupCheckBox.CheckedChanged += OnRunAtStartupChanged;

        PopulateList();
    }

    private void OnRunAtStartupChanged(object? sender, EventArgs e)
    {
        _settings.RunAtStartup = _runAtStartupCheckBox.Checked;
        StartupManager.SetEnabled(_settings.RunAtStartup);
        ConfigManager.Save(_settings);
    }

    private void PopulateList()
    {
        _list.ItemChecked -= OnItemChecked;
        _list.Items.Clear();
        foreach (var app in _settings.Apps)
        {
            var item = new ListViewItem(app.DisplayName) { Checked = app.Enabled, Tag = app };
            item.SubItems.Add(app.Aumid);
            item.SubItems.Add(ModeDisplayText(app.Mode));
            _list.Items.Add(item);
        }
        _list.ItemChecked += OnItemChecked;
    }

    private static string ModeDisplayText(TrayMode mode) => mode switch
    {
        TrayMode.Always => "Always send to tray",
        TrayMode.OnlyWhilePlayingAudio => "Only while playing audio",
        _ => mode.ToString(),
    };

    private void OnItemChecked(object? sender, ItemCheckedEventArgs e)
    {
        if (e.Item.Tag is ConfiguredApp app)
        {
            app.Enabled = e.Item.Checked;
            ConfigManager.Save(_settings);
        }
    }

    private void OnListMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
            return;

        var hit = _list.HitTest(e.Location);
        if (hit.Item is not null && !hit.Item.Selected)
        {
            _list.SelectedItems.Clear();
            hit.Item.Selected = true;
        }
    }

    private void OnModeMenuOpening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_list.SelectedItems.Count == 0)
        {
            e.Cancel = true;
            return;
        }

        _alwaysItem.Checked = false;
        _onlyPlayingItem.Checked = false;
        if (_list.SelectedItems.Count == 1 && _list.SelectedItems[0].Tag is ConfiguredApp app)
        {
            _alwaysItem.Checked = app.Mode == TrayMode.Always;
            _onlyPlayingItem.Checked = app.Mode == TrayMode.OnlyWhilePlayingAudio;
        }
    }

    private void ApplyModeToSelection(TrayMode mode)
    {
        if (_list.SelectedItems.Count == 0)
            return;

        foreach (ListViewItem item in _list.SelectedItems)
        {
            if (item.Tag is ConfiguredApp app)
            {
                app.Mode = mode;
                item.SubItems[2].Text = ModeDisplayText(mode);
            }
        }
        ConfigManager.Save(_settings);
    }

    private void OnAddClicked(object? sender, EventArgs e)
    {
        var window = WindowPickerForm.PickWindow(this);
        if (window is null || window.Aumid is null)
            return;

        if (_settings.Apps.Any(a => string.Equals(a.Aumid, window.Aumid, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(this, "That app is already in the list.", "Add app window",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _settings.Apps.Add(new ConfiguredApp
        {
            Aumid = window.Aumid,
            DisplayName = window.Title,
            Enabled = true,
            Mode = TrayMode.OnlyWhilePlayingAudio,
        });
        ConfigManager.Save(_settings);
        PopulateList();
    }

    private void OnRemoveClicked(object? sender, EventArgs e)
    {
        if (_list.SelectedItems.Count == 0)
            return;

        foreach (ListViewItem item in _list.SelectedItems)
        {
            if (item.Tag is ConfiguredApp app)
                _settings.Apps.Remove(app);
        }
        ConfigManager.Save(_settings);
        PopulateList();
    }

    private void OnUninstallClicked(object? sender, EventArgs e)
    {
        var confirm = MessageBox.Show(this,
            "This removes WinTaskIT's startup registration, its Win+R shortcut, and all " +
            "saved settings (%AppData%\\WinTaskIT), then exits. It does not delete the " +
            "program's own files -- delete this folder yourself afterward if you like.\n\n" +
            "Continue?",
            "Uninstall WinTaskIT", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
            return;

        StartupManager.SetEnabled(false);
        AppPathManager.Unregister();
        ConfigManager.DeleteAll();
        Application.Exit();
    }
}
