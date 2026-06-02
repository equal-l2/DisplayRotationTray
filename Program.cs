namespace DisplayRotationTray;

static class Program
{
    private const string DevDummyDisplayFlag = "--dev-dummy-display";

    [STAThread]
    static void Main(string[] args)
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
        var trayApp = new TrayApplicationContext(GetDevDummyDisplayCount(args));
        Application.Run(trayApp);
    }

    private static int GetDevDummyDisplayCount(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg == DevDummyDisplayFlag)
            {
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out var count))
                {
                    return Math.Max(0, count);
                }

                return 1;
            }

            var prefix = $"{DevDummyDisplayFlag}=";
            if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(arg[prefix.Length..], out var inlineCount))
            {
                return Math.Max(0, inlineCount);
            }
        }

        return 0;
    }
}
