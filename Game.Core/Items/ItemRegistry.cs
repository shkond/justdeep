namespace Game.Core.Items;

/// <summary>
/// Central registry of <see cref="ItemDefinition"/>s.
/// Provides lookup by ID and a factory for the default set of sample items.
/// </summary>
public class ItemRegistry
{
    private readonly Dictionary<string, ItemDefinition> _definitions = new();

    /// <summary>Register a definition. Overwrites if the ID already exists.</summary>
    public void Register(ItemDefinition definition)
    {
        _definitions[definition.Id] = definition;
    }

    /// <summary>Look up a definition by ID. Returns null if not found.</summary>
    public ItemDefinition? Get(string id)
    {
        return _definitions.GetValueOrDefault(id);
    }

    /// <summary>All registered definitions.</summary>
    public IReadOnlyCollection<ItemDefinition> GetAll() => _definitions.Values;

    /// <summary>
    /// Creates a registry pre-loaded with sample items:
    /// iron_sword (Equipment), healing_potion (Consumable), iron_ore (Material).
    /// </summary>
    public static ItemRegistry CreateDefault()
    {
        var registry = new ItemRegistry();

        // ── Equipment: 鉄の剣 ──
        registry.Register(new ItemDefinition(
            id: "iron_sword",
            name: "鉄の剣",
            description: "鍛冶屋で鍛えられた標準的な剣。",
            category: ItemCategory.Equipment,
            maxStack: 1,
            unitWeight: 3.0,
            basePrice: 120,
            tags: ["weapon", "sword", "metal"],
            equipmentData: new EquipmentData(
                Slot: EquipmentSlot.Weapon,
                AttackBonus: 5)));

        // ── Consumable: 回復ポーション ──
        registry.Register(new ItemDefinition(
            id: "healing_potion",
            name: "回復ポーション",
            description: "飲むとHPが50回復する薬。",
            category: ItemCategory.Consumable,
            maxStack: 10,
            unitWeight: 0.5,
            basePrice: 30,
            tags: ["potion", "heal"],
            consumableData: new ConsumableData(new HealEffect(50))));

        // ── Material: 鉄鉱石 ──
        registry.Register(new ItemDefinition(
            id: "iron_ore",
            name: "鉄鉱石",
            description: "精錬すると鉄のインゴットになる鉱石。",
            category: ItemCategory.Material,
            maxStack: 50,
            unitWeight: 2.0,
            basePrice: 15,
            tags: ["smelt", "craft", "ore"],
            materialData: new MaterialData(MaterialType: "Metal")));

        return registry;
    }
}
