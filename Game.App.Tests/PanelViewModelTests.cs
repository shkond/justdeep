using Game.App.Panels;
using Game.Core;
using Xunit;

namespace Game.App.Tests;

/// <summary>
/// Stub IGameCommands for panel VM tests — records calls for verification.
/// </summary>
public class StubGameCommands : IGameCommands
{
    public string? LastStartedPlayerName { get; private set; }
    public int UsePotionCallCount { get; private set; }
    public int LaunchExpeditionCallCount { get; private set; }

    public void StartGame(string playerName) => LastStartedPlayerName = playerName;
    public void UsePotion() => UsePotionCallCount++;
    public void LaunchExpedition() => LaunchExpeditionCallCount++;
}

public class PanelViewModelTests
{
    private static UiState MakeState(GameState mode, string playerName = "Hero") =>
        UiState.Initial with { Mode = mode, PlayerName = playerName };

    // ══════════════════════════════════════════════
    //  MainMenuPanelViewModel
    // ══════════════════════════════════════════════

    [Theory]
    [InlineData(GameState.MainMenu, true)]
    [InlineData(GameState.GameOver, true)]
    [InlineData(GameState.InDungeon, false)]
    [InlineData(GameState.InCombat, false)]
    [InlineData(GameState.InBase, false)]
    [InlineData(GameState.Returning, false)]
    public void MainMenu_VisibilityByMode(GameState mode, bool expected)
    {
        var store = new UiStateStore();
        var vm = new MainMenuPanelViewModel(store, new StubGameCommands());

        store.Update(MakeState(mode));

        Assert.Equal(expected, vm.IsVisible);
    }

    // ══════════════════════════════════════════════
    //  PlayerInfoPanelViewModel
    // ══════════════════════════════════════════════

    [Theory]
    [InlineData(GameState.MainMenu, false)]
    [InlineData(GameState.GameOver, false)]
    [InlineData(GameState.InDungeon, true)]
    [InlineData(GameState.InCombat, true)]
    [InlineData(GameState.InBase, true)]
    [InlineData(GameState.Returning, true)]
    public void PlayerInfo_VisibilityByMode(GameState mode, bool expected)
    {
        var store = new UiStateStore();
        var vm = new PlayerInfoPanelViewModel(store, new StubGameCommands());

        store.Update(MakeState(mode));

        Assert.Equal(expected, vm.IsVisible);
    }

    [Fact]
    public void PlayerInfo_ShowsStats()
    {
        var store = new UiStateStore();
        var vm = new PlayerInfoPanelViewModel(store, new StubGameCommands());

        store.Update(UiState.Initial with
        {
            Mode = GameState.InDungeon,
            PlayerName = "テスト勇者",
            Level = 5,
            CurrentHp = 80,
            MaxHp = 100,
            Attack = 20,
            Defense = 10,
            Experience = 250,
            Gold = 500
        });

        Assert.Contains("テスト勇者", vm.PlayerStats);
        Assert.Contains("レベル: 5", vm.PlayerStats);
        Assert.Contains("HP: 80/100", vm.PlayerStats);
        Assert.Contains("攻撃力: 20", vm.PlayerStats);
        Assert.Contains("防御力: 10", vm.PlayerStats);
        Assert.Contains("ゴールド: 500", vm.PlayerStats);
    }

    [Fact]
    public void PlayerInfo_ShowsDungeonLocation()
    {
        var store = new UiStateStore();
        var vm = new PlayerInfoPanelViewModel(store, new StubGameCommands());

        store.Update(UiState.Initial with
        {
            Mode = GameState.InDungeon,
            CurrentFloor = 3,
            RoomsExplored = 2,
            PlayerName = "Hero"
        });

        Assert.Contains("ダンジョン 3階", vm.GameStatus);
        Assert.Contains("2/5", vm.GameStatus);
    }

    [Fact]
    public void PlayerInfo_ShowsBaseLocation()
    {
        var store = new UiStateStore();
        var vm = new PlayerInfoPanelViewModel(store, new StubGameCommands());

        store.Update(MakeState(GameState.InBase));

        Assert.Contains("拠点", vm.GameStatus);
    }

    // ══════════════════════════════════════════════
    //  CombatPanelViewModel
    // ══════════════════════════════════════════════

