namespace Game.Core.Items;

/// <summary>
/// Defines a behaviour that an item can apply to a player.
/// Implementations should be stateless and reusable across definitions.
/// </summary>
public interface IItemEffect
{
    /// <summary>Unique identifier for this effect type.</summary>
    string EffectId { get; }

    /// <summary>Whether this effect can currently be applied to the player.</summary>
    bool CanApply(Player player);

    /// <summary>Apply the effect to the player.</summary>
    void Apply(Player player);
}
