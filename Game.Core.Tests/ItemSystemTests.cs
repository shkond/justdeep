using Game.Core;
using Game.Core.Items;
using Xunit;

namespace Game.Core.Tests;

public class ItemSystemTests
{
    // ══════════════════════════════════════════════
    //  ItemRegistry / ItemDefinition
    // ══════════════════════════════════════════════

    [Fact]
    public void ItemRegistry_CreateDefault_ContainsThreeItems()
    {
        var registry = ItemRegistry.CreateDefault();

        Assert.Equal(3, registry.GetAll().Count);
        Assert.NotNull(registry.Get("iron_sword"));
        Assert.NotNull(registry.Get("healing_potion"));
        Assert.NotNull(registry.Get("iron_ore"));
    }

    [Fact]
    public void ItemDefinition_Equipment_HasEquipmentData()
    {
        var registry = ItemRegistry.CreateDefault();
        var def = registry.Get("iron_sword")!;

        Assert.Equal(ItemCategory.Equipment, def.Category);
        Assert.NotNull(def.EquipmentData);
        Assert.Equal(EquipmentSlot.Weapon, def.EquipmentData!.Slot);
        Assert.Equal(5, def.EquipmentData.AttackBonus);
        Assert.Null(def.ConsumableData);
        Assert.Null(def.MaterialData);
    }

    [Fact]
    public void ItemDefinition_Consumable_HasConsumableData()
    {
        var registry = ItemRegistry.CreateDefault();
        var def = registry.Get("healing_potion")!;

        Assert.Equal(ItemCategory.Consumable, def.Category);
        Assert.NotNull(def.ConsumableData);
        Assert.IsType<HealEffect>(def.ConsumableData!.Effect);
        Assert.Null(def.EquipmentData);
        Assert.Null(def.MaterialData);
    }

    [Fact]
    public void ItemDefinition_Material_HasMaterialData()
    {
        var registry = ItemRegistry.CreateDefault();
        var def = registry.Get("iron_ore")!;

        Assert.Equal(ItemCategory.Material, def.Category);
        Assert.NotNull(def.MaterialData);
        Assert.Equal("Metal", def.MaterialData!.MaterialType);
        Assert.Contains("smelt", def.Tags);
        Assert.Contains("craft", def.Tags);
        Assert.Null(def.EquipmentData);
        Assert.Null(def.ConsumableData);
    }

    [Fact]
    public void ItemRegistry_Get_UnknownId_ReturnsNull()
    {
        var registry = ItemRegistry.CreateDefault();

        Assert.Null(registry.Get("nonexistent_item"));
    }

    // ══════════════════════════════════════════════
    //  ItemInstance
    // ══════════════════════════════════════════════

    [Fact]
    public void ItemInstance_DefaultQuantity_IsOne()
    {
        var instance = new ItemInstance("iron_sword");

        Assert.Equal(1, instance.Quantity);
    }

    [Fact]
    public void ItemInstance_QuantityCanBeSet()
    {
        var instance = new ItemInstance("healing_potion", 5);

        Assert.Equal(5, instance.Quantity);
        instance.Quantity = 3;
        Assert.Equal(3, instance.Quantity);
    }

    // ══════════════════════════════════════════════
    //  HealEffect
    // ══════════════════════════════════════════════

    [Fact]
    public void HealEffect_Apply_HealsPlayer()
    {
        var player = new Player("Test") { CurrentHp = 50 };
        var effect = new HealEffect(30);

        effect.Apply(player);

        Assert.Equal(80, player.CurrentHp);
    }

    [Fact]
    public void HealEffect_Apply_DoesNotOverheal()
    {
        var player = new Player("Test") { CurrentHp = 90 };
        var effect = new HealEffect(50);

        effect.Apply(player);

        Assert.Equal(player.MaxHp, player.CurrentHp);
    }

    [Fact]
    public void HealEffect_CanApply_FullHp_ReturnsFalse()
    {
        var player = new Player("Test"); // Full HP
        var effect = new HealEffect(30);

        Assert.False(effect.CanApply(player));
    }

