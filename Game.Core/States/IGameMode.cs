namespace Game.Core.States;

/// <summary>
/// Exclusive game mode — exactly one is active at any time.
/// Each mode owns its action timer and interval.
/// </summary>
public interface IGameMode
{
    /// <summary>Identifies this mode in the GameState enum.</summary>
    GameState ModeId { get; }

    /// <summary>Seconds between automatic actions in this mode.</summary>
    double ActionInterval { get; }

    /// <summary>Accumulated time since last action (set by GameEngine).</summary>
    double ActionTimer { get; set; }

    /// <summary>Called when transitioning into this mode.</summary>
    void Enter(GameSession session, GameEngine engine);

    /// <summary>Called each time the action timer fires.</summary>
    void ExecuteAction(GameSession session, GameEngine engine);

    /// <summary>Called when transitioning out of this mode.</summary>
    void Exit(GameSession session, GameEngine engine);

    /// <summary>Serialize mode-specific state to a DTO.</summary>
    Save.ModeStateData ToSaveData();
}
