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
        var container = InventoryContainer.Create(InventoryKind.Personal, 100);
        var registry = CreateRegistry();

        Assert.Equal(0, InventoryService.ComputeWeight(container, registry));
    }

    [Fact]
    public void ComputeWeight_WithItems_SumsCorrectly()
    {
        var container = InventoryContainer.Create(InventoryKind.Personal, 100);
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
        var container = InventoryContainer.Create(InventoryKind.Personal, 100);
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
        var container = InventoryContainer.Create(InventoryKind.Personal, 100);
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
        var container = InventoryContainer.Create(InventoryKind.Personal, 5.0);
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
        var container = InventoryContainer.Create(InventoryKind.Personal, 6.0);
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
        var container = InventoryContainer.Create(InventoryKind.Personal, 100);
        var registry = CreateRegistry();

        var result = InventoryService.AddItem(container, "nonexistent", 1, registry);

        Assert.False(result.Success);
        Assert.Contains("Unknown", result.FailureReason);
    }

    [Fact]
    public void AddItem_ZeroQuantity_Fails()
    {
        var container = InventoryContainer.Create(InventoryKind.Personal, 100);
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
        var container = InventoryContainer.Create(InventoryKind.Personal, 100);
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
        var container = InventoryContainer.Create(InventoryKind.Personal, 100);
        var registry = CreateRegistry();

        InventoryService.AddItem(container, "iron_ore", 5, registry);
        var result = InventoryService.RemoveItem(container, "iron_ore", 5);

        Assert.True(result.Success);
        Assert.Empty(container.Entries);
    }

    [Fact]
    public void RemoveItem_NotEnough_Fails()
    {
        var container = InventoryContainer.Create(InventoryKind.Personal, 100);
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
        var player = InventoryContainer.Create(InventoryKind.Personal, 100);

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
        var player = InventoryContainer.Create(InventoryKind.Personal, 100);
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
        var player = InventoryContainer.Create(InventoryKind.Personal, 100);

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
        var player = InventoryContainer.Create(InventoryKind.Personal, 3.0);

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
        var player = InventoryContainer.Create(InventoryKind.Personal, 100);

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
        var player = InventoryContainer.Create(InventoryKind.Personal, 100);

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
        var player = InventoryContainer.Create(InventoryKind.Personal, 100);

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
        var container = InventoryContainer.Create(InventoryKind.Personal, 50.0);

        Assert.Equal(InventoryKind.Personal, container.Kind);
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
    public void GameSession_HasDefaultPersonalAndSharedStashInventories()
    {
        var session = new GameSession(new Party([new Player("Hero")]));

        Assert.NotNull(session.Party.Members[0].PersonalInventory);
        Assert.True(session.SharedInventories.ContainsKey(InventoryKind.Stash));

        var personalInventory = session.Party.Members[0].PersonalInventory;
        Assert.Equal(InventoryKind.Personal, personalInventory.Kind);
        Assert.Equal(50.0, personalInventory.Rules.MaxWeight); // default CarryCapacity

        var stash = session.SharedInventories[InventoryKind.Stash];
        Assert.Equal(0, stash.Rules.MaxWeight); // unlimited
    }

    [Fact]
    public void GameSession_Snapshot_RestoresInventories()
    {
        var registry = CreateRegistry();
        var session = new GameSession(new Party([new Player("Hero")]));

        // Add items to player inventory
        var playerInv = session.Party.Members[0].PersonalInventory;
        InventoryService.AddItem(playerInv, "iron_ore", 5, registry);
        InventoryService.AddItem(playerInv, "healing_potion", 3, registry);

        // Add items to stash
        var stash = session.SharedInventories[InventoryKind.Stash];
        InventoryService.AddItem(stash, "iron_sword", 1, registry);

        // Snapshot and restore
        var snapshot = session.ToSnapshot();
        var restored = GameSession.FromSnapshot(snapshot);

        // Verify player inventory
        var restoredPlayerInv = restored.Party.Members[0].PersonalInventory;
        Assert.Equal(50.0, restoredPlayerInv.Rules.MaxWeight);
        Assert.Equal(2, restoredPlayerInv.Entries.Count);

        var oreEntry = restoredPlayerInv.Entries.First(e => e.DefinitionId == "iron_ore");
        Assert.Equal(5, oreEntry.Quantity);
        var potionEntry = restoredPlayerInv.Entries.First(e => e.DefinitionId == "healing_potion");
        Assert.Equal(3, potionEntry.Quantity);

        // Verify stash
        var restoredStash = restored.SharedInventories[InventoryKind.Stash];
        Assert.Single(restoredStash.Entries);
        Assert.Equal("iron_sword", restoredStash.Entries[0].DefinitionId);
    }

    [Fact]
    public void GameSession_Snapshot_RestoresIndependentPersonalInventories_ForMultiplePlayers()
    {
        var registry = CreateRegistry();
        var party = new Party([new Player("Hero"), new Player("Mage")]);
        var session = new GameSession(party);

        InventoryService.AddItem(session.Party.Members[0].PersonalInventory, "iron_ore", 2, registry);
        InventoryService.AddItem(session.Party.Members[1].PersonalInventory, "healing_potion", 7, registry);

        var snapshot = session.ToSnapshot();
        var restored = GameSession.FromSnapshot(snapshot);

        var restoredHero = restored.Party.Members[0].PersonalInventory;
        var restoredMage = restored.Party.Members[1].PersonalInventory;

        Assert.Single(restoredHero.Entries);
        Assert.Equal("iron_ore", restoredHero.Entries[0].DefinitionId);
        Assert.Equal(2, restoredHero.Entries[0].Quantity);

        Assert.Single(restoredMage.Entries);
        Assert.Equal("healing_potion", restoredMage.Entries[0].DefinitionId);
        Assert.Equal(7, restoredMage.Entries[0].Quantity);
    }

    [Fact]
    public void GameSession_FromSnapshot_WhenPersonalInventoryMissing_CreatesEmptyPersonalInventory()
    {
        var session = new GameSession(new Party([new Player("Hero") { CarryCapacity = 75.0 }]));
        var snapshot = session.ToSnapshot();
        snapshot.Players[0].PersonalInventory = null;

        var restored = GameSession.FromSnapshot(snapshot);
        var restoredPlayer = restored.Party.Members[0];

        Assert.NotNull(restoredPlayer.PersonalInventory);
        Assert.Equal(InventoryKind.Personal, restoredPlayer.PersonalInventory.Kind);
        Assert.Empty(restoredPlayer.PersonalInventory.Entries);
        Assert.Equal(75.0, restoredPlayer.PersonalInventory.Rules.MaxWeight);
    }

    [Fact]
    public void GameSession_Snapshot_RestoresSharedInventories_WithSharedKindsOnly()
    {
        var registry = CreateRegistry();
        var session = new GameSession(new Party([new Player("Hero")]));

        var stash = session.GetSharedInventory(InventoryKind.Stash);
        InventoryService.AddItem(stash, "iron_ore", 3, registry);

        session.SharedInventories[InventoryKind.Loot] = InventoryContainer.Create(InventoryKind.Loot);
        InventoryService.AddItem(session.GetSharedInventory(InventoryKind.Loot), "healing_potion", 2, registry);

        var restored = GameSession.FromSnapshot(session.ToSnapshot());

        Assert.All(restored.SharedInventories.Keys, kind =>
            Assert.True(kind is InventoryKind.Stash or InventoryKind.Loot));
        Assert.DoesNotContain(InventoryKind.Personal, restored.SharedInventories.Keys);
    }

    [Fact]
    public void GameSession_FromSnapshot_ThrowsOnInvalidSharedInventoryKind()
    {
        var session = new GameSession(new Party([new Player("Hero")]));
        var snapshot = session.ToSnapshot();

        snapshot.SharedInventories.Add(new Game.Core.Save.InventoryContainerData
        {
            Kind = InventoryKind.Personal,
            MaxWeight = 10,
        });

        Assert.Throws<InvalidOperationException>(() => GameSession.FromSnapshot(snapshot));
    }

    [Fact]
    public void GameSession_Snapshot_PreservesCarryCapacity()
    {
        var player = new Player("Hero") { CarryCapacity = 75.0 };
        var session = new GameSession(new Party([player]));

        var snapshot = session.ToSnapshot();
        var restored = GameSession.FromSnapshot(snapshot);

        Assert.Equal(75.0, restored.Party.Members[0].CarryCapacity);
    }

    // ══════════════════════════════════════════════
    //  Player CarryCapacity
    // ══════════════════════════════════════════════

    [Fact]
    public void Player_DefaultCarryCapacity_Is50()
    {
        var player = new Player("Hero");
        Assert.Equal(50.0, player.CarryCapacity);
        Assert.Equal(InventoryKind.Personal, player.PersonalInventory.Kind);
        Assert.Equal(50.0, player.PersonalInventory.Rules.MaxWeight);
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
