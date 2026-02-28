namespace Game.Core.Save;

/// <summary>
/// Interface for save/load I/O operations.
/// Defined in Core; implemented in App layer (e.g., JsonFileSaveStorage).
/// </summary>
public interface ISaveStorage
{
    /// <summary>Persist save data.</summary>
    void Save(GameSaveData data);

    /// <summary>Load save data, or null if no save exists.</summary>
    GameSaveData? Load();
}
