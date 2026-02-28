using Game.Core.Save;

namespace Game.Core.States;

/// <summary>
/// Returning mode — player is traveling back to base.
/// Counts down remaining time based on current floor depth.
/// </summary>
public class ReturningMode : IGameMode
{
    public GameState ModeId => GameState.Returning;
    public double ActionInterval => 0.5;
    public double ActionTimer { get; set; }

    /// <summary>Remaining seconds until arrival at base.</summary>
    public double RemainingTime { get; set; }

    /// <summary>
    /// Create a returning mode with time proportional to floor depth.
    /// </summary>
    public ReturningMode(int currentFloor)
    {
        RemainingTime = currentFloor * 2.0; // 2 seconds per floor
    }

    /// <summary>
    /// Restore from save data.
    /// </summary>
    public ReturningMode(double remainingTime)
    {
        RemainingTime = remainingTime;
    }

    public void Enter(GameSession session, GameEngine engine)
    {
        ActionTimer = 0;
        session.AddToLog($"拠点への帰還を開始… (残り {RemainingTime:F1} 秒)");
    }

    public void ExecuteAction(GameSession session, GameEngine engine)
    {
        RemainingTime -= ActionInterval;

        if (RemainingTime <= 0)
        {
            RemainingTime = 0;
            session.AddToLog("拠点に到着した！");
            engine.TransitionTo(new InBaseMode());
        }
        else
        {
            session.AddToLog($"帰還中… (残り {RemainingTime:F1} 秒)");
        }
    }

    public void Exit(GameSession session, GameEngine engine) { }

    public ModeStateData ToSaveData() => new ReturningModeData
    {
        ActionTimer = ActionTimer,
        RemainingTime = RemainingTime
    };
}
