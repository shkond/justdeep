using CommunityToolkit.Mvvm.ComponentModel;
using Game.Core;

namespace Game.App.Panels;

/// <summary>
/// Displays player stats and current location info.
/// Visible when game is active (not MainMenu or GameOver).
/// </summary>
public partial class PlayerInfoPanelViewModel : PanelViewModelBase
{
    public override string PanelId => "PlayerInfo";
    public override string Title => "プレイヤー情報";

    [ObservableProperty]
    private string _playerStats = "";

    [ObservableProperty]
    private string _gameStatus = "";

    public PlayerInfoPanelViewModel(IUiStateStore store, IGameCommands commands)
        : base(store, commands) { }

    protected override void OnStateChanged(UiState state)
    {
        IsVisible = state.Mode != GameState.MainMenu
                 && state.Mode != GameState.GameOver;

        if (!IsVisible) return;

        var player = state.Players.Count > 0 ? state.Players[0] : null;

        if (player is null)
        {
            PlayerStats = "【プレイヤー未設定】";
        }
        else
        {
            PlayerStats = $"【{player.Name}】\n" +
                         $"レベル: {player.Level}\n" +
                         $"HP: {player.CurrentHp}/{player.MaxHp}\n" +
                         $"攻撃力: {player.Attack}\n" +
                         $"防御力: {player.Defense}\n" +
                         $"経験値: {player.Experience}/{player.Level * 100}\n" +
                         $"ゴールド: {player.Gold}";
        }

        GameStatus = state.Mode == GameState.InBase
            ? "現在地: 拠点"
            : $"現在地: ダンジョン {state.CurrentFloor}階\n探索した部屋: {state.RoomsExplored}/5";
    }
}
