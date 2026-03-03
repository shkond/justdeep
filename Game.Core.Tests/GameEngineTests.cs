using Game.Core;
using Game.Core.States;
using Xunit;

namespace Game.Core.Tests;

public class GameEngineTests
{
    private GameEngine CreateStartedEngine()
    {
        var engine = new GameEngine();
        engine.StartNewGame("Hero", seed: 42);
        return engine;
    }

    // ── Initialization ──

    [Fact]
    public void GameEngine_InitialState_IsMainMenu()
    {
        var engine = new GameEngine();

        Assert.NotNull(engine.Session);
        Assert.NotNull(engine.Session.Player);
        Assert.Equal(GameState.MainMenu, engine.Session.CurrentStateId);
    }

    [Fact]
    public void StartNewGame_TransitionsToInDungeon()
    {
        var engine = new GameEngine();
        engine.StartNewGame("Hero", seed: 42);

        Assert.Equal("Hero", engine.Session.Player.Name);
        Assert.Equal(GameState.InDungeon, engine.Session.CurrentStateId);
        Assert.IsType<InDungeonMode>(engine.CurrentMode);
        Assert.Equal(1, engine.Session.CurrentFloor);
        Assert.True(engine.Session.GameLog.Count > 0);
    }

    // ── Tick timing ──

    [Fact]
    public void Tick_BelowInterval_NoAction()
    {
        var engine = CreateStartedEngine();
        int logCountBefore = engine.Session.GameLog.Count;

        engine.Tick(0.1);

        Assert.Equal(logCountBefore, engine.Session.GameLog.Count);
    }

    [Fact]
    public void Tick_AtInterval_ExecutesAction()
    {
        var engine = CreateStartedEngine();
        int logCountBefore = engine.Session.GameLog.Count;

        engine.Tick(1.0);

        Assert.True(engine.Session.GameLog.Count > logCountBefore,
            "Expected at least one new log entry after a full interval tick.");
    }

    [Fact]
    public void Tick_AccumulatesSmallDt()
    {
        var engine = CreateStartedEngine();
        int logCountBefore = engine.Session.GameLog.Count;

        for (int i = 0; i < 60; i++)
        {
            engine.Tick(1.0 / 60.0);
        }

        Assert.True(engine.Session.GameLog.Count > logCountBefore,
            "Expected action after accumulating 60 small dt values to ≥1.0s.");
    }

    // ── State-specific behavior ──

    [Fact]
    public void Tick_InDungeon_ProducesExplorationLog()
    {
        var engine = CreateStartedEngine();
        Assert.Equal(GameState.InDungeon, engine.Session.CurrentStateId);

        int logCountBefore = engine.Session.GameLog.Count;
        engine.Tick(1.0);

        Assert.True(engine.Session.GameLog.Count > logCountBefore);
        Assert.True(engine.Session.RoomsExplored >= 1);
    }

    [Fact]
    public void Tick_MultipleIntervals_ProgressesGame()
    {
        var engine = CreateStartedEngine();

        for (int i = 0; i < 10; i++)
        {
            engine.Tick(1.0);
        }

        Assert.True(engine.Session.GameLog.Count > 2, "Expected substantial log after 10 actions.");
    }

    // ── Inactive states don't tick ──

    [Fact]
    public void Tick_InMainMenu_DoesNothing()
    {
        var engine = new GameEngine();
        int logCount = engine.Session.GameLog.Count;

        engine.Tick(5.0);

        Assert.Equal(logCount, engine.Session.GameLog.Count);
        Assert.Equal(GameState.MainMenu, engine.Session.CurrentStateId);
    }

    [Fact]
    public void Tick_InGameOver_DoesNothing()
    {
        var engine = CreateStartedEngine();

        // Run many ticks until game over or cap out
        for (int i = 0; i < 200; i++)
        {
            engine.Tick(1.0);
            if (engine.Session.CurrentStateId == GameState.GameOver) break;
        }

        if (engine.Session.CurrentStateId == GameState.GameOver)
        {
            int logCount = engine.Session.GameLog.Count;
            engine.Tick(5.0);
            Assert.Equal(logCount, engine.Session.GameLog.Count);
        }
    }

    // ── UsePotion ──

    [Fact]
    public void UsePotion_HealsPlayer()
    {
        var engine = CreateStartedEngine();
        engine.Session.Player.CurrentHp = 50;
        int hpBefore = engine.Session.Player.CurrentHp;
        int goldBefore = engine.Session.Player.Gold;

        engine.UsePotion();

        Assert.True(engine.Session.Player.CurrentHp > hpBefore);
        Assert.True(engine.Session.Player.Gold < goldBefore);
    }

