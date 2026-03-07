using Game.Core;
using Game.Core.Items;
using Xunit;

namespace Game.Core.Tests;

public class InventorySystemTests
{
    // ── Helpers ──

    private static ItemRegistry CreateRegistry()
    {
        return ItemRegistry.CreateDefault();
        // iron_sword:   weight=3.0, maxStack=1
        // healing_potion: weight=0.5, maxStack=10
        // iron_ore:     weight=2.0, maxStack=50
    }

    // ══════════════════════════════════════════════
    //  Weight Calculation
    // ══════════════════════════════════════════════

    [Fact]
    public void ComputeWeight_EmptyContainer_ReturnsZero()
    {
        var container = InventoryContainer.Create(InventoryKind.Player, 100);
        var registry = CreateRegistry();

        Assert.Equal(0, InventoryService.ComputeWeight(container, registry));
    }

    [Fact]
    public void ComputeWeight_WithItems_SumsCorrectly()
    {
        var container = InventoryContainer.Create(InventoryKind.Player, 100);
        var registry = CreateRegistry();

        InventoryService.AddItem(container, "iron_ore", 5, registry); // 5 * 2.0 = 10
        InventoryService.AddItem(container, "healing_potion", 3, registry); // 3 * 0.5 = 1.5

        Assert.Equal(11.5, InventoryService.ComputeWeight(container, registry));
    }

    // ══════════════════════════════════════════════
    //  Add & Stack Merging
    // ══════════════════════════════════════════════

    [Fact]
    public void AddItem_NewItem_CreatesEntry()
    {
        var container = InventoryContainer.Create(InventoryKind.Player, 100);
        var registry = CreateRegistry();

        var result = InventoryService.AddItem(container, "iron_ore", 3, registry);

        Assert.True(result.Success);
        Assert.Single(container.Entries);
        Assert.Equal("iron_ore", container.Entries[0].DefinitionId);
        Assert.Equal(3, container.Entries[0].Quantity);
    }

    [Fact]
    public void AddItem_ExistingStack_MergesQuantity()
    {
        var container = InventoryContainer.Create(InventoryKind.Player, 100);
        var registry = CreateRegistry();

        InventoryService.AddItem(container, "iron_ore", 3, registry);
        InventoryService.AddItem(container, "iron_ore", 7, registry);

        Assert.Single(container.Entries);
        Assert.Equal(10, container.Entries[0].Quantity);
    }

    [Fact]
    public void AddItem_ExceedsMaxStack_CreatesNewEntry()
    {
        var container = InventoryContainer.Create(InventoryKind.Stash); // unlimited
        var registry = CreateRegistry();

        // healing_potion max stack = 10
        var result = InventoryService.AddItem(container, "healing_potion", 15, registry);

        Assert.True(result.Success);
        Assert.Equal(2, container.Entries.Count);
        Assert.Equal(10, container.Entries[0].Quantity);
        Assert.Equal(5, container.Entries[1].Quantity);
    }

    [Fact]
    public void AddItem_WeightExceeded_Fails()
    {
        var container = InventoryContainer.Create(InventoryKind.Player, 5.0);
        var registry = CreateRegistry();

        // iron_ore weight=2.0, try to add 3 = 6.0 > 5.0
        var result = InventoryService.AddItem(container, "iron_ore", 3, registry);

        Assert.False(result.Success);
        Assert.Contains("Weight", result.FailureReason);
        Assert.Empty(container.Entries);
    }

    [Fact]
    public void AddItem_ExactlyAtMaxWeight_Succeeds()
    {
        var container = InventoryContainer.Create(InventoryKind.Player, 6.0);
        var registry = CreateRegistry();

        // iron_ore weight=2.0, add 3 = 6.0 == 6.0
        var result = InventoryService.AddItem(container, "iron_ore", 3, registry);

        Assert.True(result.Success);
    }

