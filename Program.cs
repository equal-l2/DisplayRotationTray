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

        // メインウィンドウなしでシステムトレイアイコンのみで動作
        var trayApp = new TrayApplicationContext();
        Application.Run(trayApp);
    }
}