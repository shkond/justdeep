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
            var player = state.Players.Count > 0 ? state.Players[0] : null;
            var currentHp = player?.CurrentHp ?? 0;
            var maxHp = player?.MaxHp ?? 0;

            HpPercent = maxHp > 0
                ? (double)currentHp / maxHp * 100 : 0;
            CanLaunchExpedition = player is not null && currentHp >= maxHp;
            BaseStatusText = player is null
                ? "休息中… プレイヤー情報なし"
                : currentHp >= maxHp
                    ? "HP全回復！ 遠征の準備が整った。"
                    : $"休息中… HP: {currentHp}/{maxHp}";
        }
    }

    [RelayCommand]
    private void LaunchExpedition()
    {
        Commands.LaunchExpedition();
    }
}