    [Fact]
    public void AddItem_UnlimitedContainer_IgnoresWeight()
    {
        var container = InventoryContainer.Create(InventoryKind.Stash); // maxWeight = 0
        var registry = CreateRegistry();

        // iron_ore weight=2.0, add 1000 = 2000.0 — should still pass
        var result = InventoryService.AddItem(container, "iron_ore", 50, registry);

        Assert.True(result.Success);
    }

    [Fact]
    public void AddItem_UnknownItem_Fails()
    {
        var container = InventoryContainer.Create(InventoryKind.Player, 100);
        var registry = CreateRegistry();

        var result = InventoryService.AddItem(container, "nonexistent", 1, registry);

        Assert.False(result.Success);
        Assert.Contains("Unknown", result.FailureReason);
    }

    [Fact]
    public void AddItem_ZeroQuantity_Fails()
    {
        var container = InventoryContainer.Create(InventoryKind.Player, 100);
        var registry = CreateRegistry();

        var result = InventoryService.AddItem(container, "iron_ore", 0, registry);

        Assert.False(result.Success);
    }

    // ══════════════════════════════════════════════
    //  Remove
    // ══════════════════════════════════════════════

    [Fact]
    public void RemoveItem_PartialQuantity_DecreasesStack()
    {
        var container = InventoryContainer.Create(InventoryKind.Player, 100);
        var registry = CreateRegistry();

        InventoryService.AddItem(container, "iron_ore", 10, registry);
        var result = InventoryService.RemoveItem(container, "iron_ore", 3);

        Assert.True(result.Success);
        Assert.Single(container.Entries);
        Assert.Equal(7, container.Entries[0].Quantity);
    }

    [Fact]
    public void RemoveItem_AllQuantity_RemovesEntry()
    {
        var container = InventoryContainer.Create(InventoryKind.Player, 100);
        var registry = CreateRegistry();

        InventoryService.AddItem(container, "iron_ore", 5, registry);
        var result = InventoryService.RemoveItem(container, "iron_ore", 5);

        Assert.True(result.Success);
        Assert.Empty(container.Entries);
    }

    [Fact]
    public void RemoveItem_NotEnough_Fails()
    {
        var container = InventoryContainer.Create(InventoryKind.Player, 100);
        var registry = CreateRegistry();

        InventoryService.AddItem(container, "iron_ore", 3, registry);
        var result = InventoryService.RemoveItem(container, "iron_ore", 5);

        Assert.False(result.Success);
        // Original should be untouched
        Assert.Equal(3, container.Entries[0].Quantity);
    }

    // ══════════════════════════════════════════════
    //  Container-to-Container Move
    // ══════════════════════════════════════════════

    [Fact]
    public void MoveItem_LootToPlayer_Success()
    {
        var registry = CreateRegistry();
        var loot = InventoryContainer.Create(InventoryKind.Loot);
        var player = InventoryContainer.Create(InventoryKind.Player, 100);

        InventoryService.AddItem(loot, "iron_ore", 5, registry);
        var result = InventoryService.MoveItem(loot, player, "iron_ore", 5, registry);

        Assert.True(result.Success);
        Assert.Empty(loot.Entries);
        Assert.Single(player.Entries);
        Assert.Equal(5, player.Entries[0].Quantity);
    }

    [Fact]
    public void MoveItem_PlayerToStash_Success()
    {
        var registry = CreateRegistry();
        var player = InventoryContainer.Create(InventoryKind.Player, 100);
        var stash = InventoryContainer.Create(InventoryKind.Stash);

        InventoryService.AddItem(player, "healing_potion", 5, registry);
        var result = InventoryService.MoveItem(player, stash, "healing_potion", 3, registry);

        Assert.True(result.Success);
        Assert.Equal(2, player.Entries[0].Quantity);
        Assert.Single(stash.Entries);
        Assert.Equal(3, stash.Entries[0].Quantity);
    }

