using CommunityToolkit.Mvvm.ComponentModel;
using Game.Core;

namespace Game.App.Panels;

/// <summary>
/// Displays player stats and current location info.
/// Reacts to StatsChangedEvent and FloorChangedEvent only.
/// </summary>
public partial class PlayerInfoPanelViewModel : PanelViewModelBase
{
    public override string PanelId => "PlayerInfo";
    public override string Title => "プレイヤー情報";

    [ObservableProperty]
    private string _playerStats = "";

    [ObservableProperty]
    private string _gameStatus = "";

    public PlayerInfoPanelViewModel(UiEventBus eventBus, IGameCommands commands)
        : base(eventBus, commands) { }

    public override void OnEvent(IUiEvent evt)
    {
        switch (evt)
        {
            case StatsChangedEvent stats:
                UpdatePlayerStats(stats.Player);
                break;
            case FloorChangedEvent floor:
                UpdateFloorStatus(floor);
                break;
            case ModeChangedEvent mode:
                UpdateModeStatus(mode);
                break;
        }
    }

    private void UpdatePlayerStats(Player player)
    {
        PlayerStats = $"【{player.Name}】\n" +
                     $"レベル: {player.Level}\n" +
                     $"HP: {player.CurrentHp}/{player.MaxHp}\n" +
                     $"攻撃力: {player.Attack}\n" +
                     $"防御力: {player.Defense}\n" +
                     $"経験値: {player.Experience}/{player.Level * 100}\n" +
                     $"ゴールド: {player.Gold}";
    }

    private void UpdateFloorStatus(FloorChangedEvent floor)
    {
        GameStatus = $"現在地: ダンジョン {floor.NewFloor}階\n" +
                    $"探索した部屋: {floor.RoomsExplored}/5";
    }

    private void UpdateModeStatus(ModeChangedEvent mode)
    {
        if (mode.NewMode == GameState.InBase)
        {
            GameStatus = "現在地: 拠点";
        }
    }
}
