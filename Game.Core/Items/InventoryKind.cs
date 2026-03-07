namespace Game.Core.Items;

/// <summary>
/// Identifies the type of inventory container.
/// New kinds (Shop, Temporary, etc.) can be added without breaking existing code.
/// </summary>
public enum InventoryKind
{
    /// <summary>Player's carried inventory.</summary>
    Player,

    /// <summary>Permanent stash / warehouse.</summary>
    Stash,

    /// <summary>Loot dropped by enemies or found in the dungeon.</summary>
    Loot,
}
