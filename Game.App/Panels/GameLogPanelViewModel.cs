using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Game.App.Panels;

/// <summary>
/// Displays the game log. Shows the most recent 15 entries.
/// </summary>
public partial class GameLogPanelViewModel : PanelViewModelBase
{
    public override string PanelId => "GameLog";
    public override string Title => "ゲームログ";

    [ObservableProperty]
    private string _actionLog = "";

    public GameLogPanelViewModel(IUiStateStore store, IGameCommands commands)
        : base(store, commands) { }

    protected override void OnStateChanged(UiState state)
    {
        var recent = state.GameLog.TakeLast(15);
        ActionLog = string.Join("\n", recent);
    }
}
