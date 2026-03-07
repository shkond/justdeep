using Game.Core.Items;

namespace Game.Core;

public class Player
{
    public Guid Id { get; }
    public string Name { get; private set; }
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public int Gold { get; set; }

    // ── Inventory capacity ──
    /// <summary>Maximum total weight the player can carry.</summary>
    public double CarryCapacity { get; set; } = 50.0;

    // ── Equipment ──
    public Dictionary<EquipmentSlot, ItemInstance?> Equipment { get; } = new()
    {
        [EquipmentSlot.Weapon] = null,
        [EquipmentSlot.Armor] = null,
        [EquipmentSlot.Accessory] = null,
    };

    public Player(string name)
        : this(Guid.NewGuid(), name)
    {
    }

    public Player(Guid id, string name)
    {
        Id = id;
        Name = name;
        Level = 1;
        MaxHp = 100;
        CurrentHp = MaxHp;
        Attack = 10;
        Defense = 5;
        Experience = 0;
        Gold = 50;
    }

    public bool IsAlive => CurrentHp > 0;

    /// <summary>Current HP as a percentage (0–100).</summary>
    public double HpPercent => MaxHp > 0 ? (double)CurrentHp / MaxHp * 100 : 0;

    /// <summary>True when HP falls to 30% or below — signals retreat.</summary>
    public bool ShouldRetreat => IsAlive && HpPercent <= 30;

    // ── Equipment stat bonuses ──

    /// <summary>Sum of AttackBonus from all equipped items.</summary>
    public int EquipmentAttackBonus => ComputeEquipmentBonus(e => e.AttackBonus);

    /// <summary>Sum of DefenseBonus from all equipped items.</summary>
    public int EquipmentDefenseBonus => ComputeEquipmentBonus(e => e.DefenseBonus);

    /// <summary>Base Attack + equipment bonus.</summary>
    public int TotalAttack => Attack + EquipmentAttackBonus;

    /// <summary>Base Defense + equipment bonus.</summary>
    public int TotalDefense => Defense + EquipmentDefenseBonus;

    private int ComputeEquipmentBonus(Func<EquipmentData, int> selector)
    {
        // We need the registry to resolve definitions — but to keep
        // Player free of registry dependency at the field level, we
        // cache resolved stats.  For now we store EquipmentData directly
        // in a parallel dictionary.
        return _resolvedEquipment.Values
            .Where(d => d != null)
            .Sum(d => selector(d!));
    }

    // Parallel cache of resolved EquipmentData per slot
    private readonly Dictionary<EquipmentSlot, EquipmentData?> _resolvedEquipment = new()
    {
        [EquipmentSlot.Weapon] = null,
        [EquipmentSlot.Armor] = null,
        [EquipmentSlot.Accessory] = null,
    };

    // ── Combat ──

    public void TakeDamage(int damage)
    {
        int actualDamage = Math.Max(1, damage - TotalDefense);
        CurrentHp = Math.Max(0, CurrentHp - actualDamage);
    }

    public void Heal(int amount)
    {
        CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
    }

    // ── Experience / Gold ──

    public void GainExperience(int exp)
    {
        Experience += exp;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        int expNeeded = Level * 100;
        while (Experience >= expNeeded)
        {
            Experience -= expNeeded;
            Level++;
            MaxHp += 20;
            CurrentHp = MaxHp;
            Attack += 3;
            Defense += 2;
            CarryCapacity += 5;
            expNeeded = Level * 100;
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;
    }

    public bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            return true;
        }
        return false;
    }

    // ══════════════════════════════════════════════
    //  Equipment API
    // ══════════════════════════════════════════════

    /// <summary>
    /// Whether the player meets the requirements to equip this item.
    /// </summary>
    public bool CanEquip(ItemDefinition definition)
    {
        if (definition.Category != ItemCategory.Equipment) return false;
        if (definition.EquipmentData is null) return false;
        return Level >= definition.EquipmentData.RequiredLevel;
    }

    /// <summary>
    /// Equip an item. Returns the previously equipped item in that slot
    /// (null if slot was empty). Returns null and does nothing if the
    /// item cannot be equipped.
    /// </summary>
    public ItemInstance? Equip(ItemInstance item, ItemRegistry registry)
    {
        var def = registry.Get(item.DefinitionId);
        if (def is null || !CanEquip(def)) return null;

        var slot = def.EquipmentData!.Slot;
        var previous = Equipment[slot];

        Equipment[slot] = item;
        _resolvedEquipment[slot] = def.EquipmentData;

        return previous;
    }

    /// <summary>
    /// Remove the item in the given slot. Returns the removed item, or null.
    /// </summary>
    public ItemInstance? Unequip(EquipmentSlot slot)
    {
        var previous = Equipment[slot];
        Equipment[slot] = null;
        _resolvedEquipment[slot] = null;
        return previous;
    }

    // ══════════════════════════════════════════════
    //  Item Use API
    // ══════════════════════════════════════════════

    /// <summary>
    /// Use a consumable item. Returns true if the item was used
    /// (effect applied and quantity decremented).
    /// </summary>
    public bool UseItem(ItemInstance item, ItemRegistry registry)
    {
        var def = registry.Get(item.DefinitionId);
        if (def is null) return false;
        if (def.Category != ItemCategory.Consumable) return false;
        if (def.ConsumableData?.Effect is null) return false;
        if (item.Quantity <= 0) return false;

        var effect = def.ConsumableData.Effect;
        if (!effect.CanApply(this)) return false;

        effect.Apply(this);
        item.Quantity--;
        return true;
    }
}
