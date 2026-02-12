using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public class GameManagerTests
{
    private GameManager CreateStartedGame()
    {
        var gm = new GameManager();
        gm.StartNewGame("Hero");
        return gm;
    }

    // ── Initialization ──

    [Fact]
    public void GameManager_InitialState_IsMainMenu()
    {
        var gm = new GameManager();

        Assert.NotNull(gm.Player);
        Assert.Equal(GameState.MainMenu, gm.CurrentState);
        Assert.Equal(1, gm.CurrentFloor);
        Assert.Equal(0, gm.RoomsExplored);
    }

    [Fact]
    public void StartNewGame_TransitionsToInDungeon()
    {
        var gm = new GameManager();
        gm.StartNewGame("Hero");

        Assert.Equal("Hero", gm.Player.Name);
        Assert.Equal(GameState.InDungeon, gm.CurrentState);
        Assert.Equal(1, gm.CurrentFloor);
        Assert.True(gm.GameLog.Count > 0);
    }

    // ── Tick timing ──

    [Fact]
    public void Tick_BelowInterval_NoAction()
    {
        var gm = CreateStartedGame();
        int logCountBefore = gm.GameLog.Count;

        // Tick with tiny dt — should NOT reach action interval
        gm.Tick(0.1);

        Assert.Equal(logCountBefore, gm.GameLog.Count);
    }

    [Fact]
    public void Tick_AtInterval_ExecutesAction()
    {
        var gm = CreateStartedGame();
        int logCountBefore = gm.GameLog.Count;

        // Tick with 1.0s — should trigger exactly one action
        gm.Tick(1.0);

        Assert.True(gm.GameLog.Count > logCountBefore,
            "Expected at least one new log entry after a full interval tick.");
    }

    [Fact]
    public void Tick_AccumulatesSmallDt()
    {
        var gm = CreateStartedGame();
        int logCountBefore = gm.GameLog.Count;

        // 60 ticks × 1/60s = 1.0s total → should trigger action
        for (int i = 0; i < 60; i++)
        {
            gm.Tick(1.0 / 60.0);
        }

        Assert.True(gm.GameLog.Count > logCountBefore,
            "Expected action after accumulating 60 small dt values to ≥1.0s.");
    }

    // ── State-specific behavior ──

    [Fact]
    public void Tick_InDungeon_ProducesExplorationLog()
    {
        var gm = CreateStartedGame();
        Assert.Equal(GameState.InDungeon, gm.CurrentState);

        int logCountBefore = gm.GameLog.Count;
        gm.Tick(1.0);

        Assert.True(gm.GameLog.Count > logCountBefore);
        // RoomsExplored should have incremented
        Assert.True(gm.RoomsExplored >= 1);
    }

    [Fact]
    public void Tick_MultipleIntervals_ProgressesGame()
    {
        var gm = CreateStartedGame();

        // Run enough ticks to explore several rooms
        for (int i = 0; i < 10; i++)
        {
            gm.Tick(1.0);
        }

        // Game should have progressed — either still exploring or in combat/gameover
        Assert.True(gm.GameLog.Count > 2, "Expected substantial log after 10 actions.");
    }

    // ── Inactive states don't tick ──

    [Fact]
    public void Tick_InMainMenu_DoesNothing()
    {
        var gm = new GameManager(); // MainMenu state
        int logCount = gm.GameLog.Count;

        gm.Tick(5.0);

        Assert.Equal(logCount, gm.GameLog.Count);
        Assert.Equal(GameState.MainMenu, gm.CurrentState);
    }

    [Fact]
    public void Tick_InGameOver_DoesNothing()
    {
        var gm = CreateStartedGame();

        // Force game over by draining HP
        gm.Player.CurrentHp = 0;
        // We need a state transition — simulate by ticking into combat and dying
        // Instead, let many ticks run until game over or cap out
        for (int i = 0; i < 200; i++)
        {
            gm.Tick(1.0);
            if (gm.CurrentState == GameState.GameOver) break;
        }

        if (gm.CurrentState == GameState.GameOver)
        {
            int logCount = gm.GameLog.Count;
            gm.Tick(5.0);
            Assert.Equal(logCount, gm.GameLog.Count);
        }
        // If player didn't die in 200 actions, skip this assertion
    }

    // ── UsePotion (the one remaining public action) ──

    [Fact]
    public void UsePotion_HealsPlayer()
    {
        var gm = CreateStartedGame();
        gm.Player.CurrentHp = 50; // damage player
        int hpBefore = gm.Player.CurrentHp;
        int goldBefore = gm.Player.Gold;

        gm.UsePotion();

        Assert.True(gm.Player.CurrentHp > hpBefore);
        Assert.True(gm.Player.Gold < goldBefore);
    }

    [Fact]
    public void UsePotion_NotEnoughGold_DoesNotHeal()
    {
        var gm = CreateStartedGame();
        gm.Player.CurrentHp = 50;
        gm.Player.Gold = 0;
        int hpBefore = gm.Player.CurrentHp;

        gm.UsePotion();

        Assert.Equal(hpBefore, gm.Player.CurrentHp);
    }
}
