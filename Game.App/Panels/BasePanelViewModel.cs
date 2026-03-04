using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.Core;

namespace Game.App.Panels;

/// <summary>
/// Base (拠点) panel — visible only during InBase mode.
/// Shows HP recovery progress and re-expedition button.
/// </summary>
public partial class BasePanelViewModel : PanelViewModelBase
{
    public override string PanelId => "Base";
    public override string Title => "拠点";

    [ObservableProperty]
    private double _hpPercent = 100;

    [ObservableProperty]
    private string _baseStatusText = "";

    [ObservableProperty]
    private bool _canLaunchExpedition;

    public BasePanelViewModel(IUiStateStore store, IGameCommands commands)
        : base(store, commands)
    {
    }

    protected override void OnStateChanged(UiState state)
    {
        IsVisible = state.Mode == GameState.InBase;

        if (IsVisible)
        {
            HpPercent = state.MaxHp > 0
                ? (double)state.CurrentHp / state.MaxHp * 100 : 100;
            CanLaunchExpedition = state.CurrentHp >= state.MaxHp;
            BaseStatusText = state.CurrentHp >= state.MaxHp
                ? "HP全回復！ 遠征の準備が整った。"
                : $"休息中… HP: {state.CurrentHp}/{state.MaxHp}";
        }
    }

    [RelayCommand]
    private void LaunchExpedition()
    {
        Commands.LaunchExpedition();
    }
}
