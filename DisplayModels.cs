namespace DisplayRotationTray;

/// <summary>
/// ディスプレイ情報を表すレコード
/// </summary>
/// <param name="DeviceName">デバイス名（例：\\.\DISPLAY1）</param>
/// <param name="MonitorName">モニターの名前（例：LG Ultra HD）</param>
/// <param name="IsPrimary">プライマリディスプレイかどうか</param>
public record DisplayInfo(string DeviceName, string MonitorName, bool IsPrimary);

/// <summary>
/// ディスプレイ回転操作の情報を表すレコード
/// </summary>
/// <param name="DeviceName">デバイス名（例：\\.\DISPLAY1）</param>
/// <param name="Rotation">回転角度（0: 通常, 1: 90度, 2: 180度, 3: 270度）</param>
public record DisplayRotationInfo(string DeviceName, int Rotation);
