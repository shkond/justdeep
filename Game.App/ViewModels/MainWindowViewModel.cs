using System;
using System.Diagnostics;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.Core;

namespace Game.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly GameManager _gameManager;
    private readonly DispatcherTimer _tickTimer;
    private readonly Stopwatch _stopwatch;

    [ObservableProperty]
    private string _playerName = "冒険者";

    [ObservableProperty]
    private string _playerStats = "";

    [ObservableProperty]
    private string _gameStatus = "";

    [ObservableProperty]
    private string _actionLog = "";

    [ObservableProperty]
    private bool _isMainMenu = true;

    [ObservableProperty]
    private bool _isPlaying = false;

    [ObservableProperty]
    private bool _isInCombat = false;

    [ObservableProperty]
    private string _enemyInfo = "";

    public MainWindowViewModel()
    {
        _gameManager = new GameManager();

        _stopwatch = new Stopwatch();

        // ~60fps timer (16ms interval)
        _tickTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _tickTimer.Tick += OnTick;

        UpdateDisplay();
    }

    [RelayCommand]
    private void StartGame()
    {
        _gameManager.StartNewGame(PlayerName);
        IsMainMenu = false;
        IsPlaying = true;

        _stopwatch.Restart();
        _tickTimer.Start();

        UpdateDisplay();
    }

    [RelayCommand]
    private void UsePotion()
    {
        _gameManager.UsePotion();
        UpdateDisplay();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        double dt = _stopwatch.Elapsed.TotalSeconds;
        _stopwatch.Restart();

        _gameManager.Tick(dt);

        // Sync UI state
        IsInCombat = _gameManager.CurrentState == GameState.InCombat;

        if (_gameManager.CurrentState == GameState.GameOver
            || _gameManager.CurrentState == GameState.InBase)
        {
            _tickTimer.Stop();
            _stopwatch.Stop();
            IsPlaying = false;

            if (_gameManager.CurrentState == GameState.GameOver)
            {
                IsMainMenu = true;
            }
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var player = _gameManager.Player;

        PlayerStats = $"【{player.Name}】\n" +
                     $"レベル: {player.Level}\n" +
                     $"HP: {player.CurrentHp}/{player.MaxHp}\n" +
                     $"攻撃力: {player.Attack}\n" +
                     $"防御力: {player.Defense}\n" +
                     $"経験値: {player.Experience}/{player.Level * 100}\n" +
                     $"ゴールド: {player.Gold}";

        GameStatus = $"現在地: ダンジョン {_gameManager.CurrentFloor}階\n" +
                    $"探索した部屋: {_gameManager.RoomsExplored}/5";

        if (IsInCombat && _gameManager.CurrentEnemy != null)
        {
            var enemy = _gameManager.CurrentEnemy;
            EnemyInfo = $"【{enemy.Name}】\n" +
                       $"HP: {enemy.CurrentHp}/{enemy.MaxHp}\n" +
                       $"攻撃力: {enemy.Attack}\n" +
                       $"防御力: {enemy.Defense}";
        }
        else
        {
            EnemyInfo = "";
        }

        // Show the most recent 15 log entries
        var recentLogs = _gameManager.GameLog.TakeLast(15);
        ActionLog = string.Join("\n", recentLogs);
    }
}