    [Fact]
    public void MoveItem_StashToPlayer_Success()
    {
        var registry = CreateRegistry();
        var stash = InventoryContainer.Create(InventoryKind.Stash);
        var player = InventoryContainer.Create(InventoryKind.Player, 100);

        InventoryService.AddItem(stash, "iron_sword", 1, registry);
        var result = InventoryService.MoveItem(stash, player, "iron_sword", 1, registry);

        Assert.True(result.Success);
        Assert.Empty(stash.Entries);
        Assert.Single(player.Entries);
    }

    [Fact]
    public void MoveItem_WeightExceeded_FailsAndSourceUnchanged()
    {
        var registry = CreateRegistry();
        var stash = InventoryContainer.Create(InventoryKind.Stash);
        var player = InventoryContainer.Create(InventoryKind.Player, 3.0);

        InventoryService.AddItem(stash, "iron_ore", 5, registry); // weight=2.0 each
        var result = InventoryService.MoveItem(stash, player, "iron_ore", 5, registry);

        Assert.False(result.Success);
        // Source should be untouched
        Assert.Equal(5, stash.Entries[0].Quantity);
        Assert.Empty(player.Entries);
    }

    [Fact]
    public void MoveItem_PartialQuantity_SourceAndDestCorrect()
    {
        var registry = CreateRegistry();
        var loot = InventoryContainer.Create(InventoryKind.Loot);
        var player = InventoryContainer.Create(InventoryKind.Player, 100);

        InventoryService.AddItem(loot, "iron_ore", 10, registry);
        var result = InventoryService.MoveItem(loot, player, "iron_ore", 4, registry);

        Assert.True(result.Success);
        Assert.Equal(6, loot.Entries[0].Quantity);
        Assert.Equal(4, player.Entries[0].Quantity);
    }

    [Fact]
    public void MoveItem_NotEnoughInSource_Fails()
    {
        var registry = CreateRegistry();
        var loot = InventoryContainer.Create(InventoryKind.Loot);
        var player = InventoryContainer.Create(InventoryKind.Player, 100);

        InventoryService.AddItem(loot, "iron_ore", 2, registry);
        var result = InventoryService.MoveItem(loot, player, "iron_ore", 5, registry);

        Assert.False(result.Success);
        Assert.Equal(2, loot.Entries[0].Quantity); // unchanged
    }

    [Fact]
    public void MoveItem_MergesIntoExistingStack()
    {
        var registry = CreateRegistry();
        var loot = InventoryContainer.Create(InventoryKind.Loot);
        var player = InventoryContainer.Create(InventoryKind.Player, 100);

        InventoryService.AddItem(player, "iron_ore", 3, registry);
        InventoryService.AddItem(loot, "iron_ore", 5, registry);

        var result = InventoryService.MoveItem(loot, player, "iron_ore", 5, registry);

        Assert.True(result.Success);
        Assert.Single(player.Entries);
        Assert.Equal(8, player.Entries[0].Quantity);
    }

    // ══════════════════════════════════════════════
    //  InventoryContainer creation
    // ══════════════════════════════════════════════

    [Fact]
    public void InventoryContainer_Create_SetsKindAndRules()
    {
        var container = InventoryContainer.Create(InventoryKind.Player, 50.0);

        Assert.Equal(InventoryKind.Player, container.Kind);
        Assert.Equal(50.0, container.Rules.MaxWeight);
        Assert.Empty(container.Entries);
    }

    [Fact]
    public void InventoryRules_Unlimited_HasZeroMaxWeight()
    {
        var rules = InventoryRules.Unlimited;
        Assert.Equal(0, rules.MaxWeight);
    }

    // ══════════════════════════════════════════════
    //  GameSession Integration
    // ══════════════════════════════════════════════

