using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.Core;

namespace Game.App.Panels;

/// <summary>
/// Base (拠点) panel — visible only during InBase mode.
/// Shows HP recovery progress and re-expedition button.
/// Reacts to ModeChangedEvent for visibility, StatsChangedEvent for HP progress.
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

    public BasePanelViewModel(UiEventBus eventBus, IGameCommands commands)
        : base(eventBus, commands)
    {
        IsVisible = false; // Hidden by default
    }

    public override void OnEvent(IUiEvent evt)
    {
        switch (evt)
        {
            case ModeChangedEvent mode:
                IsVisible = mode.NewMode == GameState.InBase;
                break;
            case StatsChangedEvent stats when IsVisible:
                UpdateRecoveryStatus(stats.Player);
                break;
        }
    }

    private void UpdateRecoveryStatus(Player player)
    {
        HpPercent = player.MaxHp > 0 ? (double)player.CurrentHp / player.MaxHp * 100 : 100;
        CanLaunchExpedition = player.CurrentHp >= player.MaxHp;

        BaseStatusText = player.CurrentHp >= player.MaxHp
            ? "HP全回復！ 遠征の準備が整った。"
            : $"休息中… HP: {player.CurrentHp}/{player.MaxHp}";
    }

    [RelayCommand]
    private void LaunchExpedition()
    {
        Commands.LaunchExpedition();
    }
}
