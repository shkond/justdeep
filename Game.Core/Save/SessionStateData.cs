namespace Game.Core.Save;

/// <summary>
/// Flat session data: player stats, inventory, location, and log.
/// No mode-specific fields — those live in ModeStateData.
/// </summary>
public class SessionStateData
{
    public string PlayerName { get; set; } = "";
    public int Level { get; set; }
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Experience { get; set; }
    public int Gold { get; set; }
    public List<string> Inventory { get; set; } = [];   // Stub for future
    public int CurrentFloor { get; set; }
    public int RoomsExplored { get; set; }
    public List<string> GameLog { get; set; } = [];
}
