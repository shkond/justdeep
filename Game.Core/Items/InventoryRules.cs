namespace Game.Core.Items;

/// <summary>
/// Per-container constraints that govern what an
/// <see cref="InventoryContainer"/> will accept.
/// </summary>
public class InventoryRules
{
    /// <summary>
    /// Maximum total weight the container can hold.
    /// A value of <c>0</c> means unlimited.
    /// </summary>
    public double MaxWeight { get; }

    public InventoryRules(double maxWeight = 0)
    {
        MaxWeight = maxWeight;
    }

    /// <summary>Convenience instance: no weight limit.</summary>
    public static InventoryRules Unlimited => new(0);
}
