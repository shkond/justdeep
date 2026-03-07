namespace Game.Core.Items;

/// <summary>
/// Consumable-specific definition data (immutable).
/// Attached to an <see cref="ItemDefinition"/> when Category == Consumable.
/// </summary>
public record ConsumableData(IItemEffect Effect);
