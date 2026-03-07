namespace Game.Core.Items;

/// <summary>
/// Immutable definition of an item — the "template" that describes what
/// an item IS.  Shared across all instances of the same item.
/// <para>
/// Category-specific behaviour is expressed through nullable sub-records
/// (<see cref="EquipmentData"/>, <see cref="ConsumableData"/>,
///  <see cref="MaterialData"/>) rather than class inheritance,
/// keeping the type hierarchy flat and extensible.
/// </para>
/// </summary>
public class ItemDefinition
{
    // ── Identity ──
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public ItemCategory Category { get; }

    // ── Stacking / Economy ──
    public int MaxStack { get; }
    public double UnitWeight { get; }
    public int BasePrice { get; }

    // ── Free-form tags (crafting, filtering, search) ──
    public IReadOnlyList<string> Tags { get; }

    // ── Category-specific data (nullable) ──
    public EquipmentData? EquipmentData { get; }
    public ConsumableData? ConsumableData { get; }
    public MaterialData? MaterialData { get; }

    public ItemDefinition(
        string id,
        string name,
        string description,
        ItemCategory category,
        int maxStack = 1,
        double unitWeight = 0,
        int basePrice = 0,
        IReadOnlyList<string>? tags = null,
        EquipmentData? equipmentData = null,
        ConsumableData? consumableData = null,
        MaterialData? materialData = null)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        MaxStack = maxStack;
        UnitWeight = unitWeight;
        BasePrice = basePrice;
        Tags = tags ?? Array.Empty<string>();
        EquipmentData = equipmentData;
        ConsumableData = consumableData;
        MaterialData = materialData;
    }
}
