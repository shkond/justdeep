using Game.Core.Save;

namespace Game.Core.States;

/// <summary>
/// Base mode — player is resting at base, recovering HP over time.
/// </summary>
public class InBaseMode : IGameMode
{
    public GameState ModeId => GameState.InBase;
    public double ActionInterval => 0.5;  // 0.5s tick for smooth recovery
    public double ActionTimer { get; set; }

    /// <summary>Accumulated recovery time at base.</summary>
    public double RecoveryTimer { get; set; }

    /// <summary>Whether HP has fully recovered.</summary>
    public bool IsFullyRecovered { get; set; }

    public void Enter(GameSession session, GameEngine engine)
    {
        ActionTimer = 0;
        RecoveryTimer = 0;
        IsFullyRecovered = session.Player.CurrentHp >= session.Player.MaxHp;
        session.AddToLog("拠点に到着。休息を開始する。");
    }

    public void ExecuteAction(GameSession session, GameEngine engine)
    {
        RecoveryTimer += ActionInterval;
        var player = session.Player;

        if (player.CurrentHp < player.MaxHp)
        {
            // 10s = 20 ticks of 0.5s → heal MaxHp/20 per tick
            int healAmount = Math.Max(1, player.MaxHp / 20);
            player.Heal(healAmount);
            session.AddToLog($"休息中… HP {healAmount} 回復（HP: {player.CurrentHp}/{player.MaxHp}）");
        }
        else if (!IsFullyRecovered)
        {
            IsFullyRecovered = true;
            session.AddToLog("HP全回復。冒険の準備が整った。");
        }
    }

    public void Exit(GameSession session, GameEngine engine) { }

    public ModeStateData ToSaveData() => new InBaseModeData
    {
        ActionTimer = ActionTimer,
        RecoveryTimer = RecoveryTimer
    };
}
