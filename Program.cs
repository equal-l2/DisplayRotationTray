namespace DisplayRotationTray;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        // メインウィンドウなしでシステムトレイアイコンのみで動作
        var trayApp = new TrayApplicationContext();
        Application.Run(trayApp);
    }
}