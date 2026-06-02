namespace DisplayRotationTray;

public class SettingsForm : Form
{
    private readonly DisplayRotation displayRotation;
    private readonly AppSettings settings;
    private readonly List<DisplayRotationToggle> rotationToggles = [];
    private readonly List<DisplayTargetOption> targetOptions = [];
    private CheckBox modeToggle = null!;
    private FlowLayoutPanel displayPanel = null!;

    public SettingsForm(DisplayRotation displayRotation, AppSettings settings)
    {
        this.displayRotation = displayRotation;
        this.settings = settings;

        Text = "設定";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(520, 420);

        BuildLayout();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 5
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var titleLabel = new Label
        {
            AutoSize = true,
            Text = "クイック切り替え",
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };
        root.Controls.Add(titleLabel, 0, 0);

        var descriptionLabel = new Label
        {
            AutoSize = true,
            Text = "トレイアイコンを左クリックしたとき、選択したディスプレイを次の対象角度へ直接切り替えます。",
            MaximumSize = new Size(480, 0),
            Margin = new Padding(0, 0, 0, 10)
        };
        root.Controls.Add(descriptionLabel, 0, 1);

        modeToggle = new CheckBox
        {
            AutoSize = true,
            Text = "クイック切り替えを有効にする",
            Checked = settings.LeftClickRotationEnabled,
            Margin = new Padding(0, 0, 0, 12)
        };
        modeToggle.CheckedChanged += (_, _) => displayPanel.Enabled = modeToggle.Checked;
        root.Controls.Add(modeToggle, 0, 2);

        displayPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 10)
        };

        var displays = displayRotation.GetDisplays();
        foreach (var display in displays)
        {
            displayPanel.Controls.Add(CreateDisplayRotationGroup(display));
        }

        if (displays.Count == 0)
        {
            displayPanel.Controls.Add(new Label
            {
                AutoSize = true,
                Text = "アクティブなディスプレイが見つかりません。",
                Margin = new Padding(0, 8, 0, 0)
            });
        }

        displayPanel.Enabled = modeToggle.Checked;
        root.Controls.Add(displayPanel, 0, 3);

        var buttonPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            FlowDirection = FlowDirection.RightToLeft
        };

        var okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Width = 86
        };
        okButton.Click += (_, _) => SaveSettings();

        var cancelButton = new Button
        {
            Text = "キャンセル",
            DialogResult = DialogResult.Cancel,
            Width = 86
        };

        buttonPanel.Controls.Add(okButton);
        buttonPanel.Controls.Add(cancelButton);
        root.Controls.Add(buttonPanel, 0, 4);

        AcceptButton = okButton;
        CancelButton = cancelButton;
        Controls.Add(root);
    }

    private GroupBox CreateDisplayRotationGroup(DisplayInfo display)
    {
        var group = new GroupBox
        {
            Text = string.Empty,
            Width = 460,
            Height = 136,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(12, 18, 12, 8)
        };

        var rotations = settings.GetEnabledRotations(display.DeviceName);
        var groupLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        groupLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        groupLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        groupLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var targetOption = new RadioButton
        {
            AutoSize = true,
            Text = displayRotation.GetDisplayName(display),
            Checked = settings.LeftClickTargetDeviceName == display.DeviceName,
            Margin = new Padding(0, 0, 0, 8)
        };
        targetOption.CheckedChanged += (_, _) =>
        {
            if (!targetOption.Checked)
            {
                return;
            }

            foreach (var option in targetOptions.Where(option => option.Option != targetOption))
            {
                option.Option.Checked = false;
            }
        };
        targetOptions.Add(new DisplayTargetOption(display.DeviceName, targetOption));
        groupLayout.Controls.Add(targetOption, 0, 0);

        var rotationLabel = new Label
        {
            AutoSize = true,
            Text = "切り替え対象角度",
            Margin = new Padding(16, 0, 0, 4)
        };
        groupLayout.Controls.Add(rotationLabel, 0, 1);

        var rotationPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(16, 0, 0, 0)
        };

        foreach (var rotation in AppSettings.RotationCycle)
        {
            var toggle = new CheckBox
            {
                AutoSize = true,
                Text = $"{rotation * 90}°",
                Checked = rotations.Contains(rotation),
                Margin = new Padding(0, 4, 18, 0)
            };

            rotationToggles.Add(new DisplayRotationToggle(display.DeviceName, rotation, toggle));
            rotationPanel.Controls.Add(toggle);
        }

        groupLayout.Controls.Add(rotationPanel, 0, 2);
        group.Controls.Add(groupLayout);
        return group;
    }

    private void SaveSettings()
    {
        settings.LeftClickRotationEnabled = modeToggle.Checked;
        settings.LeftClickTargetDeviceName = targetOptions
            .FirstOrDefault(option => option.Option.Checked)
            ?.DeviceName;
        settings.LeftClickRotationTargets = rotationToggles
            .GroupBy(toggle => toggle.DeviceName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Where(toggle => toggle.Toggle.Checked)
                    .Select(toggle => toggle.Rotation)
                    .ToList());
        settings.Save();
    }

    private record DisplayTargetOption(string DeviceName, RadioButton Option);
    private record DisplayRotationToggle(string DeviceName, int Rotation, CheckBox Toggle);
}
