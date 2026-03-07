using Game.Core.Items;

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
    public double CarryCapacity { get; set; }

    public int CurrentFloor { get; set; }
    public int RoomsExplored { get; set; }
    public List<string> GameLog { get; set; } = [];

    // ── Inventory ──
    public List<InventoryContainerData> Inventories { get; set; } = [];
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
