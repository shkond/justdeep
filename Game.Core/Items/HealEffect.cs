namespace Game.Core.Items;

/// <summary>
/// Restores HP when used. The first concrete <see cref="IItemEffect"/>.
/// </summary>
public class HealEffect : IItemEffect
{
    public string EffectId => "heal";
    public int Amount { get; }

    public HealEffect(int amount)
    {
        Amount = amount;
    }

    public bool CanApply(Player player)
        => player.IsAlive && player.CurrentHp < player.MaxHp;

    public void Apply(Player player)
        => player.Heal(Amount);
}
