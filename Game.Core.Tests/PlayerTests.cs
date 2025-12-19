using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public class PlayerTests
{
    [Fact]
    public void Player_InitialState_IsCorrect()
    {
        // Arrange & Act
        var player = new Player("TestPlayer");

        // Assert
        Assert.Equal("TestPlayer", player.Name);
        Assert.Equal(1, player.Level);
        Assert.Equal(100, player.MaxHp);
        Assert.Equal(100, player.CurrentHp);
        Assert.Equal(10, player.Attack);
        Assert.Equal(5, player.Defense);
        Assert.True(player.IsAlive);
    }

    [Fact]
    public void Player_TakeDamage_ReducesHp()
    {
        // Arrange
        var player = new Player("TestPlayer");
        int initialHp = player.CurrentHp;

        // Act
        player.TakeDamage(30);

        // Assert
        // Actual damage = Max(1, 30 - 5) = 25
        Assert.Equal(initialHp - 25, player.CurrentHp);
        Assert.True(player.IsAlive);
    }

    [Fact]
    public void Player_TakeDamage_CanDie()
    {
        // Arrange
        var player = new Player("TestPlayer");

        // Act
        player.TakeDamage(200);

        // Assert
        Assert.Equal(0, player.CurrentHp);
        Assert.False(player.IsAlive);
    }

    [Fact]
    public void Player_Heal_IncreasesHp()
    {
        // Arrange
        var player = new Player("TestPlayer");
        player.TakeDamage(50);

        // Act
        player.Heal(30);

        // Assert
        Assert.True(player.CurrentHp > 0);
        Assert.True(player.CurrentHp <= player.MaxHp);
    }

    [Fact]
    public void Player_Heal_DoesNotExceedMaxHp()
    {
        // Arrange
        var player = new Player("TestPlayer");
        player.TakeDamage(10);

        // Act
        player.Heal(100);

        // Assert
        Assert.Equal(player.MaxHp, player.CurrentHp);
    }

    [Fact]
    public void Player_GainExperience_LevelsUp()
    {
        // Arrange
        var player = new Player("TestPlayer");
        int initialLevel = player.Level;

        // Act
        player.GainExperience(100); // Level 1 requires 100 exp

        // Assert
        Assert.Equal(initialLevel + 1, player.Level);
        Assert.Equal(120, player.MaxHp); // 100 + 20
        Assert.Equal(120, player.CurrentHp); // Healed to full
        Assert.Equal(13, player.Attack); // 10 + 3
        Assert.Equal(7, player.Defense); // 5 + 2
    }

    [Fact]
    public void Player_SpendGold_Success()
    {
        // Arrange
        var player = new Player("TestPlayer");
        int initialGold = player.Gold;

        // Act
        bool result = player.SpendGold(30);

        // Assert
        Assert.True(result);
        Assert.Equal(initialGold - 30, player.Gold);
    }

    [Fact]
    public void Player_SpendGold_InsufficientFunds()
    {
        // Arrange
        var player = new Player("TestPlayer");
        int initialGold = player.Gold;

        // Act
        bool result = player.SpendGold(1000);

        // Assert
        Assert.False(result);
        Assert.Equal(initialGold, player.Gold);
    }
}
