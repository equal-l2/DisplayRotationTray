namespace DisplayRotationTray;

static class Program
{
    [STAThread]
    static void Main()
    {
        // 多重起動防止
        using var mutex = new Mutex(true, "DisplayRotationTray.SingleInstance", out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "DisplayRotationTray は既に起動しています。",
                "DisplayRotationTray",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        // UIの配色をシステムカラーに合わせる
        Application.SetColorMode(SystemColorMode.System);
        // メニューの左側マージンの余計なエフェクトを消す
        ToolStripManager.Renderer = new ToolStripSystemRenderer();

        // メインウィンドウなしでシステムトレイアイコンのみで動作
        var trayApp = new TrayApplicationContext();
        Application.Run(trayApp);
    }
}