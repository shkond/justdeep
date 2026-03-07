namespace Game.Core.Items;

/// <summary>
/// Stateless service that performs inventory operations —
/// add, remove, move, and weight calculations.
/// <para>
/// All methods take the containers and <see cref="ItemRegistry"/> as
/// parameters so the service itself holds no state.
/// </para>
/// </summary>
public static class InventoryService
{
    // ══════════════════════════════════════════════
    //  Weight
    // ══════════════════════════════════════════════

    /// <summary>
    /// Compute the total weight of all items in a container.
    /// </summary>
    public static double ComputeWeight(InventoryContainer container, ItemRegistry registry)
    {
        double total = 0;
        foreach (var entry in container.Entries)
        {
            var def = registry.Get(entry.DefinitionId);
            if (def is not null)
                total += def.UnitWeight * entry.Quantity;
        }
        return total;
    }

    /// <summary>
    /// Check whether adding <paramref name="quantity"/> units of the given
    /// item would exceed the container's weight limit.
    /// </summary>
    public static bool CanAdd(
        InventoryContainer container, string definitionId, int quantity, ItemRegistry registry)
    {
        if (container.Rules.MaxWeight <= 0) return true; // unlimited

        var def = registry.Get(definitionId);
        if (def is null) return false;

        double currentWeight = ComputeWeight(container, registry);
        double additionalWeight = def.UnitWeight * quantity;
        return currentWeight + additionalWeight <= container.Rules.MaxWeight;
    }

    // ══════════════════════════════════════════════
    //  Add
    // ══════════════════════════════════════════════

    /// <summary>
    /// Add items to a container. Merges into existing stacks where possible,
    /// respecting <see cref="ItemDefinition.MaxStack"/>.
    /// Returns <see cref="TransferResult.Fail"/> if weight would be exceeded.
    /// </summary>
    public static TransferResult AddItem(
        InventoryContainer container, string definitionId, int quantity, ItemRegistry registry)
    {
        if (quantity <= 0)
            return TransferResult.Fail("Quantity must be positive.");

        var def = registry.Get(definitionId);
        if (def is null)
            return TransferResult.Fail($"Unknown item: {definitionId}");

        if (!CanAdd(container, definitionId, quantity, registry))
            return TransferResult.Fail("Weight limit exceeded.");

        int remaining = quantity;

        // Try to fill existing stacks first
        foreach (var entry in container.Entries)
        {
            if (remaining <= 0) break;
            if (entry.DefinitionId != definitionId) continue;

            int space = def.MaxStack - entry.Quantity;
            if (space <= 0) continue;

            int toAdd = Math.Min(space, remaining);
            entry.Quantity += toAdd;
            remaining -= toAdd;
        }

        // Create new stacks for the remainder
        while (remaining > 0)
        {
            int stackSize = Math.Min(def.MaxStack, remaining);
            container.Entries.Add(InventoryEntry.Of(definitionId, stackSize));
            remaining -= stackSize;
        }

        return TransferResult.Ok();
    }

    // ══════════════════════════════════════════════
    //  Remove
    // ══════════════════════════════════════════════

    /// <summary>
    /// Remove items from a container. Removes from last stacks first.
    /// Entries whose quantity reaches 0 are pruned.
    /// </summary>
    public static TransferResult RemoveItem(
        InventoryContainer container, string definitionId, int quantity)
    {
        if (quantity <= 0)
            return TransferResult.Fail("Quantity must be positive.");

        int available = container.Entries
            .Where(e => e.DefinitionId == definitionId)
            .Sum(e => e.Quantity);

        if (available < quantity)
            return TransferResult.Fail("Not enough items.");

        int remaining = quantity;

        // Remove from the tail so we drain newest stacks first
        for (int i = container.Entries.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var entry = container.Entries[i];
            if (entry.DefinitionId != definitionId) continue;

            int toRemove = Math.Min(entry.Quantity, remaining);
            entry.Quantity -= toRemove;
            remaining -= toRemove;

            if (entry.Quantity <= 0)
                container.Entries.RemoveAt(i);
        }

        return TransferResult.Ok();
    }

    // ══════════════════════════════════════════════
    //  Move (container-to-container)
    // ══════════════════════════════════════════════

    /// <summary>
    /// Move items between containers. Atomic — if adding to the
    /// destination fails, the source is left unchanged.
    /// </summary>
    public static TransferResult MoveItem(
        InventoryContainer source,
        InventoryContainer destination,
        string definitionId,
        int quantity,
        ItemRegistry registry)
    {
        if (quantity <= 0)
            return TransferResult.Fail("Quantity must be positive.");

        // Verify source has enough
        int available = source.Entries
            .Where(e => e.DefinitionId == definitionId)
            .Sum(e => e.Quantity);

        if (available < quantity)
            return TransferResult.Fail("Not enough items in source.");

        // Check destination weight
        if (!CanAdd(destination, definitionId, quantity, registry))
            return TransferResult.Fail("Destination weight limit exceeded.");

        // Perform the transfer (remove then add)
        var removeResult = RemoveItem(source, definitionId, quantity);
        if (!removeResult.Success)
            return removeResult;

        var addResult = AddItem(destination, definitionId, quantity, registry);
        if (!addResult.Success)
        {
            // Rollback: put items back into source
            AddItem(source, definitionId, quantity, registry);
            return addResult;
        }

        return TransferResult.Ok();
    }
}
