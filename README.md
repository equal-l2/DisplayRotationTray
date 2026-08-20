# Display Rotation Tray

Windowsのシステムトレイからディスプレイの回転角度を簡単に変更できるユーティリティです。

## 機能

- システムトレイに常駐し、右クリックでメニューを開く
- 接続されているディスプレイを自動検出
- 各ディスプレイの回転角度を個別に設定（0°/90°/180°/270°）
- メインウィンドウなし（システムトレイのみで動作）

## 動作環境

- Windows 10/11
- .NET 10.0

## 使い方

### 実行

```powershell
dotnet run
```

### 操作方法

1. システムトレイにアイコンが表示されます
2. アイコンを右クリックしてメニューを開く
3. ディスプレイ名を選択してサブメニューを表示
4. 回転角度（0°/90°/180°/270°）を選択

### 終了

システムトレイアイコンを右クリック → 「終了」を選択

## 配布用ビルド

```powershell
dotnet publish
```

ビルド成果物は `bin\Release\net10.0-windows\win-x64\publish\` に出力されます。

## プロジェクト構成

```
display-rotation-win/
├── Program.cs                  # エントリポイント
├── TrayApplicationContext.cs   # システムトレイ管理
├── DisplayRotation.cs          # ディスプレイ回転ロジック
├── DisplayModels.cs            # データモデル
├── Win32Native.cs              # Win32 API定義
└── DisplayRotationTray.csproj  # プロジェクトファイル
```

## 技術スタック

- C# / .NET 10.0
- Windows Forms（システムトレイ）
- Win32 API（ディスプレイ制御）
- WMI（モニター名取得）

## デバッグログ

デバッグログは以下のパスに出力されます：

```
%LOCALAPPDATA%\DisplayRotationTray\Logs\debug.log
```

例：`C:\Users\<ユーザー名>\AppData\Local\DisplayRotationTray\Logs\debug.log`

## ライセンス

MIT