    [Theory]
    [InlineData(GameState.InCombat, true)]
    [InlineData(GameState.InDungeon, false)]
    [InlineData(GameState.MainMenu, false)]
    [InlineData(GameState.InBase, false)]
    public void Combat_VisibilityByMode(GameState mode, bool expected)
    {
        var store = new UiStateStore();
        var vm = new CombatPanelViewModel(store, new StubGameCommands());

        store.Update(MakeState(mode));

        Assert.Equal(expected, vm.IsVisible);
    }

    [Fact]
    public void Combat_ShowsEnemyInfo()
    {
        var store = new UiStateStore();
        var vm = new CombatPanelViewModel(store, new StubGameCommands());

        store.Update(UiState.Initial with
        {
            Mode = GameState.InCombat,
            EnemyName = "ゴブリン",
            EnemyCurrentHp = 30,
            EnemyMaxHp = 50,
            EnemyAttack = 12,
            EnemyDefense = 5,
            PlayerName = "Hero"
        });

        Assert.Contains("ゴブリン", vm.EnemyInfo);
        Assert.Contains("HP: 30/50", vm.EnemyInfo);
        Assert.Contains("攻撃力: 12", vm.EnemyInfo);
        Assert.Contains("防御力: 5", vm.EnemyInfo);
    }

    [Fact]
    public void Combat_NoEnemy_EmptyInfo()
    {
        var store = new UiStateStore();
        var vm = new CombatPanelViewModel(store, new StubGameCommands());

        store.Update(MakeState(GameState.InCombat));

        Assert.Equal("", vm.EnemyInfo);
    }

    // ══════════════════════════════════════════════
    //  BasePanelViewModel
    // ══════════════════════════════════════════════

    [Theory]
    [InlineData(GameState.InBase, true)]
    [InlineData(GameState.InDungeon, false)]
    [InlineData(GameState.MainMenu, false)]
    public void Base_VisibilityByMode(GameState mode, bool expected)
    {
        var store = new UiStateStore();
        var vm = new BasePanelViewModel(store, new StubGameCommands());

        store.Update(MakeState(mode));

        Assert.Equal(expected, vm.IsVisible);
    }

    [Fact]
    public void Base_RecoveryInProgress()
    {
        var store = new UiStateStore();
        var vm = new BasePanelViewModel(store, new StubGameCommands());

        store.Update(UiState.Initial with
        {
            Mode = GameState.InBase,
            CurrentHp = 50,
            MaxHp = 100,
            PlayerName = "Hero"
        });

        Assert.Contains("休息中", vm.BaseStatusText);
        Assert.Contains("50/100", vm.BaseStatusText);
        Assert.False(vm.CanLaunchExpedition);
        Assert.Equal(50.0, vm.HpPercent);
    }

    [Fact]
    public void Base_FullRecovery()
    {
        var store = new UiStateStore();
        var vm = new BasePanelViewModel(store, new StubGameCommands());

        store.Update(UiState.Initial with
        {
            Mode = GameState.InBase,
            CurrentHp = 100,
            MaxHp = 100,
            PlayerName = "Hero"
        });

        Assert.Contains("HP全回復", vm.BaseStatusText);
        Assert.True(vm.CanLaunchExpedition);
        Assert.Equal(100.0, vm.HpPercent);
    }

    // ══════════════════════════════════════════════
    //  GameLogPanelViewModel
    // ══════════════════════════════════════════════

    [Fact]
    public void GameLog_ShowsLatest15()
    {
        var store = new UiStateStore();
        var vm = new GameLogPanelViewModel(store, new StubGameCommands());

        var log = new List<string>();
        for (int i = 1; i <= 20; i++)
            log.Add($"Entry {i}");

        store.Update(UiState.Initial with { GameLog = log, PlayerName = "Hero" });

        // Should show entries 6-20 (last 15)
        Assert.DoesNotContain("Entry 5", vm.ActionLog);
        Assert.Contains("Entry 6", vm.ActionLog);
        Assert.Contains("Entry 20", vm.ActionLog);
    }

    [Fact]
    public void GameLog_EmptyLog_EmptyOutput()
    {
        var store = new UiStateStore();
        var vm = new GameLogPanelViewModel(store, new StubGameCommands());

        // Initial state has empty log
        Assert.Equal("", vm.ActionLog);
    }
}