    [Fact]
    public void UsePotion_NotEnoughGold_DoesNotHeal()
    {
        var engine = CreateStartedEngine();
        engine.Session.Player.CurrentHp = 50;
        engine.Session.Player.Gold = 0;
        int hpBefore = engine.Session.Player.CurrentHp;

        engine.UsePotion();

        Assert.Equal(hpBefore, engine.Session.Player.CurrentHp);
    }

    // ── Mode transitions ──

    [Fact]
    public void TransitionTo_ChangesMode()
    {
        var engine = CreateStartedEngine();
        Assert.IsType<InDungeonMode>(engine.CurrentMode);

        engine.TransitionTo(new InBaseMode());

        Assert.IsType<InBaseMode>(engine.CurrentMode);
        Assert.Equal(GameState.InBase, engine.Session.CurrentStateId);
    }

    // ── Seed reproducibility ──

    [Fact]
    public void SameSeed_ProducesSameGameSequence()
    {
        var engine1 = new GameEngine();
        engine1.StartNewGame("Hero", seed: 12345);

        var engine2 = new GameEngine();
        engine2.StartNewGame("Hero", seed: 12345);

        // Run 20 ticks — both should have identical logs
        for (int i = 0; i < 20; i++)
        {
            engine1.Tick(1.0);
            engine2.Tick(1.0);
        }

        Assert.Equal(engine1.Session.GameLog.Count, engine2.Session.GameLog.Count);
        for (int i = 0; i < engine1.Session.GameLog.Count; i++)
        {
            Assert.Equal(engine1.Session.GameLog[i], engine2.Session.GameLog[i]);
        }
    }

    // ── Save/Load ──

    [Fact]
    public void CreateSaveData_ContainsBasicInfo()
    {
        var engine = CreateStartedEngine();
        engine.Tick(1.0); // Advance at least once

        var save = engine.CreateSaveData();

        Assert.Equal(1, save.SaveFormatVersion);
        Assert.Equal(42UL, save.RunSeed);
        Assert.Equal(4, save.RngState.Length);
        Assert.Equal("Hero", save.SessionState.PlayerName);
        Assert.NotNull(save.ModeState);
    }

    // ── InBaseMode recovery ──

    [Fact]
    public void InBaseMode_FullRecovery_Within10Seconds()
    {
        var engine = CreateStartedEngine();
        engine.Session.Player.CurrentHp = 1;
        engine.TransitionTo(new InBaseMode());

        // Tick 20 times at 0.5s each = 10 seconds
        for (int i = 0; i < 20; i++)
        {
            engine.Tick(0.5);
        }

        Assert.Equal(engine.Session.Player.MaxHp, engine.Session.Player.CurrentHp);
    }

    [Fact]
    public void InBaseMode_DoesNotOverheal()
    {
        var engine = CreateStartedEngine();
        engine.Session.Player.CurrentHp = engine.Session.Player.MaxHp - 1;
        engine.TransitionTo(new InBaseMode());

        // Tick until auto-expedition fires or cap out
        for (int i = 0; i < 40; i++)
        {
            engine.Tick(0.5);
            if (engine.Session.CurrentStateId == GameState.InDungeon) break;
        }

        // HP should be exactly MaxHp (not overhealed)
        Assert.Equal(engine.Session.Player.MaxHp, engine.Session.Player.CurrentHp);
    }

    // ── LaunchExpedition ──

    [Fact]
    public void LaunchExpedition_FromBase_TransitionsToInDungeon()
    {
        var engine = CreateStartedEngine();
        engine.TransitionTo(new InBaseMode());
        Assert.Equal(GameState.InBase, engine.Session.CurrentStateId);

        engine.LaunchExpedition();

        Assert.Equal(GameState.InDungeon, engine.Session.CurrentStateId);
        Assert.IsType<InDungeonMode>(engine.CurrentMode);
        Assert.Equal(0, engine.Session.RoomsExplored);
    }

    [Fact]
    public void LaunchExpedition_NotInBase_DoesNothing()
    {
        var engine = CreateStartedEngine();
        Assert.Equal(GameState.InDungeon, engine.Session.CurrentStateId);

        engine.LaunchExpedition();

        // Should remain in dungeon, not reset rooms
        Assert.Equal(GameState.InDungeon, engine.Session.CurrentStateId);
    }

    // ── HP-based retreat ──

