using Game.Core.Items;

namespace Game.Core.Save;

/// <summary>
/// Flat session data: player stats, inventory, location, and log.
/// No mode-specific fields — those live in ModeStateData.
/// </summary>
public class SessionStateData
{
    public List<PlayerData> Players { get; set; } = [];

    public int CurrentFloor { get; set; }
    public int RoomsExplored { get; set; }
    public List<string> GameLog { get; set; } = [];

    // ── Shared inventories only (Stash / Loot) ──
    public List<InventoryContainerData> SharedInventories { get; set; } = [];
}

/// <summary>
/// Serialization DTO for a single <see cref="InventoryContainer"/>.
/// </summary>
public class InventoryContainerData
{
    public InventoryKind Kind { get; set; }
    public double MaxWeight { get; set; }
    public List<InventoryEntryData> Entries { get; set; } = [];
}

/// <summary>
/// Serialization DTO for a single <see cref="InventoryEntry"/>.
/// </summary>
public class InventoryEntryData
{
    public string DefinitionId { get; set; } = "";
    public int Quantity { get; set; }
}
