namespace DisplayRotationTray;

public class TrayApplicationContext : ApplicationContext
{
    private NotifyIcon trayIcon;
    private ContextMenuStrip contextMenu;
    private DisplayRotation displayRotation;
    private AppSettings settings;

    public TrayApplicationContext(int dummyDisplayCount = 0)
    {
        displayRotation = new DisplayRotation(dummyDisplayCount);
        settings = AppSettings.Load();

        // メインメニュー作成
        contextMenu = new ContextMenuStrip();

        // トレイアイコン作成
        trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            ContextMenuStrip = contextMenu,
            Text = "Display Rotation",
            Visible = true
        };

        // 左クリック時はクイック切り替えを実行し、右クリック時はメニューを更新
        trayIcon.MouseUp += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                ToggleLeftClickTargets();
            }
            else if (e.Button == MouseButtons.Right)
            {
                UpdateContextMenu();
            }
        };

        // 初回メニュー更新
        UpdateContextMenu();
    }

    /// <summary>
    /// コンテキストメニューを更新する
    /// ディスプレイ名をトップレベルに直接配置し、各ディスプレイのサブメニューに回転角度を設定
    /// </summary>
    private void UpdateContextMenu()
    {
        // メニューをクリア
        contextMenu.Items.Clear();

        // ディスプレイ一覧を取得してメニューに追加
        var displays = displayRotation.GetDisplays();
        foreach (var display in displays)
        {
            // ディスプレイ名をトップレベルのメニュー項目として追加
            // 人間にわかりやすい表示名を使用
            var displayName = displayRotation.GetDisplayName(display);
            var displayItem = new ToolStripMenuItem(displayName);

            // 現在の回転角度を取得
            var currentRotation = displayRotation.GetCurrentRotation(display.DeviceName);

            // 回転角度メニュー追加（サブメニューとして）
            var rotations = new[] { (0, "0°"), (1, "90°"), (2, "180°"), (3, "270°") };
            foreach (var (rotation, label) in rotations)
            {
                var rotationItem = new ToolStripMenuItem(label)
                {
                    Checked = currentRotation == rotation,
                    Tag = new DisplayRotationInfo(display.DeviceName, rotation)
                };
                rotationItem.Click += OnRotationClick;
                displayItem.DropDownItems.Add(rotationItem);
            }

            contextMenu.Items.Add(displayItem);
        }

        // ディスプレイが複数ある場合は区切り線を追加
        if (displays.Count > 0)
        {
            contextMenu.Items.Add(new ToolStripSeparator());
        }

        // 設定メニューを追加
        contextMenu.Items.Add("設定", null, (s, e) => ShowSettings());

        // 終了メニューを追加
        contextMenu.Items.Add("終了", null, (s, e) => ExitApplication());
    }

    private void OnRotationClick(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item && item.Tag is DisplayRotationInfo info)
        {
            try
            {
                displayRotation.SetRotation(info.DeviceName, info.Rotation);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"回転の設定に失敗しました: {ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void ToggleLeftClickTargets()
    {
        if (!settings.LeftClickRotationEnabled)
        {
            return;
        }

        var displays = displayRotation.GetDisplays();
        EnsureDefaultLeftClickRotationSettings(displays);

        var targetDisplay = displays.FirstOrDefault(display => display.DeviceName == settings.LeftClickTargetDeviceName);
        if (targetDisplay is null)
        {
            ShowSettings();
            return;
        }

        var enabledRotations = settings.GetEnabledRotations(targetDisplay.DeviceName);
        if (enabledRotations.Count == 0)
        {
            ShowSettings();
            return;
        }

        try
        {
            var currentRotation = displayRotation.GetCurrentRotation(targetDisplay.DeviceName);
            displayRotation.SetRotation(targetDisplay.DeviceName, GetNextRotation(currentRotation, enabledRotations));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"クイック切り替えに失敗しました: {ex.Message}", "エラー",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void EnsureDefaultLeftClickRotationSettings(List<DisplayInfo> displays)
    {
        if (displays.Count == 0)
        {
            return;
        }

        var settingsChanged = false;
        if (string.IsNullOrEmpty(settings.LeftClickTargetDeviceName) ||
            !displays.Any(display => display.DeviceName == settings.LeftClickTargetDeviceName))
        {
            var defaultDisplay = displays.FirstOrDefault(display => display.IsPrimary) ?? displays[0];
            settings.LeftClickTargetDeviceName = defaultDisplay.DeviceName;
            settingsChanged = true;
        }

        if (!settings.LeftClickRotationTargets.ContainsKey(settings.LeftClickTargetDeviceName))
        {
            settings.LeftClickRotationTargets[settings.LeftClickTargetDeviceName] = AppSettings.RotationCycle.ToList();
            settingsChanged = true;
        }

        if (settingsChanged)
        {
            settings.Save();
        }
    }

    private void ShowSettings()
    {
        using var settingsForm = new SettingsForm(displayRotation, settings);
        settingsForm.ShowDialog();
        settings = AppSettings.Load();
        UpdateContextMenu();
    }

    private static int GetNextRotation(int currentRotation, List<int> enabledRotations)
    {
        if (enabledRotations.Count == 0)
        {
            return currentRotation;
        }

        foreach (var rotation in AppSettings.RotationCycle)
        {
            if (rotation > currentRotation && enabledRotations.Contains(rotation))
            {
                return rotation;
            }
        }

        return enabledRotations[0];
    }

    private void ExitApplication()
    {
        trayIcon.Visible = false;
        trayIcon.Dispose();
        Application.Exit();
    }
}
