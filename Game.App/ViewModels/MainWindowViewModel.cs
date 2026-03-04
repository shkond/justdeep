using System;
using System.Diagnostics;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Game.Core;
using Game.Core.States;
using Game.App.Panels;

namespace Game.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IGameCommands
{
    private readonly GameEngine _engine;
    private readonly DispatcherTimer _tickTimer;
    private readonly Stopwatch _stopwatch;
    private readonly UiStateStore _store;

    /// <summary>The UI Shell that manages panel layout.</summary>
    public UiShellViewModel Shell { get; }

    public MainWindowViewModel()
    {
        _engine = new GameEngine();
        _store = new UiStateStore();
        Shell = new UiShellViewModel();

        // Create panels — all subscribe to the store
        var mainMenu = new MainMenuPanelViewModel(_store, this);
        var playerInfo = new PlayerInfoPanelViewModel(_store, this);
        var combat = new CombatPanelViewModel(_store, this);
        var basePanel = new BasePanelViewModel(_store, this);
        var gameLog = new GameLogPanelViewModel(_store, this);

        Shell.AddPanel(mainMenu, "Left");
        Shell.AddPanel(playerInfo, "Left");
        Shell.AddPanel(combat, "Left");
        Shell.AddPanel(basePanel, "Left");
        Shell.AddPanel(gameLog, "Right");

        _stopwatch = new Stopwatch();

        // ~60fps timer (16ms interval)
        _tickTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _tickTimer.Tick += OnTick;
    }

    // ══════════════════════════════════════════════
    //  IGameCommands implementation
    // ══════════════════════════════════════════════

    public void StartGame(string playerName)
    {
        _engine.StartNewGame(playerName);

        _stopwatch.Restart();
        _tickTimer.Start();

        SyncUiState();
    }

    public void UsePotion()
    {
        _engine.UsePotion();
        // State change will be picked up on next tick via SyncUiState
    }

    public void LaunchExpedition()
    {
        _engine.LaunchExpedition();
        // State change will be picked up on next tick via SyncUiState
    }

    // ══════════════════════════════════════════════
    //  Tick loop — sync state to store
    // ══════════════════════════════════════════════

    private void OnTick(object? sender, EventArgs e)
    {
        double dt = _stopwatch.Elapsed.TotalSeconds;
        _stopwatch.Restart();

        _engine.Tick(dt);

        SyncUiState();

        // Handle terminal states
        if (_engine.Session.CurrentStateId == GameState.GameOver)
        {
            _tickTimer.Stop();
            _stopwatch.Stop();
        }
    }

    /// <summary>
    /// Project the current GameSession into UiState and push to store.
    /// The store's record-equality check ensures subscribers are only
    /// notified when something actually changed (no manual diff fields needed).
    /// </summary>
    private void SyncUiState()
    {
        var s = _engine.Session;
        var p = s.Player;
        var e = s.CurrentEnemy;

        _store.Update(new UiState(
            Mode: s.CurrentStateId,
            PlayerName: p.Name,
            Level: p.Level,
            CurrentHp: p.CurrentHp,
            MaxHp: p.MaxHp,
            Attack: p.Attack,
            Defense: p.Defense,
            Experience: p.Experience,
            Gold: p.Gold,
            CurrentFloor: s.CurrentFloor,
            RoomsExplored: s.RoomsExplored,
            EnemyName: e?.Name,
            EnemyCurrentHp: e?.CurrentHp,
            EnemyMaxHp: e?.MaxHp,
            EnemyAttack: e?.Attack,
            EnemyDefense: e?.Defense,
            GameLog: s.GameLog
        ));
    }
}
