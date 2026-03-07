namespace Game.Core;

public class Party
{
    private readonly List<Player> _members;

    public IReadOnlyList<Player> Members => _members;

    public Party(IEnumerable<Player> members)
    {
        _members = members?.ToList() ?? throw new ArgumentNullException(nameof(members));
        if (_members.Count == 0)
        {
            throw new ArgumentException("Party must contain at least one member.", nameof(members));
        }
    }

    public bool IsAlive => _members.Any(m => m.IsAlive);
    public bool ShouldRetreat => _members.Any(m => m.ShouldRetreat);
    public bool IsFullyHealed => _members.All(m => m.CurrentHp >= m.MaxHp);
    public bool AnyCritical => _members.Any(m => m.IsAlive && m.HpPercent <= 30);
    public bool AllDown => _members.All(m => !m.IsAlive);
    public bool NeedsRecovery => _members.Any(m => m.CurrentHp < m.MaxHp);

    public IEnumerable<Player> AliveMembers => _members.Where(m => m.IsAlive);

    public Player? FindById(Guid id) => _members.FirstOrDefault(m => m.Id == id);

    public void ForEachAlive(Action<Player> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        foreach (var member in AliveMembers)
        {
            action(member);
        }
    }

    public IReadOnlyList<Player> GetCombatants()
    {
        return AliveMembers.ToList();
    }

    public Player SelectIncomingTarget(XoshiroRng rng)
    {
        ArgumentNullException.ThrowIfNull(rng);

        var alive = AliveMembers.ToList();
        if (alive.Count == 0)
        {
            throw new InvalidOperationException("Cannot select incoming target: no alive party members.");
        }

        return alive[rng.Next(alive.Count)];
    }

    public void DistributeGold(int amount)
    {
        if (amount <= 0) return;
        _members[0].AddGold(amount);
    }

    public void DistributeExperience(int amount)
    {
        if (amount <= 0) return;
        _members[0].GainExperience(amount);
    }
}
