namespace Game.Core.Items;

/// <summary>
/// Material-specific definition data (immutable).
/// Attached to an <see cref="ItemDefinition"/> when Category == Material.
/// <para>
/// Additional crafting metadata can be expressed via <see cref="ItemDefinition.Tags"/>
/// and this record's <see cref="MaterialType"/> property.
/// </para>
/// </summary>
public record MaterialData(string MaterialType);
