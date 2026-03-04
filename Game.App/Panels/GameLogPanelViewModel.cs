using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Game.App.Panels;

/// <summary>
/// Displays the game log. Reacts to LogAddedEvent only.
/// </summary>
public partial class GameLogPanelViewModel : PanelViewModelBase
{
    public override string PanelId => "GameLog";
    public override string Title => "ゲームログ";

    [ObservableProperty]
    private string _actionLog = "";

    public GameLogPanelViewModel(UiEventBus eventBus, IGameCommands commands)
        : base(eventBus, commands) { }

    public override void OnEvent(IUiEvent evt)
    {
        if (evt is LogAddedEvent log)
        {
            // Show the most recent 15 entries from the full log
            var recent = log.NewEntries.TakeLast(15);
            ActionLog = string.Join("\n", recent);
        }
    }
}
