namespace Game.Core.Items;

/// <summary>
/// A single slot in an <see cref="InventoryContainer"/>.
/// Wraps an <see cref="ItemInstance"/> to provide inventory-specific context
/// while reusing the core item runtime model.
/// </summary>
public class InventoryEntry
{
    /// <summary>The underlying item instance.</summary>
    public ItemInstance Item { get; }

    // ── Convenience accessors ──

    /// <summary>Definition ID of the held item.</summary>
    public string DefinitionId => Item.DefinitionId;

    /// <summary>Current quantity in this slot.</summary>
    public int Quantity
    {
        get => Item.Quantity;
        set => Item.Quantity = value;
    }

    public InventoryEntry(ItemInstance item)
    {
        Item = item;
    }

    /// <summary>Shorthand factory.</summary>
    public static InventoryEntry Of(string definitionId, int quantity = 1)
        => new(new ItemInstance(definitionId, quantity));
}
