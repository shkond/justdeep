using CommunityToolkit.Mvvm.ComponentModel;
using Game.Core;

namespace Game.App.Panels;

/// <summary>
/// Combat panel — visible only during InCombat mode.
/// Shows enemy info. Reacts to state changes for visibility and enemy data.
/// </summary>
public partial class CombatPanelViewModel : PanelViewModelBase
{
    public override string PanelId => "Combat";
    public override string Title => "戦闘";

    [ObservableProperty]
    private string _enemyInfo = "";

    public CombatPanelViewModel(IUiStateStore store, IGameCommands commands)
        : base(store, commands)
    {
    }

    protected override void OnStateChanged(UiState state)
    {
        IsVisible = state.Mode == GameState.InCombat;

        if (IsVisible && state.EnemyName != null)
        {
            EnemyInfo = $"【{state.EnemyName}】\n" +
                       $"HP: {state.EnemyCurrentHp}/{state.EnemyMaxHp}\n" +
                       $"攻撃力: {state.EnemyAttack}\n" +
                       $"防御力: {state.EnemyDefense}";
        }
        else
        {
            EnemyInfo = "";
        }
    }
}
