using Game.App.Panels;
using Game.Core;
using Xunit;

namespace Game.App.Tests;

/// <summary>
/// Stub IGameCommands for panel VM tests — records calls for verification.
/// </summary>
public class StubGameCommands : IGameCommands
{
    public int StartGameCallCount { get; private set; }
    public int UsePotionCallCount { get; private set; }
    public int LaunchExpeditionCallCount { get; private set; }
    public List<Guid> UsePotionTargets { get; } = [];

    public void StartGame() => StartGameCallCount++;
    public void UsePotion(Guid playerId)
    {
        UsePotionCallCount++;
        UsePotionTargets.Add(playerId);
    }

    public void LaunchExpedition() => LaunchExpeditionCallCount++;
}

public class PanelViewModelTests
{
    private static PlayerSnapshot MakePlayer(
        string name = "Hero",
        int level = 1,
        int currentHp = 100,
        int maxHp = 100,
        int attack = 10,
        int defense = 5,
        int experience = 0,
        int gold = 0) =>
        new(
            PlayerId: Guid.NewGuid(),
            Name: name,
            Level: level,
            CurrentHp: currentHp,
            MaxHp: maxHp,
            Attack: attack,
            Defense: defense,
            Experience: experience,
            Gold: gold);

    private static UiState MakeState(GameState mode, string playerName = "Hero") =>
        UiState.Initial with { Mode = mode, Players = [MakePlayer(name: playerName)] };

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

    [Fact]
    public void MainMenu_StartGame_InvokesCommand()
    {
        var store = new UiStateStore();
        var commands = new StubGameCommands();
        var vm = new MainMenuPanelViewModel(store, commands);

        vm.StartGameCommand.Execute(null);

        Assert.Equal(1, commands.StartGameCallCount);
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
            Players =
            [
                MakePlayer(
                    name: "テスト勇者",
                    level: 5,
                    currentHp: 80,
                    maxHp: 100,
                    attack: 20,
                    defense: 10,
                    experience: 250,
                    gold: 500)
            ]
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
            Players = [MakePlayer(name: "Hero")]
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
            Players = [MakePlayer(name: "Hero")]
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
            Players = [MakePlayer(name: "Hero", currentHp: 50, maxHp: 100)]
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
            Players = [MakePlayer(name: "Hero", currentHp: 100, maxHp: 100)]
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

        store.Update(UiState.Initial with { GameLog = log, Players = [MakePlayer(name: "Hero")] });

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
