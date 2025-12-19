# JustDeep - テキストベース ダンジョンRPG

Avalonia UIを使用したテキストベースのダンジョンRPG（C#/.NET 8）。
MVVMパターンを採用し、ロジック(`Game.Core`)とUI(`Game.App`)を分離しています。

## プロジェクト構成

- **Game.Core**: ゲームロジックを含むクラスライブラリ
  - Player: プレイヤーの状態管理（HP、攻撃力、防御力など）
  - Enemy: 敵キャラクターの実装
  - GameManager: ゲーム状態管理、戦闘システム、ダンジョン探索

- **Game.App**: Avalonia UIアプリケーション
  - MVVM パターンによるUI実装
  - ViewModels: ゲーム状態とUIの橋渡し
  - Views: UIレイアウト

## 必要要件

- .NET 8.0 SDK以上

## ビルド方法

```bash
dotnet build
```

## 実行方法

```bash
dotnet run --project Game.App
```

## ゲームの遊び方

1. プレイヤー名を入力して「ゲーム開始」をクリック
2. ダンジョンを探索して敵と戦い、レベルアップしていく
3. 5部屋ごとにボスが出現
4. ボスを倒すと次の階に進む

### 行動
- **部屋を探索する**: ランダムなイベントが発生（敵、宝箱、ショップ、空部屋）
- **休息する**: HPを回復
- **ポーション使用**: 30ゴールドでHPを50回復
- **攻撃する**: 戦闘中に敵を攻撃
- **逃げる**: 戦闘から逃げる（60%の成功率）

## 技術スタック

- C# / .NET 8
- Avalonia UI 11.3
- MVVM パターン
- CommunityToolkit.Mvvm
