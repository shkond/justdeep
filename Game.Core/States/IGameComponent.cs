namespace Game.Core.States;

/// <summary>
/// Parallel game component — zero or more can be active simultaneously.
/// Examples (future): poison, buffs, debuffs.
/// </summary>
public interface IGameComponent
{
    /// <summary>Unique identifier for this component type.</summary>
    string ComponentId { get; }

    /// <summary>Whether this component is currently active.</summary>
    bool IsActive { get; }

    /// <summary>Called every frame with the elapsed time.</summary>
    void Tick(double dt, GameSession session, GameEngine engine);

    /// <summary>Serialize component state to a DTO.</summary>
    Save.ComponentData ToSaveData();
}
