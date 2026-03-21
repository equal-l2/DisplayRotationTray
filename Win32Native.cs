namespace DisplayRotationTray;

using System.Runtime.InteropServices;

/// <summary>
/// Win32 APIの定義、構造体、定数をまとめたクラス
/// </summary>
public static class Win32Native
{
    #region Win32 API定義
    
    /// <summary>
    /// ディスプレイデバイスを列挙する（Unicode版）
    /// </summary>
    /// <param name="lpDevice">デバイス名（nullで全デバイス）</param>
    /// <param name="iDevNum">デバイスインデックス（0から開始）</param>
    /// <param name="lpDisplayDevice">デバイス情報を格納する構造体</param>
    /// <param name="dwFlags">フラグ（通常0）</param>
    /// <returns>成功時true、失敗時false</returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    /// <summary>
    /// ディスプレイ設定を列挙する（Unicode版）
    /// </summary>
    /// <param name="lpszDeviceName">デバイス名</param>
    /// <param name="iModeNum">モード番号（ENUM_CURRENT_SETTINGSで現在の設定）</param>
    /// <param name="lpDevMode">設定を格納するDEVMODE構造体</param>
    /// <returns>成功時true、失敗時false</returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

    /// <summary>
    /// ディスプレイ設定を変更する（Unicode版）
    /// </summary>
    /// <param name="lpszDeviceName">デバイス名（nullでプライマリディスプレイ）</param>
    /// <param name="lpDevMode">新しい設定を含むDEVMODE構造体</param>
    /// <param name="hwnd">ウィンドウハンドル（通常nullptr）</param>
    /// <param name="dwflags">変更フラグ（CDS_TESTでテスト、CDS_UPDATEREGISTRYで適用）</param>
    /// <param name="lParam">追加パラメータ（通常nullptr）</param>
    /// <returns>変更結果コード（DISP_CHANGE_SUCCESSFULで成功）</returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int ChangeDisplaySettingsEx(string? lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);

    #endregion

    #region 定数定義
    
    /// <summary>
    /// 現在のディスプレイ設定を取得するための特別な値
    /// </summary>
    public const uint ENUM_CURRENT_SETTINGS = unchecked((uint)-1);
    
    /// <summary>
    /// ディスプレイ回転角度の定数
    /// DMDO = Display Device Orientation
    /// </summary>
    public const int DMDO_DEFAULT = 0;  // 通常（横向き）

    /// <summary>
    /// ChangeDisplaySettingsExのフラグ
    /// </summary>
    public const uint CDS_UPDATEREGISTRY = 0x00000001;  // 設定をレジストリに保存して適用
    public const uint CDS_TEST = 0x00000002;            // 設定のテストのみ（実際には変更しない）
    
    /// <summary>
    /// ChangeDisplaySettingsExの戻り値：成功
    /// </summary>
    public const int DISP_CHANGE_SUCCESSFUL = 0;

    /// <summary>
    /// DISPLAY_DEVICE.StateFlagsのフラグ定義
    /// </summary>
    public const uint DISPLAY_DEVICE_ACTIVE = 0x00000001;           // デバイスがアクティブ
    public const uint DISPLAY_DEVICE_PRIMARY_DEVICE = 0x00000004;   // プライマリディスプレイ

    #endregion

    #region 構造体定義

    /// <summary>
    /// ディスプレイデバイス情報を表す構造体
    /// Win32 APIのDISPLAY_DEVICEW構造体に対応
    /// 
    /// 【重要】フィールドの削除・並べ替えは禁止
    /// Win32 APIは構造体のメモリレイアウトを前提として動作するため、
    /// フィールドを削除したり順序を変更すると、API呼び出し時にメモリ破壊が発生する。
    /// 未使用のフィールドがあっても削除せずにそのまま残すこと。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAY_DEVICE
    {
        public int cb;                                    // 構造体のサイズ（バイト単位）
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;                         // デバイス名（例：\\.\DISPLAY1）
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;                       // デバイスの説明（例：モニター）
        public uint StateFlags;                           // デバイスの状態フラグ
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;                           // デバイスID
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;                          // レジストリキー
    }

    /// <summary>
    /// ディスプレイ設定を表す構造体
    /// Win32 APIのDEVMODEW構造体に対応
    /// 
    /// 【重要】フィールドの削除・並べ替えは禁止
    /// Win32 APIは構造体のメモリレイアウトを前提として動作するため、
    /// フィールドを削除したり順序を変更すると、API呼び出し時にメモリ破壊が発生する。
    /// 未使用のフィールドがあっても削除せずにそのまま残すこと。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;                       // デバイス名
        public short dmSpecVersion;                       // 仕様バージョン
        public short dmDriverVersion;                     // ドライバーバージョン
        public short dmSize;                              // 構造体サイズ
        public short dmDriverExtra;                       // 追加ドライバー情報サイズ
        public int dmFields;                              // 有効なフィールドを示すフラグ
        public int dmPositionX;                           // ディスプレイのX座標
        public int dmPositionY;                           // ディスプレイのY座標
        public int dmDisplayOrientation;                  // ディスプレイの回転角度（0-3）
        public int dmDisplayFixedOutput;                  // 固定出力モード
        public short dmColor;                             // カラーモード
        public short dmDuplex;                            // 両面印刷設定
        public short dmYResolution;                       // Y解像度
        public short dmTTOption;                          // TrueTypeオプション
        public short dmCollate;                           // コレート設定
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;                         // 用紙フォーム名
        public short dmLogPixels;                         // 論理インチあたりのピクセル数
        public int dmBitsPerPel;                          // ピクセルあたりのビット数
        public int dmPelsWidth;                           // 画面幅（ピクセル）
        public int dmPelsHeight;                          // 画面高さ（ピクセル）
        public int dmDisplayFlags;                        // ディスプレイフラグ
        public int dmDisplayFrequency;                    // リフレッシュレート
        public int dmICMMethod;                           // ICMメソッド
        public int dmICMIntent;                           // ICMインテント
        public int dmMediaType;                           // メディアタイプ
        public int dmDitherType;                          // ディザリングタイプ
        public int dmReserved1;                           // 予約済み1
        public int dmReserved2;                           // 予約済み2
        public int dmPanningWidth;                        // パンニング幅
        public int dmPanningHeight;                       // パンニング高さ
    }

    #endregion
}
