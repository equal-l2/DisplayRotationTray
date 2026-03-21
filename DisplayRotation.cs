namespace DisplayRotationTray;

using System.Management;
using System.Runtime.InteropServices;
using static DisplayRotationTray.Win32Native;

/// <summary>
/// ディスプレイの回転を管理するクラス
/// Win32 APIを使用してディスプレイ設定を操作する
/// </summary>
public class DisplayRotation
{
    // デバッグログファイルのパス
    // %LOCALAPPDATA%\DisplayRotationTray\Logs\debug.log に出力
    private static readonly string LogFilePath = GetLogFilePath();

    /// <summary>
    /// ログファイルのパスを取得する
    /// </summary>
    private static string GetLogFilePath()
    {
        // %LOCALAPPDATA%\DisplayRotationTray\Logs をログフォルダとして使用
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var logDir = Path.Combine(localAppData, "DisplayRotationTray", "Logs");
        
        // ディレクトリが存在しない場合は作成
        Directory.CreateDirectory(logDir);
        
        return Path.Combine(logDir, "debug.log");
    }

    /// <summary>
    /// デバッグログをファイルに書き込む
    /// </summary>
    private static void LogDebug(string message)
    {
        try
        {
            File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}");
        }
        catch
        {
            // ログ書き込みエラーは無視
        }
    }

    /// <summary>
    /// アクティブなディスプレイの一覧を取得する
    /// </summary>
    /// <returns>アクティブなディスプレイのリスト</returns>
    public List<DisplayInfo> GetDisplays()
    {
        var displays = new List<DisplayInfo>();
        var device = new DISPLAY_DEVICE();
        device.cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE));

        uint deviceIndex = 0;
        
        // EnumDisplayDevicesを順番に呼び出して、すべてのディスプレイデバイスを列挙
        while (EnumDisplayDevices(null, deviceIndex, ref device, 0))
        {
            // StateFlagsにはディスプレイの状態がビットフラグとして格納される
            // 主要なフラグ：
            // - DISPLAY_DEVICE_ACTIVE (0x00000001): デバイスがアクティブ（接続されている）
            // - DISPLAY_DEVICE_PRIMARY_DEVICE (0x00000004): プライマリディスプレイ
            // - DISPLAY_DEVICE_MIRRORING_DRIVER (0x00000008): ミラーリングドライバー
            // - DISPLAY_DEVICE_VGA_COMPATIBLE (0x00000010): VGA互換
            
            // アクティブなディスプレイのみを追加
            // これにより、接続されていないディスプレイや仮想ディスプレイを除外
            if ((device.StateFlags & DISPLAY_DEVICE_ACTIVE) != 0)
            {
                bool isPrimary = (device.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE) != 0;
                
                // モニターの名前を取得
                // EnumDisplayDevicesを2回呼び出す必要がある：
                // 1回目：ディスプレイアダプター（グラフィックカード）の情報
                // 2回目：実際のモニターの情報
                string monitorName = GetMonitorName(device.DeviceName);
                
                displays.Add(new DisplayInfo(device.DeviceName, monitorName, isPrimary));
            }
            
            deviceIndex++;
        }

        return displays;
    }

    /// <summary>
    /// 指定されたディスプレイデバイスに接続されているモニターの名前を取得する
    /// WMIを使用してモニター情報を取得する
    /// </summary>
    /// <param name="deviceName">ディスプレイデバイス名（例：\\.\DISPLAY1）</param>
    /// <returns>モニターの名前（取得できない場合はデバイス名）</returns>
    private string GetMonitorName(string deviceName)
    {
        LogDebug($"[DEBUG] GetMonitorName called for: {deviceName}");
        
        // EnumDisplayDevicesでモニターのDeviceIDを取得
        var monitor = new DISPLAY_DEVICE();
        monitor.cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE));

        uint monitorIndex = 0;
        
        while (EnumDisplayDevices(deviceName, monitorIndex, ref monitor, 0))
        {
            LogDebug($"[DEBUG] Monitor {monitorIndex}: DeviceName={monitor.DeviceName}, DeviceString={monitor.DeviceString}, DeviceID={monitor.DeviceID}, StateFlags={monitor.StateFlags}");
            
            if ((monitor.StateFlags & DISPLAY_DEVICE_ACTIVE) != 0)
            {
                // DeviceIDからモニターIDを抽出
                // 例: "MONITOR\GSM5B55\{4d36e96e-e325-11ce-bfc1-08002be10318}\0001"
                // → "GSM5B55" がモニターID
                string monitorId = ExtractMonitorId(monitor.DeviceID);
                LogDebug($"[DEBUG] Extracted monitorId: {monitorId}");
                
                if (!string.IsNullOrEmpty(monitorId))
                {
                    // WMIからモニター名を取得
                    string? wmiName = GetMonitorNameFromWmi(monitorId);
                    LogDebug($"[DEBUG] WMI result: {wmiName ?? "null"}");
                    
                    if (!string.IsNullOrEmpty(wmiName))
                    {
                        return wmiName;
                    }
                }
                
                // WMIから取得できない場合はDeviceStringを使用
                if (!string.IsNullOrWhiteSpace(monitor.DeviceString))
                {
                    LogDebug($"[DEBUG] Using DeviceString: {monitor.DeviceString}");
                    return monitor.DeviceString;
                }
            }
            monitorIndex++;
        }

        LogDebug($"[DEBUG] Falling back to deviceName: {deviceName}");
        return deviceName;
    }

    /// <summary>
    /// モニターのDeviceIDからモニターIDを抽出する
    /// </summary>
    /// <param name="deviceId">DeviceID（例: "MONITOR\GSM5B55\{4d36e96e-e325-11ce-bfc1-08002be10318}\0001"）</param>
    /// <returns>モニターID（例: "GSM5B55"）</returns>
    private string ExtractMonitorId(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
            return string.Empty;

        // DeviceIDの形式: "MONITOR\<モニタID>\<GUID>\<インスタンス>"
        // バックスラッシュで分割して2番目の要素を取得
        var parts = deviceId.Split('\\');
        if (parts.Length >= 2)
        {
            return parts[1];
        }

        return string.Empty;
    }

    /// <summary>
    /// WMIからモニターの名前を取得する
    /// WmiMonitorIDクラスを使用してモニターのフレンドリ名を取得
    /// </summary>
    /// <param name="monitorId">モニターID（例: "GSM5B55"）</param>
    /// <returns>モニターの名前（取得できない場合はnull）</returns>
    private string? GetMonitorNameFromWmi(string monitorId)
    {
        try
        {
            LogDebug($"[DEBUG] GetMonitorNameFromWmi called with monitorId: {monitorId}");
            
            // WMIクエリ: WmiMonitorIDクラスからモニター情報を取得
            // このクラスにはUserFriendlyNameプロパティがあり、
            // Windows設定アプリで表示されるモニター名と同じ情報が含まれている
            using var searcher = new ManagementObjectSearcher(
                "root\\wmi",
                "SELECT * FROM WmiMonitorID");

            using var collection = searcher.Get();
            
            LogDebug($"[DEBUG] WMI query returned {collection.Count} monitors");
            
            foreach (ManagementObject monitor in collection)
            {
                // InstanceNameプロパティを確認
                // InstanceNameの形式: "DISPLAY\GSM5B55\5&c6f7c27&0&UID4352_0"
                var wmiInstanceName = monitor["InstanceName"] as string;
                LogDebug($"[DEBUG] WMI monitor InstanceName: {wmiInstanceName}");
                
                // InstanceNameにモニターIDが含まれているか確認
                if (!string.IsNullOrEmpty(wmiInstanceName) && 
                    wmiInstanceName.Contains(monitorId, StringComparison.OrdinalIgnoreCase))
                {
                    LogDebug($"[DEBUG] Monitor ID matched!");
                    
                    // UserFriendlyNameはushort[]型で格納されている
                    // 各要素がUTF-16文字のコードポイント
                    if (monitor["UserFriendlyName"] is ushort[] nameChars)
                    {
                        // ushort[]を文字列に変換
                        // null終端文字を除去
                        var name = new string(nameChars
                            .TakeWhile(c => c != 0)
                            .Select(c => (char)c)
                            .ToArray());
                        
                        LogDebug($"[DEBUG] UserFriendlyName: '{name}'");
                        
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            return name;
                        }
                    }
                    else
                    {
                        LogDebug($"[DEBUG] UserFriendlyName is null or not ushort[]");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogDebug($"[DEBUG] WMI exception: {ex.Message}");
            LogDebug($"[DEBUG] WMI exception stack trace: {ex.StackTrace}");
        }

        LogDebug("[DEBUG] GetMonitorNameFromWmi returning null");
        return null;
    }

    /// <summary>
    /// ディスプレイの表示名を生成する
    /// </summary>
    /// <param name="display">ディスプレイ情報</param>
    /// <returns>人間にわかりやすい表示名</returns>
    public string GetDisplayName(DisplayInfo display)
    {
        // モニター名が空の場合はデバイス名をフォールバックとして使用
        string name = string.IsNullOrWhiteSpace(display.MonitorName) 
            ? display.DeviceName 
            : display.MonitorName;
        
        // プライマリディスプレイの場合は注釈を追加
        if (display.IsPrimary)
        {
            return $"{name} (プライマリ)";
        }
        
        return name;
    }

    /// <summary>
    /// 指定されたディスプレイの現在の回転角度を取得する
    /// </summary>
    /// <param name="deviceName">デバイス名（例：\\.\DISPLAY1）</param>
    /// <returns>回転角度（0: 通常, 1: 90度, 2: 180度, 3: 270度）</returns>
    public int GetCurrentRotation(string deviceName)
    {
        var devMode = new DEVMODE();
        devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

        // ENUM_CURRENT_SETTINGSを使用して現在の設定を取得
        // unchecked((int)ENUM_CURRENT_SETTINGS)でuintをintに変換（値は-1）
        if (EnumDisplaySettings(deviceName, unchecked((int)ENUM_CURRENT_SETTINGS), ref devMode))
        {
            return devMode.dmDisplayOrientation;
        }

        // 取得失敗時はデフォルト（通常）を返す
        return DMDO_DEFAULT;
    }

    /// <summary>
    /// 指定されたディスプレイの回転角度を設定する
    /// </summary>
    /// <param name="deviceName">デバイス名（例：\\.\DISPLAY1）</param>
    /// <param name="rotation">回転角度（0: 通常, 1: 90度, 2: 180度, 3: 270度）</param>
    /// <exception cref="InvalidOperationException">設定の取得または変更に失敗した場合</exception>
    public void SetRotation(string deviceName, int rotation)
    {
        var devMode = new DEVMODE();
        devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

        // 現在の設定を取得
        if (!EnumDisplaySettings(deviceName, unchecked((int)ENUM_CURRENT_SETTINGS), ref devMode))
        {
            throw new InvalidOperationException("ディスプレイ設定の取得に失敗しました");
        }

        // 現在の回転角度を保存
        int currentRotation = devMode.dmDisplayOrientation;

        // 縦横の入れ替えが必要か判定
        // 回転角度の偶奇が変わらない場合は縦横の入れ替え不要
        // 例：0度（横向き）→ 90度（縦向き）には入れ替え必要
        // 例：0度（横向き）→ 180度（横向き）には入れ替え不要
        bool needsSwap = (currentRotation % 2) != (rotation % 2);

        // 新しい回転角度を設定
        devMode.dmDisplayOrientation = rotation;

        // 縦横の入れ替えが必要な場合、画面解像度を入れ替える
        // 例：1920x1080 → 1080x1920
        if (needsSwap)
        {
            (devMode.dmPelsWidth, devMode.dmPelsHeight) = (devMode.dmPelsHeight, devMode.dmPelsWidth);
        }

        // まずテスト実行して、設定が有効か確認
        // CDS_TESTフラグを指定すると、実際に変更せずにテストのみ行う
        int result = ChangeDisplaySettingsEx(deviceName, ref devMode, IntPtr.Zero, CDS_TEST, IntPtr.Zero);
        if (result != DISP_CHANGE_SUCCESSFUL)
        {
            throw new InvalidOperationException("この回転角度はサポートされていません");
        }

        // テストが成功した場合、実際に設定を適用
        // CDS_UPDATEREGISTRYフラグでレジストリに保存して永続化
        result = ChangeDisplaySettingsEx(deviceName, ref devMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
        if (result != DISP_CHANGE_SUCCESSFUL)
        {
            throw new InvalidOperationException("ディスプレイの回転に失敗しました");
        }
    }
}
