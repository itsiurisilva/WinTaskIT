using System.Diagnostics;
using WinTaskIT.Config;
using WinTaskIT.Startup;

namespace WinTaskIT.UI;

internal sealed class SettingsForm : Form
{
    private const string GitHubUrl = "https://github.com/itsiurisilva/WinTaskIT";

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

    // Composed from the transparent logo mark + drawn text rather than the
    // opaque header banner image -- the banner's own baked-in background
    // color doesn't reliably pixel-match a BackColor set here, which left a
    // visible seam at the pillarbox edges. A transparent mark has no such
    // seam, since this panel's BackColor is the only background involved.
    private readonly Panel _header = new()
    {
        Dock = DockStyle.Top,
        Height = 90,
        BackColor = ColorTranslator.FromHtml("#0C0D0F"),
    };

    // TableLayoutPanel + Anchor=None centers each cell's content instead of
    // hardcoded pixel Locations, which clipped the subtitle under DPI scaling.
    private readonly TableLayoutPanel _headerLayout = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        RowCount = 1,
        BackColor = Color.Transparent,
    };

    private readonly TableLayoutPanel _headerTextStack = new()
    {
        ColumnCount = 1,
        RowCount = 2,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        BackColor = Color.Transparent,
        Anchor = AnchorStyles.None,
        Margin = new Padding(0),
    };

    private readonly FlowLayoutPanel _headerWordmarkRow = new()
    {
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        FlowDirection = FlowDirection.LeftToRight,
        BackColor = Color.Transparent,
        Margin = new Padding(0),
        Padding = new Padding(0),
    };

    private readonly PictureBox _headerMark = new()
    {
        Size = new Size(40, 40),
        SizeMode = PictureBoxSizeMode.Zoom,
        BackColor = Color.Transparent,
        Anchor = AnchorStyles.None,
    };

    private readonly Label _headerWordmark1 = new()
    {
        Text = "WinTask",
        AutoSize = true,
        Margin = new Padding(0),
        Font = new Font("Segoe UI", 17F, FontStyle.Bold),
        ForeColor = ColorTranslator.FromHtml("#F5F1EA"),
        BackColor = Color.Transparent,
    };

    private readonly Label _headerWordmark2 = new()
    {
        Text = "IT",
        AutoSize = true,
        Margin = new Padding(0),
        Font = new Font("Segoe UI", 17F, FontStyle.Bold),
        ForeColor = ColorTranslator.FromHtml("#FFB238"),
        BackColor = Color.Transparent,
    };

    private readonly Label _headerSubtitle = new()
    {
        Text = "github.com/itsiurisilva/WinTaskIT",
        AutoSize = true,
        Margin = new Padding(0),
        Font = new Font("Consolas", 8.5F),
        ForeColor = ColorTranslator.FromHtml("#8A8D93"),
        BackColor = Color.Transparent,
    };

    private readonly FlowLayoutPanel _startupRow = new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        FlowDirection = FlowDirection.LeftToRight,
    };

    private readonly CheckBox _runAtStartupCheckBox = new()
    {
        Text = "Run at Windows startup",
        AutoSize = true,
        Padding = new Padding(6),
    };

    private readonly PictureBox _githubLink = new()
    {
        // No background chip -- uses the dark-stroke logo variant instead of
        // the cream one, since dark linework has enough contrast directly
        // against the dialog's light background without a backing box.
        Size = new Size(24, 24),
        SizeMode = PictureBoxSizeMode.Zoom,
        Cursor = Cursors.Hand,
        Margin = new Padding(6, 7, 0, 0),
        BackColor = Color.Transparent,
    };

    private AppSettings _settings = ConfigManager.Load();

    public SettingsForm()
    {
        Text = "WinTaskIT settings";
        Width = 700;
        Height = 490;
        MinimumSize = new Size(600, 400);
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

        using (var logoSource = LoadEmbeddedImage("WinTaskIT.Resources.logo.png"))
        using (var logoDarkSource = LoadEmbeddedImage("WinTaskIT.Resources.logo-dark.png"))
        {
            Icon = (Icon)Icon.FromHandle(ResizeHighQuality(logoSource, 64).GetHicon()).Clone();
            _headerMark.Image = ResizeHighQuality(logoSource, 80);
            _githubLink.Image = ResizeHighQuality(logoDarkSource, 48);
        }
        _githubLink.Click += (_, _) => Process.Start(new ProcessStartInfo(GitHubUrl) { UseShellExecute = true });
        new ToolTip().SetToolTip(_githubLink, "Open WinTaskIT on GitHub");
        _startupRow.Controls.Add(_runAtStartupCheckBox);
        _startupRow.Controls.Add(_githubLink);

        _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _headerWordmarkRow.Controls.Add(_headerWordmark1);
        _headerWordmarkRow.Controls.Add(_headerWordmark2);
        _headerTextStack.Controls.Add(_headerWordmarkRow, 0, 0);
        _headerTextStack.Controls.Add(_headerSubtitle, 0, 1);
        _headerLayout.Controls.Add(_headerMark, 0, 0);
        _headerLayout.Controls.Add(_headerTextStack, 1, 0);
        _header.Controls.Add(_headerLayout);

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
        Controls.Add(_startupRow);
        Controls.Add(_header);
        Controls.Add(buttonPanel);
        Controls.Add(exitPanel);

        _list.ItemChecked += OnItemChecked;
        _runAtStartupCheckBox.Checked = _settings.RunAtStartup;
        _runAtStartupCheckBox.CheckedChanged += OnRunAtStartupChanged;

        PopulateList();
    }

    private static Image LoadEmbeddedImage(string resourceName)
    {
        var assembly = typeof(SettingsForm).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        return Image.FromStream(stream);
    }

    // PictureBox's own on-the-fly scaling (and Bitmap.GetHicon) use low-quality
    // interpolation, which looks blocky scaling this artwork's 376x376 source
    // down to icon sizes. Resizing ahead of time with high-quality settings
    // avoids that instead of relying on the control's own scaling.
    private static Bitmap ResizeHighQuality(Image source, int size)
    {
        var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.DrawImage(source, new Rectangle(0, 0, size, size));
        return bitmap;
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
