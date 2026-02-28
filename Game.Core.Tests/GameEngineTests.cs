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
}