    [Fact]
    public void HealEffect_CanApply_Dead_ReturnsFalse()
    {
        var player = new Player("Test") { CurrentHp = 0 };
        var effect = new HealEffect(30);

        Assert.False(effect.CanApply(player));
    }

    [Fact]
    public void HealEffect_CanApply_Damaged_ReturnsTrue()
    {
        var player = new Player("Test") { CurrentHp = 50 };
        var effect = new HealEffect(30);

        Assert.True(effect.CanApply(player));
    }

    // ══════════════════════════════════════════════
    //  Equipment
    // ══════════════════════════════════════════════

    [Fact]
    public void Player_Equip_IncreasesStats()
    {
        var registry = ItemRegistry.CreateDefault();
        var player = new Player("Test");
        var sword = new ItemInstance("iron_sword");

        int baseTotalAttack = player.TotalAttack;
        player.Equip(sword, registry);

        Assert.Equal(baseTotalAttack + 5, player.TotalAttack);
        Assert.Same(sword, player.Equipment[EquipmentSlot.Weapon]);
    }

    [Fact]
    public void Player_Unequip_RestoresOriginalStats()
    {
        var registry = ItemRegistry.CreateDefault();
        var player = new Player("Test");
        var sword = new ItemInstance("iron_sword");
        int originalTotalAttack = player.TotalAttack;

        player.Equip(sword, registry);
        Assert.Equal(originalTotalAttack + 5, player.TotalAttack);

        var removed = player.Unequip(EquipmentSlot.Weapon);
        Assert.Same(sword, removed);
        Assert.Equal(originalTotalAttack, player.TotalAttack);
        Assert.Null(player.Equipment[EquipmentSlot.Weapon]);
    }

    [Fact]
    public void Player_Equip_ReplacesExistingItem()
    {
        var registry = ItemRegistry.CreateDefault();

        // Register a second weapon
        registry.Register(new ItemDefinition(
            id: "steel_sword",
            name: "鋼の剣",
            description: "より強い剣。",
            category: ItemCategory.Equipment,
            equipmentData: new EquipmentData(EquipmentSlot.Weapon, AttackBonus: 10)));

        var player = new Player("Test");
        var ironSword = new ItemInstance("iron_sword");
        var steelSword = new ItemInstance("steel_sword");

        player.Equip(ironSword, registry);
        Assert.Equal(player.Attack + 5, player.TotalAttack);

        var replaced = player.Equip(steelSword, registry);
        Assert.Same(ironSword, replaced);
        Assert.Equal(player.Attack + 10, player.TotalAttack);
        Assert.Same(steelSword, player.Equipment[EquipmentSlot.Weapon]);
    }

    [Fact]
    public void Player_CanEquip_LevelCheck()
    {
        var registry = ItemRegistry.CreateDefault();

        registry.Register(new ItemDefinition(
            id: "high_level_armor",
            name: "伝説の鎧",
            description: "レベル10以上必要。",
            category: ItemCategory.Equipment,
            equipmentData: new EquipmentData(EquipmentSlot.Armor, DefenseBonus: 20, RequiredLevel: 10)));

        var def = registry.Get("high_level_armor")!;
        var player = new Player("Test"); // Level 1

        Assert.False(player.CanEquip(def));

        player.Level = 10;
        Assert.True(player.CanEquip(def));
    }

    [Fact]
    public void Player_CanEquip_NonEquipment_ReturnsFalse()
    {
        var registry = ItemRegistry.CreateDefault();
        var potionDef = registry.Get("healing_potion")!;
        var player = new Player("Test");

        Assert.False(player.CanEquip(potionDef));
    }

