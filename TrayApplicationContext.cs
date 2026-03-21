namespace DisplayRotationTray;

public class TrayApplicationContext : ApplicationContext
{
    private NotifyIcon trayIcon;
    private ContextMenuStrip contextMenu;
    private DisplayRotation displayRotation;

    public TrayApplicationContext()
    {
        displayRotation = new DisplayRotation();

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

        // 右クリック時にメニューを更新
        trayIcon.MouseUp += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
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

    private void ExitApplication()
    {
        trayIcon.Visible = false;
        trayIcon.Dispose();
        Application.Exit();
    }
}
