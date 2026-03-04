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

    /// <summary>The UI Shell that manages panel layout.</summary>
    public UiShellViewModel Shell { get; }

    // ── Diff detection state (previous frame values) ──
    private GameState _lastModeId;
    private int _lastLogCount;
    private int _lastHp;
    private int _lastAttack;
    private int _lastDefense;
    private int _lastLevel;
    private int _lastExp;
    private int _lastGold;
    private int _lastFloor;
    private int _lastRooms;
    private int _lastEnemyHp;

    public MainWindowViewModel()
    {
        _engine = new GameEngine();

        var eventBus = new UiEventBus();
        Shell = new UiShellViewModel(eventBus);

        // Create panels and register them in slots
        var mainMenu = new MainMenuPanelViewModel(eventBus, this);
        var playerInfo = new PlayerInfoPanelViewModel(eventBus, this) { IsVisible = false };
        var combat = new CombatPanelViewModel(eventBus, this);
        var basePanel = new BasePanelViewModel(eventBus, this);
        var gameLog = new GameLogPanelViewModel(eventBus, this);

        Shell.AddPanel(mainMenu, "Left");
        Shell.AddPanel(playerInfo, "Left");
        Shell.AddPanel(combat, "Left");
        Shell.AddPanel(basePanel, "Left");
        Shell.AddPanel(gameLog, "Right");

        // Initialize diff state
        _lastModeId = _engine.Session.CurrentStateId;
        _lastLogCount = _engine.Session.GameLog.Count;

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

        // Fire initial events: mode change from MainMenu → InDungeon
        var bus = Shell.EventBus;
        bus.Publish(new ModeChangedEvent(GameState.MainMenu, _engine.Session.CurrentStateId));
        SyncDiffState();
        PublishStatsAndFloor();
        PublishLogDiff();
    }

    public void UsePotion()
    {
        _engine.UsePotion();
        // Stats and log will be diff-detected on next tick
    }

    public void LaunchExpedition()
    {
        _engine.LaunchExpedition();
        // Mode change will be diff-detected on next tick
    }

    // ══════════════════════════════════════════════
    //  Tick loop — diff detection → event publishing
    // ══════════════════════════════════════════════

    private void OnTick(object? sender, EventArgs e)
    {
        double dt = _stopwatch.Elapsed.TotalSeconds;
        _stopwatch.Restart();

        _engine.Tick(dt);

        PublishDiffEvents();

        // Handle terminal states
        if (_engine.Session.CurrentStateId == GameState.GameOver)
        {
            _tickTimer.Stop();
            _stopwatch.Stop();
        }
    }

    /// <summary>
    /// Compare current state to previous frame, publish only changed events.
    /// </summary>
    private void PublishDiffEvents()
    {
        var bus = Shell.EventBus;
        var session = _engine.Session;
        var player = session.Player;

        // Mode changed?
        if (session.CurrentStateId != _lastModeId)
        {
            var oldMode = _lastModeId;
            _lastModeId = session.CurrentStateId;
            bus.Publish(new ModeChangedEvent(oldMode, session.CurrentStateId));
        }

        // Stats changed?
        bool statsChanged = player.CurrentHp != _lastHp
                         || player.Attack != _lastAttack
                         || player.Defense != _lastDefense
                         || player.Level != _lastLevel
                         || player.Experience != _lastExp
                         || player.Gold != _lastGold;

        bool enemyChanged = (session.CurrentEnemy?.CurrentHp ?? -1) != _lastEnemyHp;

        if (statsChanged || enemyChanged)
        {
            bus.Publish(new StatsChangedEvent(player, session.CurrentEnemy));
            _lastHp = player.CurrentHp;
            _lastAttack = player.Attack;
            _lastDefense = player.Defense;
            _lastLevel = player.Level;
            _lastExp = player.Experience;
            _lastGold = player.Gold;
            _lastEnemyHp = session.CurrentEnemy?.CurrentHp ?? -1;
        }

        // Floor/rooms changed?
        if (session.CurrentFloor != _lastFloor || session.RoomsExplored != _lastRooms)
        {
            bus.Publish(new FloorChangedEvent(session.CurrentFloor, session.RoomsExplored));
            _lastFloor = session.CurrentFloor;
            _lastRooms = session.RoomsExplored;
        }

        // Log entries added?
        PublishLogDiff();
    }

    private void PublishLogDiff()
    {
        var session = _engine.Session;
        if (session.GameLog.Count != _lastLogCount)
        {
            Shell.EventBus.Publish(new LogAddedEvent(session.GameLog));
            _lastLogCount = session.GameLog.Count;
        }
    }

    private void PublishStatsAndFloor()
    {
        var session = _engine.Session;
        var player = session.Player;
        Shell.EventBus.Publish(new StatsChangedEvent(player, session.CurrentEnemy));
        Shell.EventBus.Publish(new FloorChangedEvent(session.CurrentFloor, session.RoomsExplored));
    }

    private void SyncDiffState()
    {
        var session = _engine.Session;
        var player = session.Player;
        _lastModeId = session.CurrentStateId;
        _lastHp = player.CurrentHp;
        _lastAttack = player.Attack;
        _lastDefense = player.Defense;
        _lastLevel = player.Level;
        _lastExp = player.Experience;
        _lastGold = player.Gold;
        _lastFloor = session.CurrentFloor;
        _lastRooms = session.RoomsExplored;
        _lastEnemyHp = session.CurrentEnemy?.CurrentHp ?? -1;
    }
}
