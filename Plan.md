目的 

最初の着手は「60fps固定Tickで回る遠征シミュレーション（行動=ログ1件）＋撤退→帰還（遭遇リスク付き）＋Seed込みJSONセーブ」の縦切りです。現状はボタンで Explore/Attack/RunAway を手動実行するUI/ロジックなので、ここを先に“自動で進む”形に変えるのが最短です。

いまの実装に合わせた最小改造方針
現 GameManager は ExploreRoom() / EnterCombat() / PlayerAttack() を呼ぶと、内部で Random を使って部屋/敵/ダメージを決めて GameLog に文字列を積む構造です。
UIも MainWindow.axaml で「探索/攻撃/逃げる/ポーション」ボタンを出しており、ポーションは“30ゴールドで使用”として表示されています。
この構造を維持しつつ、**「ボタンで1手」→「Tickで一定間隔ごとに1行動」**に置き換えるのがPhase0のゴールです。

Phase 0（MVP）仕様を確定

1) Tickと“行動”の関係（60fps固定）
Tick() は毎フレーム呼ばれる（1/60秒固定）。

ログの単位が「行動1回」なので、内部で actionTimer += dt して actionTimer >= actionInterval のときだけ1行動進める。

actionInterval は最初は固定（例: 探索1.0s、戦闘1.0s、帰還判定1.0s）でOK；あとで施設/装備/バフで短縮可能にする。

2) 撤退→帰還（遭遇リスク）モデル
遠征開始からの経過時間を T_expedition とする。

撤退ルール発火時点で T_return_target = T_expedition / 2 を固定し、帰還状態に遷移する（あなたの「半分かかる」を厳密に満たす）。

帰還中は「一定距離（時間）ごとに判定」なので、returnCheckTimer が returnCheckInterval を超えるたびに確率 p_encounter で戦闘突入。

戦闘に入ると帰還タイマーは止める/進める、どちらにするかは設計選択（止める方が“危険で遅れる”感が出る）。

3) Seed込みセーブ（互換性NO）
現状 GameManager が new Random() を内部で持つので、ロードで同じ未来を再現できません。
そのため「Seedを保持できるRNG」に置き換え、JSONセーブに Seed + RNG状態（最低でも「何回乱数を引いたか」相当）を保存します。

自前RNG（Xoshiro）で状態（ulong×2など）をそのままJSON保存

具体的なIssue分割（最初の5枚）

GameState拡張と遠征状態機械

GameState に Returning と InBase（拠点）を追加（今は MainMenu/InDungeon/InCombat/... だけ）。

GameManager に「遠征開始時刻」「撤退開始時刻」「帰還残り時間」を持たせる。

Tick基盤の追加

GameManager.Tick() を追加し、60fpsで呼ばれても“行動間隔”でしか進まないようにする。

行動種類：探索1回、戦闘1ターン（=攻撃1回）、帰還チェック1回、など。

ログを“行動イベント”にする（後でタブ分割できる形）
現状は GameLog に文字列を積んで最新15件をUI表示するだけです。

GameEvent（例：CombatTurnResolved, RoomExplored, RetreatTriggered, ReturnEncounterRoll）のような型付きイベントを内部で保持し、表示用に文字列化する層を作る。

これで「別タブに表示」「1行動ずつ」などのUI要求に追従しやすくなる。

RNG差し替え（Seed保存）

IRandom を切って GameManager の _random を注入にする（今は private readonly Random _random = new();）。

SaveData に RngState を入れてJSON保存・復元。

“撤退ルール”の仮実装（GUIの前にプリセットでOK）

まずはコード内プリセット（例：HP<=30%で撤退開始）を1つだけ実装して、「撤退→帰還→帰還中遭遇→帰還完了」まで通す。

GUIビルダーは、このルール評価が動いてから入る（UIだけ先に作ると評価対象が足りず詰まりやすい）。
