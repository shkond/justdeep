using CommunityToolkit.Mvvm.ComponentModel;
using Game.Core;

namespace Game.App.Panels;

/// <summary>
/// Combat panel — visible only during InCombat mode.
/// Shows enemy info and combat actions.
/// Reacts to ModeChangedEvent for visibility, StatsChangedEvent for enemy info.
/// </summary>
public partial class CombatPanelViewModel : PanelViewModelBase
{
    public override string PanelId => "Combat";
    public override string Title => "戦闘";

    [ObservableProperty]
    private string _enemyInfo = "";

    public CombatPanelViewModel(UiEventBus eventBus, IGameCommands commands)
        : base(eventBus, commands)
    {
        IsVisible = false; // Hidden by default
    }

    public override void OnEvent(IUiEvent evt)
    {
        switch (evt)
        {
            case ModeChangedEvent mode:
                IsVisible = mode.NewMode == GameState.InCombat;
                break;
            case StatsChangedEvent stats when IsVisible:
                UpdateEnemyInfo(stats.CurrentEnemy);
                break;
        }
    }

    private void UpdateEnemyInfo(Enemy? enemy)
    {
        if (enemy == null)
        {
            EnemyInfo = "";
            return;
        }

        EnemyInfo = $"【{enemy.Name}】\n" +
                   $"HP: {enemy.CurrentHp}/{enemy.MaxHp}\n" +
                   $"攻撃力: {enemy.Attack}\n" +
                   $"防御力: {enemy.Defense}";
    }
}
