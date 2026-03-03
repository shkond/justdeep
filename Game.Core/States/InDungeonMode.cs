using Game.Core.Save;

namespace Game.Core.States;

/// <summary>
/// Dungeon exploration mode — player automatically explores rooms.
/// </summary>
public class InDungeonMode : IGameMode
{
    public GameState ModeId => GameState.InDungeon;
    public double ActionInterval => 1.0;
    public double ActionTimer { get; set; }

    public void Enter(GameSession session, GameEngine engine)
    {
        ActionTimer = 0;
    }

    public void ExecuteAction(GameSession session, GameEngine engine)
    {
        session.RoomsExplored++;

        // Boss every 5 rooms
        if (session.RoomsExplored % 5 == 0)
        {
            HandleRoom(DungeonRoom.Boss, session, engine);
            return;
        }

        int roll = engine.Rng.Next(100);
        DungeonRoom room;
        if (roll < 50)
            room = DungeonRoom.Enemy;
        else if (roll < 70)
            room = DungeonRoom.Treasure;
        else if (roll < 85)
            room = DungeonRoom.Shop;
        else
            room = DungeonRoom.Empty;

        HandleRoom(room, session, engine);

        // After room event, check retreat condition (only if we haven't already
        // transitioned to combat — combat handles its own HP checks)
        if (engine.CurrentMode == this && session.Player.ShouldRetreat)
        {
            session.AddToLog($"HP危険！ 撤退を開始する！（HP: {session.Player.CurrentHp}/{session.Player.MaxHp}）");
            engine.TransitionTo(new ReturningMode(session.CurrentFloor));
        }
    }

    public void Exit(GameSession session, GameEngine engine) { }

    public ModeStateData ToSaveData() => new InDungeonModeData
    {
        ActionTimer = ActionTimer
    };

    // ── Private ──

    private void HandleRoom(DungeonRoom room, GameSession session, GameEngine engine)
    {
        switch (room)
        {
            case DungeonRoom.Enemy:
                EnterCombat(false, session, engine);
                break;
            case DungeonRoom.Boss:
                EnterCombat(true, session, engine);
                break;
            case DungeonRoom.Treasure:
                OpenTreasure(session, engine);
                break;
            case DungeonRoom.Shop:
                session.AddToLog("ショップを見つけた！");
                break;
            case DungeonRoom.Empty:
                session.AddToLog("空の部屋だ。");
                break;
        }
    }

    private void EnterCombat(bool isBoss, GameSession session, GameEngine engine)
    {
        Enemy enemy;
        if (isBoss)
        {
            enemy = Enemy.CreateDragon(session.CurrentFloor);
            session.AddToLog($"ボス【{enemy.Name}】が現れた！");
        }
        else
        {
            int enemyType = engine.Rng.Next(3);
            enemy = enemyType switch
            {
                0 => Enemy.CreateSlime(session.CurrentFloor),
                1 => Enemy.CreateGoblin(session.CurrentFloor),
                _ => Enemy.CreateOrc(session.CurrentFloor)
            };
            session.AddToLog($"【{enemy.Name}】が現れた！");
        }

        session.CurrentEnemy = enemy;
        engine.TransitionTo(new InCombatMode());
    }

    private void OpenTreasure(GameSession session, GameEngine engine)
    {
        int goldFound = engine.Rng.Next(20, 50) + session.CurrentFloor * 10;
        session.Player.AddGold(goldFound);
        session.AddToLog($"宝箱を発見！ ゴールド {goldFound} を獲得！");
    }
}
