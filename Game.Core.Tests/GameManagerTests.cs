using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public class GameManagerTests
{
    [Fact]
    public void GameManager_InitialState_IsCorrect()
    {
        // Arrange & Act
        var gameManager = new GameManager();

        // Assert
        Assert.NotNull(gameManager.Player);
        Assert.Equal(GameState.MainMenu, gameManager.CurrentState);
        Assert.Equal(1, gameManager.CurrentFloor);
        Assert.Equal(0, gameManager.RoomsExplored);
    }

    [Fact]
    public void GameManager_StartNewGame_InitializesGame()
    {
        // Arrange
        var gameManager = new GameManager();

        // Act
        gameManager.StartNewGame("Hero");

        // Assert
        Assert.Equal("Hero", gameManager.Player.Name);
        Assert.Equal(GameState.InDungeon, gameManager.CurrentState);
        Assert.Equal(1, gameManager.CurrentFloor);
        Assert.True(gameManager.GameLog.Count > 0);
    }

    [Fact]
    public void GameManager_ExploreRoom_IncreasesRoomsExplored()
    {
        // Arrange
        var gameManager = new GameManager();
        gameManager.StartNewGame("Hero");
        int initialRooms = gameManager.RoomsExplored;

        // Act
        var roomType = gameManager.ExploreRoom();

        // Assert
        Assert.Equal(initialRooms + 1, gameManager.RoomsExplored);
    }

    [Fact]
    public void GameManager_ExploreRoom_FifthRoomIsBoss()
    {
        // Arrange
        var gameManager = new GameManager();
        gameManager.StartNewGame("Hero");

        // Act - explore 4 rooms
        for (int i = 0; i < 4; i++)
        {
            gameManager.ExploreRoom();
        }
        var fifthRoom = gameManager.ExploreRoom();

        // Assert
        Assert.Equal(DungeonRoom.Boss, fifthRoom);
    }

    [Fact]
    public void GameManager_EnterCombat_CreatesEnemy()
    {
        // Arrange
        var gameManager = new GameManager();
        gameManager.StartNewGame("Hero");

        // Act
        gameManager.EnterCombat(false);

        // Assert
        Assert.NotNull(gameManager.CurrentEnemy);
        Assert.Equal(GameState.InCombat, gameManager.CurrentState);
        Assert.True(gameManager.CurrentEnemy.IsAlive);
    }

    [Fact]
    public void GameManager_PlayerAttack_DamagesEnemy()
    {
        // Arrange
        var gameManager = new GameManager();
        gameManager.StartNewGame("Hero");
        gameManager.EnterCombat(false);
        var enemy = gameManager.CurrentEnemy;
        Assert.NotNull(enemy);
        int initialEnemyHp = enemy.CurrentHp;

        // Act
        var result = gameManager.PlayerAttack();

        // Assert
        Assert.True(initialEnemyHp > enemy.CurrentHp || !enemy.IsAlive);
        Assert.True(result.DamageDealt > 0);
    }

    [Fact]
    public void GameManager_OpenTreasure_GivesGold()
    {
        // Arrange
        var gameManager = new GameManager();
        gameManager.StartNewGame("Hero");
        int initialGold = gameManager.Player.Gold;

        // Act
        gameManager.OpenTreasure();

        // Assert
        Assert.True(gameManager.Player.Gold > initialGold);
    }

    [Fact]
    public void GameManager_Rest_HealsPlayer()
    {
        // Arrange
        var gameManager = new GameManager();
        gameManager.StartNewGame("Hero");
        gameManager.Player.TakeDamage(50);
        int hpBeforeRest = gameManager.Player.CurrentHp;

        // Act
        gameManager.Rest();

        // Assert
        Assert.True(gameManager.Player.CurrentHp > hpBeforeRest);
    }
}
