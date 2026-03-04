using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.Core;

namespace Game.App.Panels;

/// <summary>
/// Main menu panel — visible before game start and on game over.
/// Handles player name input and game start command.
/// </summary>
public partial class MainMenuPanelViewModel : PanelViewModelBase
{
    public override string PanelId => "MainMenu";
    public override string Title => "メインメニュー";

    [ObservableProperty]
    private string _playerName = "冒険者";

    public MainMenuPanelViewModel(IUiStateStore store, IGameCommands commands)
        : base(store, commands)
    {
    }

    protected override void OnStateChanged(UiState state)
    {
        IsVisible = state.Mode == GameState.MainMenu
                 || state.Mode == GameState.GameOver;
    }

    [RelayCommand]
    private void StartGame()
    {
        Commands.StartGame(PlayerName);
    }
}
