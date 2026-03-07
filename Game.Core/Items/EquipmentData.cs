namespace Game.Core.Items;

/// <summary>
/// Equipment-specific definition data (immutable).
/// Attached to an <see cref="ItemDefinition"/> when Category == Equipment.
/// </summary>
public record EquipmentData(
    EquipmentSlot Slot,
    int AttackBonus = 0,
    int DefenseBonus = 0,
    int HpBonus = 0,
    int RequiredLevel = 1);
