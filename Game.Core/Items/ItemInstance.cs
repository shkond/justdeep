namespace Game.Core.Items;

/// <summary>
/// A runtime instance of an item — the "thing" that a player holds.
/// Points to an <see cref="ItemDefinition"/> by ID and tracks per-instance
/// mutable state (quantity, and future extensions like quality/durability).
/// </summary>
public class ItemInstance
{
    public string DefinitionId { get; }
    public int Quantity { get; set; }

    // ── Future extensions ──
    // public int? Durability { get; set; }
    // public ItemQuality Quality { get; set; }
    // public List<RandomModifier> Modifiers { get; set; }

    public ItemInstance(string definitionId, int quantity = 1)
    {
        DefinitionId = definitionId;
        Quantity = quantity;
    }
}
