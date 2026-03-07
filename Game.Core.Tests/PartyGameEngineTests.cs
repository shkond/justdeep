using Game.Core;
using Game.Core.States;
using Xunit;

namespace Game.Core.Tests;

public class PartyGameEngineTests
{
    [Fact]
    public void StartNewGame_WithMultipleMembers_CreatesParty()
    {
        var engine = new GameEngine();

        engine.StartNewGame(["Hero", "Mage", "Tank"], seed: 42);

        Assert.Equal(3, engine.Session.Party.Members.Count);
        Assert.Equal("Hero", engine.Session.Party.Members[0].Name);
        Assert.Equal(GameState.InDungeon, engine.Session.CurrentStateId);
    }

    [Fact]
    public void MultiParty_CombatToReturnToRecoveryCycle_Works()
    {
        var engine = new GameEngine();
        engine.StartNewGame(["Hero", "Mage"], seed: 42);

        // Low HP on leader to force retreat decision.
        var leader = engine.Session.Party.Members[0];
        leader.CurrentHp = leader.MaxHp * 20 / 100;

        var weakEnemy = new Enemy("スライム", 1, 10, 0, 10, 5);
        engine.Session.CurrentEnemy = weakEnemy;
        engine.TransitionTo(new InCombatMode());

        // Resolve combat; should retreat because Party.ShouldRetreat is true.
        for (int i = 0; i < 10; i++)
        {
            engine.Tick(1.0);
            if (engine.Session.CurrentStateId != GameState.InCombat)
            {
                break;
            }
        }

        Assert.Equal(GameState.Returning, engine.Session.CurrentStateId);

        // Reach base.
        for (int i = 0; i < 50; i++)
        {
            engine.Tick(0.5);
            if (engine.Session.CurrentStateId == GameState.InBase)
            {
                break;
            }
        }

        Assert.Equal(GameState.InBase, engine.Session.CurrentStateId);

        // Recover and auto-launch.
        for (int i = 0; i < 80; i++)
        {
            engine.Tick(0.5);
            if (engine.Session.CurrentStateId == GameState.InDungeon)
            {
                break;
            }
        }

        Assert.Equal(GameState.InDungeon, engine.Session.CurrentStateId);
        Assert.True(engine.Session.Party.IsFullyHealed);
    }
}
