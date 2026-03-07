namespace Game.Core.Items;

/// <summary>
/// Pure data container that holds a collection of <see cref="InventoryEntry"/>
/// items along with its <see cref="InventoryKind"/> and <see cref="InventoryRules"/>.
/// <para>
/// All mutation logic (add, remove, move, weight-check) is handled by
/// <see cref="InventoryService"/>; this class is intentionally kept as a
/// plain data holder.
/// </para>
/// </summary>
public class InventoryContainer
{
    /// <summary>What kind of container this is.</summary>
    public InventoryKind Kind { get; }

    /// <summary>Rules that govern capacity constraints.</summary>
    public InventoryRules Rules { get; }

    /// <summary>Items currently in this container.</summary>
    public List<InventoryEntry> Entries { get; } = [];

    public InventoryContainer(InventoryKind kind, InventoryRules rules)
    {
        Kind = kind;
        Rules = rules;
    }

    /// <summary>Create a container with no weight limit.</summary>
    public static InventoryContainer Create(InventoryKind kind, double maxWeight = 0)
        => new(kind, new InventoryRules(maxWeight));
}