    [Fact]
    public void Player_Equip_LevelTooLow_ReturnsNull()
    {
        var registry = ItemRegistry.CreateDefault();
        registry.Register(new ItemDefinition(
            id: "high_level_sword",
            name: "勇者の剣",
            description: "レベル5必要。",
            category: ItemCategory.Equipment,
            equipmentData: new EquipmentData(EquipmentSlot.Weapon, AttackBonus: 15, RequiredLevel: 5)));

        var player = new Player("Test"); // Level 1
        var sword = new ItemInstance("high_level_sword");

        var result = player.Equip(sword, registry);
        Assert.Null(result);
        Assert.Null(player.Equipment[EquipmentSlot.Weapon]);
    }

    [Fact]
    public void Player_TotalDefense_IncludesEquipmentBonus()
    {
        var registry = ItemRegistry.CreateDefault();
        registry.Register(new ItemDefinition(
            id: "iron_armor",
            name: "鉄の鎧",
            description: "標準的な防具。",
            category: ItemCategory.Equipment,
            equipmentData: new EquipmentData(EquipmentSlot.Armor, DefenseBonus: 8)));

        var player = new Player("Test");
        int baseTotalDefense = player.TotalDefense;

        player.Equip(new ItemInstance("iron_armor"), registry);
        Assert.Equal(baseTotalDefense + 8, player.TotalDefense);
    }

    // ══════════════════════════════════════════════
    //  Item Use (Consumable)
    // ══════════════════════════════════════════════

    [Fact]
    public void Player_UseItem_Consumable_HealsAndReducesQuantity()
    {
        var registry = ItemRegistry.CreateDefault();
        var player = new Player("Test") { CurrentHp = 50 };
        var potion = new ItemInstance("healing_potion", 3);

        bool used = player.UseItem(potion, registry);

        Assert.True(used);
        Assert.Equal(100, player.CurrentHp); // 50 + 50 = 100 = MaxHp
        Assert.Equal(2, potion.Quantity);
    }

    [Fact]
    public void Player_UseItem_NonConsumable_ReturnsFalse()
    {
        var registry = ItemRegistry.CreateDefault();
        var player = new Player("Test") { CurrentHp = 50 };
        var sword = new ItemInstance("iron_sword");

        bool used = player.UseItem(sword, registry);

        Assert.False(used);
    }

    [Fact]
    public void Player_UseItem_ZeroQuantity_ReturnsFalse()
    {
        var registry = ItemRegistry.CreateDefault();
        var player = new Player("Test") { CurrentHp = 50 };
        var potion = new ItemInstance("healing_potion", 0);

        bool used = player.UseItem(potion, registry);

        Assert.False(used);
        Assert.Equal(50, player.CurrentHp);
    }

    [Fact]
    public void Player_UseItem_FullHp_ReturnsFalse()
    {
        var registry = ItemRegistry.CreateDefault();
        var player = new Player("Test"); // Full HP
        var potion = new ItemInstance("healing_potion", 3);

        bool used = player.UseItem(potion, registry);

        Assert.False(used);
        Assert.Equal(3, potion.Quantity); // Not consumed
    }

    [Fact]
    public void Player_UseItem_Dead_ReturnsFalse()
    {
        var registry = ItemRegistry.CreateDefault();
        var player = new Player("Test") { CurrentHp = 0 };
        var potion = new ItemInstance("healing_potion", 3);

        bool used = player.UseItem(potion, registry);

        Assert.False(used);
        Assert.Equal(3, potion.Quantity);
    }

    // ══════════════════════════════════════════════
    //  GameEngine integration
    // ══════════════════════════════════════════════

    [Fact]
    public void GameEngine_HasItemRegistry()
    {
        var engine = new GameEngine();

        Assert.NotNull(engine.ItemRegistry);
        Assert.Equal(3, engine.ItemRegistry.GetAll().Count);
    }

    [Fact]
    public void GameEngine_StartNewGame_ReInitializesItemRegistry()
    {
        var engine = new GameEngine();
        engine.StartNewGame(["Hero"], 42);

        Assert.NotNull(engine.ItemRegistry);
        Assert.NotNull(engine.ItemRegistry.Get("iron_sword"));
    }
}
