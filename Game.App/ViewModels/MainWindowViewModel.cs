using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.Core;

namespace Game.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly GameManager _gameManager;

    [ObservableProperty]
    private string _playerName = "冒険者";

    [ObservableProperty]
    private string _playerStats = "";

    [ObservableProperty]
    private string _gameStatus = "";

    [ObservableProperty]
    private string _actionLog = "";

    [ObservableProperty]
    private bool _isInCombat = false;

    [ObservableProperty]
    private bool _isInDungeon = false;

    [ObservableProperty]
    private bool _isMainMenu = true;

    [ObservableProperty]
    private string _enemyInfo = "";

    public MainWindowViewModel()
    {
        _gameManager = new GameManager();
        UpdateDisplay();
    }

    [RelayCommand]
    private void StartGame()
    {
        _gameManager.StartNewGame(PlayerName);
        IsMainMenu = false;
        IsInDungeon = true;
        IsInCombat = false;
        UpdateDisplay();
    }

    [RelayCommand]
    private void Explore()
    {
        if (!IsInDungeon) return;

        var roomType = _gameManager.ExploreRoom();
        
        switch (roomType)
        {
            case DungeonRoom.Enemy:
                _gameManager.EnterCombat(false);
                IsInCombat = true;
                IsInDungeon = false;
                break;
            case DungeonRoom.Boss:
                _gameManager.EnterCombat(true);
                IsInCombat = true;
                IsInDungeon = false;
                break;
            case DungeonRoom.Treasure:
                _gameManager.OpenTreasure();
                break;
            case DungeonRoom.Shop:
                _gameManager.AddToLog("ショップを見つけた！");
                break;
            case DungeonRoom.Empty:
                _gameManager.AddToLog("空の部屋だ。");
                break;
        }
        
        UpdateDisplay();
    }

    [RelayCommand]
    private void Attack()
    {
        if (!IsInCombat) return;

        var result = _gameManager.PlayerAttack();
        
        if (result.PlayerWon)
        {
            IsInCombat = false;
            IsInDungeon = true;
        }
        else if (_gameManager.CurrentState == GameState.GameOver)
        {
            IsInCombat = false;
            IsInDungeon = false;
            IsMainMenu = true;
        }
        
        UpdateDisplay();
    }

    [RelayCommand]
    private void RunAway()
    {
        if (!IsInCombat) return;

        bool success = _gameManager.RunAway();
        
        if (success || _gameManager.CurrentState == GameState.GameOver)
        {
            IsInCombat = false;
            
            if (_gameManager.CurrentState == GameState.GameOver)
            {
                IsInDungeon = false;
                IsMainMenu = true;
            }
            else
            {
                IsInDungeon = true;
            }
        }
        
        UpdateDisplay();
    }

    [RelayCommand]
    private void UsePotion()
    {
        _gameManager.UsePotion();
        UpdateDisplay();
    }

    [RelayCommand]
    private void Rest()
    {
        if (!IsInDungeon) return;
        
        _gameManager.Rest();
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

        // ログの最新10件を表示
        var recentLogs = _gameManager.GameLog.TakeLast(15);
        ActionLog = string.Join("\n", recentLogs);
    }
}