    [Fact]
    public void InCombat_HpBelow30Percent_AfterVictory_TriggersRetreat()
    {
        var engine = CreateStartedEngine();

        // Force into combat with a weak enemy
        var weakEnemy = new Enemy("スライム", 1, 10, 0, 10, 5);
        engine.Session.CurrentEnemy = weakEnemy;
        engine.TransitionTo(new InCombatMode());

        // Set player HP to 25% (below 30% threshold)
        engine.Session.Player.CurrentHp = engine.Session.Player.MaxHp * 25 / 100;

        // Tick enough for combat to resolve (enemy has only 1 HP)
        for (int i = 0; i < 10; i++)
        {
            engine.Tick(1.0);
            if (engine.CurrentMode.ModeId != GameState.InCombat) break;
        }

        // Should have transitioned to Returning (not InDungeon) due to low HP
        Assert.Equal(GameState.Returning, engine.Session.CurrentStateId);
        Assert.IsType<ReturningMode>(engine.CurrentMode);
    }

    [Fact]
    public void InCombat_HpBelow30Percent_MidCombat_FleesToReturning()
    {
        var engine = CreateStartedEngine();

        // Force into combat with a strong enemy
        var strongEnemy = new Enemy("ドラゴン", 999, 50, 0, 10, 5);
        engine.Session.CurrentEnemy = strongEnemy;
        engine.TransitionTo(new InCombatMode());

        // Run combat ticks — player will get hit and HP will drop
        for (int i = 0; i < 50; i++)
        {
            engine.Tick(1.0);
            var state = engine.Session.CurrentStateId;
            if (state == GameState.Returning || state == GameState.GameOver) break;
        }

        // Should have either fled (Returning) or died (GameOver)
        var finalState = engine.Session.CurrentStateId;
        Assert.True(
            finalState == GameState.Returning || finalState == GameState.GameOver,
            $"Expected Returning or GameOver, got {finalState}");
    }

    [Fact]
    public void InDungeon_HpBelow30Percent_TriggersRetreat()
    {
        var engine = CreateStartedEngine();

        // Set HP to 20% — should trigger retreat on next non-combat room
        engine.Session.Player.CurrentHp = engine.Session.Player.MaxHp * 20 / 100;

        // Tick until state changes
        for (int i = 0; i < 20; i++)
        {
            engine.Tick(1.0);
            var state = engine.Session.CurrentStateId;
            if (state != GameState.InDungeon && state != GameState.InCombat) break;
        }

        // Should end up Returning (possibly via combat first)
        var finalState = engine.Session.CurrentStateId;
        Assert.True(
            finalState == GameState.Returning || finalState == GameState.GameOver,
            $"Expected Returning or GameOver with low HP, got {finalState}");
    }

    [Fact]
    public void InBase_FullRecovery_AutoLaunchesExpedition()
    {
        var engine = CreateStartedEngine();
        engine.Session.Player.CurrentHp = 1;
        engine.TransitionTo(new InBaseMode());

        // Tick enough for full recovery (20 ticks × 0.5s = 10s)
        for (int i = 0; i < 30; i++)
        {
            engine.Tick(0.5);
            if (engine.Session.CurrentStateId == GameState.InDungeon) break;
        }

        // Should have auto-transitioned to InDungeon
        Assert.Equal(GameState.InDungeon, engine.Session.CurrentStateId);
        Assert.IsType<InDungeonMode>(engine.CurrentMode);
    }

    [Fact]
    public void FullCycle_Retreat_Recover_Reexpedition()
    {
        var engine = CreateStartedEngine();

        // Set HP low to trigger retreat on next room
        engine.Session.Player.CurrentHp = engine.Session.Player.MaxHp * 20 / 100;

        // Phase 1: Tick until retreating
        for (int i = 0; i < 50; i++)
        {
            engine.Tick(1.0);
            if (engine.Session.CurrentStateId == GameState.Returning) break;
            if (engine.Session.CurrentStateId == GameState.GameOver) return; // Can't test further
        }
        Assert.Equal(GameState.Returning, engine.Session.CurrentStateId);

        // Phase 2: Tick until at base
        for (int i = 0; i < 50; i++)
        {
            engine.Tick(0.5);
            if (engine.Session.CurrentStateId == GameState.InBase) break;
        }
        Assert.Equal(GameState.InBase, engine.Session.CurrentStateId);

        // Phase 3: Tick until auto-expedition launches
        for (int i = 0; i < 50; i++)
        {
            engine.Tick(0.5);
            if (engine.Session.CurrentStateId == GameState.InDungeon) break;
        }
        Assert.Equal(GameState.InDungeon, engine.Session.CurrentStateId);
        Assert.Equal(engine.Session.Player.MaxHp, engine.Session.Player.CurrentHp);
    }
}
