using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.Core;

namespace Game.App.Panels;

/// <summary>
/// Main menu panel — visible before game start.
/// Handles player name input and game start command.
/// Reacts to ModeChangedEvent for visibility.
/// </summary>
public partial class MainMenuPanelViewModel : PanelViewModelBase
{
    public override string PanelId => "MainMenu";
    public override string Title => "メインメニュー";

    [ObservableProperty]
    private string _playerName = "冒険者";

    public MainMenuPanelViewModel(UiEventBus eventBus, IGameCommands commands)
        : base(eventBus, commands)
    {
        IsVisible = true; // Visible by default
    }

    public override void OnEvent(IUiEvent evt)
    {
        if (evt is ModeChangedEvent mode)
        {
            // Show only when transitioning to MainMenu or GameOver
            IsVisible = mode.NewMode == GameState.MainMenu
                     || mode.NewMode == GameState.GameOver;
        }
    }

    [RelayCommand]
    private void StartGame()
    {
        Commands.StartGame(PlayerName);
    }
}
