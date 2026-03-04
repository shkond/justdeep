# アーキテクチャ設計 / Architecture Design

## MVVMパターンの実装

このプロジェクトは、Model-View-ViewModel (MVVM) パターンを採用しています。

```
┌─────────────────────────────────────────────────────────────┐
│                      Game.App (UI Layer)                    │
│  ┌──────────────┐         ┌─────────────────────────────┐  │
│  │   Views      │◄────────┤   ViewModels                 │  │
│  │  (XAML)      │         │  - MainWindowViewModel       │  │
│  │              │         │  - Observable Properties     │  │
│  │  MainWindow  │         │  - Relay Commands            │  │
│  │    .axaml    │         │  - Data Binding              │  │
│  └──────────────┘         └─────────────┬───────────────┘  │
│                                          │                   │
└──────────────────────────────────────────┼───────────────────┘
                                           │ 参照
                                           ▼
┌─────────────────────────────────────────────────────────────┐
│                   Game.Core (Logic Layer)                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Models                                              │  │
│  │  - Player: プレイヤーの状態管理                      │  │
│  │  - Enemy: 敵キャラクターの定義                       │  │
│  │  - GameManager: ゲームロジック全体の管理             │  │
│  │  - GameEnums: ゲーム状態と部屋タイプの定義           │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## プロジェクト構成

### Game.Core（ゲームロジック層）
ビジネスロジックとゲームルールを含む独立したクラスライブラリ。

#### クラス
- **Player.cs**: プレイヤーキャラクター
  - ステータス管理（HP、攻撃力、防御力、レベル）
  - 経験値とレベルアップシステム
  - ゴールドの管理
  - ダメージ計算と回復

- **Enemy.cs**: 敵キャラクター
  - 複数の敵タイプ（スライム、ゴブリン、オーク、ドラゴン）
  - 階層に応じた難易度スケーリング
  - ファクトリーメソッドパターンによる生成

- **GameManager.cs**: ゲーム状態管理
  - ダンジョン探索システム
  - 戦闘システム
  - 部屋タイプの生成（敵、宝箱、ショップ、空部屋）
  - ゲームログの管理
  - ボス戦の管理

- **GameEnums.cs**: 列挙型定義
  - GameState: ゲーム状態
  - DungeonRoom: 部屋タイプ
  - CombatResult: 戦闘結果

### Game.App（UI層）
Avalonia UIを使用したプレゼンテーション層。

#### 状態管理
- **UiState.cs**: UI向け immutable record（GameSession の射影）
- **IUiStateStore.cs / UiStateStore.cs**: 単一状態ストア（record equality で差分検知・通知）

#### ViewModels
- **MainWindowViewModel.cs**: メインウィンドウのビューモデル
  - GameEngine を Tick し、`SyncUiState()` で UiStateStore を更新
  - IGameCommands 実装（StartGame, UsePotion, LaunchExpedition）
- **UiShellViewModel.cs**: パネルレイアウト管理（Left/Right スロット）

#### Panels
- **PanelViewModelBase.cs**: パネル VM 基底クラス（UiStateStore 購読・自動破棄）
- **5 パネル VM**: MainMenu, PlayerInfo, Combat, Base, GameLog
  - 各パネルは `OnStateChanged(UiState)` で可視性・表示内容を更新

#### Views
- **MainWindow.axaml**: メインウィンドウのUI定義
  - データバインディング
  - 動的な表示切り替え
  - ボタンとコマンドのバインディング

## データフロー

```
User Input (Button Click)
    ↓
View (MainWindow.axaml)
    ↓
Command Binding (IGameCommands)
    ↓
MainWindowViewModel → GameEngine.Tick()
    ↓
SyncUiState() → UiStateStore.Update(UiState)
    ↓  (record equality で差分検知)
PanelViewModelBase.OnStateChanged(UiState)
    ↓
Observable Property Changed
    ↓
View Update (UI Refresh)
```

## 依存関係

```
Game.App
  └─► Game.Core

Game.Core.Tests
  └─► Game.Core
```

## 技術スタック

- **.NET 8**: 最新の.NETフレームワーク
- **C# 12**: 最新のC#言語機能
- **Avalonia UI 11.3**: クロスプラットフォームUIフレームワーク
- **CommunityToolkit.Mvvm**: MVVMパターンの実装をサポート
- **xUnit**: ユニットテストフレームワーク

## 設計原則

1. **関心の分離**: ロジックとUIを完全に分離
2. **単一責任の原則**: 各クラスは明確な責任を持つ
3. **依存性の逆転**: UIがロジックに依存、逆はない
4. **テスタビリティ**: ロジック層は独立してテスト可能
5. **拡張性**: 新しい敵タイプや機能の追加が容易

## 将来の拡張可能性

- アイテムシステムの追加
- スキルシステムの実装
- セーブ/ロード機能
- 複数のダンジョンタイプ
- 難易度設定
- サウンドエフェクト
- アニメーション効果