    [Fact]
    public void GameSession_HasDefaultPlayerAndStashContainers()
    {
        var session = new GameSession(new Player("Hero"));

        Assert.True(session.Inventories.ContainsKey(InventoryKind.Player));
        Assert.True(session.Inventories.ContainsKey(InventoryKind.Stash));

        var playerContainer = session.Inventories[InventoryKind.Player];
        Assert.Equal(InventoryKind.Player, playerContainer.Kind);
        Assert.Equal(50.0, playerContainer.Rules.MaxWeight); // default CarryCapacity

        var stash = session.Inventories[InventoryKind.Stash];
        Assert.Equal(0, stash.Rules.MaxWeight); // unlimited
    }

    [Fact]
    public void GameSession_Snapshot_RestoresInventories()
    {
        var registry = CreateRegistry();
        var session = new GameSession(new Player("Hero"));

        // Add items to player inventory
        var playerInv = session.Inventories[InventoryKind.Player];
        InventoryService.AddItem(playerInv, "iron_ore", 5, registry);
        InventoryService.AddItem(playerInv, "healing_potion", 3, registry);

        // Add items to stash
        var stash = session.Inventories[InventoryKind.Stash];
        InventoryService.AddItem(stash, "iron_sword", 1, registry);

        // Snapshot and restore
        var snapshot = session.ToSnapshot();
        var restored = GameSession.FromSnapshot(snapshot);

        // Verify player inventory
        var restoredPlayerInv = restored.Inventories[InventoryKind.Player];
        Assert.Equal(50.0, restoredPlayerInv.Rules.MaxWeight);
        Assert.Equal(2, restoredPlayerInv.Entries.Count);

        var oreEntry = restoredPlayerInv.Entries.First(e => e.DefinitionId == "iron_ore");
        Assert.Equal(5, oreEntry.Quantity);
        var potionEntry = restoredPlayerInv.Entries.First(e => e.DefinitionId == "healing_potion");
        Assert.Equal(3, potionEntry.Quantity);

        // Verify stash
        var restoredStash = restored.Inventories[InventoryKind.Stash];
        Assert.Single(restoredStash.Entries);
        Assert.Equal("iron_sword", restoredStash.Entries[0].DefinitionId);
    }

    [Fact]
    public void GameSession_Snapshot_PreservesCarryCapacity()
    {
        var player = new Player("Hero") { CarryCapacity = 75.0 };
        var session = new GameSession(player);

        var snapshot = session.ToSnapshot();
        var restored = GameSession.FromSnapshot(snapshot);

        Assert.Equal(75.0, restored.Player.CarryCapacity);
    }

    // ══════════════════════════════════════════════
    //  Player CarryCapacity
    // ══════════════════════════════════════════════

    [Fact]
    public void Player_DefaultCarryCapacity_Is50()
    {
        var player = new Player("Hero");
        Assert.Equal(50.0, player.CarryCapacity);
    }

    [Fact]
    public void Player_LevelUp_IncreasesCarryCapacity()
    {
        var player = new Player("Hero");
        player.GainExperience(100); // Level 1 -> 2

        Assert.Equal(2, player.Level);
        Assert.Equal(55.0, player.CarryCapacity);
    }

    // ══════════════════════════════════════════════
    //  TransferResult
    // ══════════════════════════════════════════════

    [Fact]
    public void TransferResult_Ok_IsSuccessful()
    {
        var result = TransferResult.Ok();
        Assert.True(result.Success);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public void TransferResult_Fail_HasReason()
    {
        var result = TransferResult.Fail("Too heavy");
        Assert.False(result.Success);
        Assert.Equal("Too heavy", result.FailureReason);
    }

    // ══════════════════════════════════════════════
    //  InventoryKind extensibility
    // ══════════════════════════════════════════════

    [Fact]
    public void InventoryContainer_SupportsLootKind()
    {
        var loot = InventoryContainer.Create(InventoryKind.Loot);
        Assert.Equal(InventoryKind.Loot, loot.Kind);
        Assert.Equal(0, loot.Rules.MaxWeight); // unlimited by default
    }
}
