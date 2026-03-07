using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public class PartyTests
{
    [Fact]
    public void Aggregates_WorkForSingleMemberParty()
    {
        var leader = new Player("Hero");
        var party = new Party([leader]);

        Assert.True(party.IsAlive);
        Assert.False(party.AllDown);
        Assert.True(party.IsFullyHealed);
        Assert.False(party.NeedsRecovery);
        Assert.False(party.ShouldRetreat);
        Assert.False(party.AnyCritical);
    }

    [Fact]
    public void Aggregates_WorkForMultiMemberParty()
    {
        var a = new Player("A") { CurrentHp = 10, MaxHp = 100 };
        var b = new Player("B") { CurrentHp = 0, MaxHp = 100 };
        var c = new Player("C") { CurrentHp = 60, MaxHp = 100 };
        var party = new Party([a, b, c]);

        Assert.True(party.IsAlive);
        Assert.False(party.AllDown);
        Assert.False(party.IsFullyHealed);
        Assert.True(party.NeedsRecovery);
        Assert.True(party.ShouldRetreat);
        Assert.True(party.AnyCritical);
    }

    [Fact]
    public void FindById_ReturnsTargetMember()
    {
        var a = new Player("A");
        var b = new Player("B");
        var party = new Party([a, b]);

        Assert.Same(b, party.FindById(b.Id));
        Assert.Null(party.FindById(Guid.NewGuid()));
    }

    [Fact]
    public void ForEachAlive_VisitsAliveOnly()
    {
        var a = new Player("A") { CurrentHp = 100 };
        var b = new Player("B") { CurrentHp = 0 };
        var c = new Player("C") { CurrentHp = 50 };
        var party = new Party([a, b, c]);

        var visited = new List<string>();
        party.ForEachAlive(p => visited.Add(p.Name));

        Assert.Equal(2, visited.Count);
        Assert.Contains("A", visited);
        Assert.Contains("C", visited);
        Assert.DoesNotContain("B", visited);
    }

    [Fact]
    public void GetCombatants_ReturnsAliveMembers()
    {
        var a = new Player("A") { CurrentHp = 100 };
        var b = new Player("B") { CurrentHp = 0 };
        var party = new Party([a, b]);

        var combatants = party.GetCombatants();

        Assert.Single(combatants);
        Assert.Same(a, combatants[0]);
    }

    [Fact]
    public void SelectIncomingTarget_SelectsOnlyAliveMembers()
    {
        var rng = new XoshiroRng(1234);
        var a = new Player("A") { CurrentHp = 100 };
        var b = new Player("B") { CurrentHp = 0 };
        var c = new Player("C") { CurrentHp = 80 };
        var party = new Party([a, b, c]);

        for (int i = 0; i < 20; i++)
        {
            var target = party.SelectIncomingTarget(rng);
            Assert.True(target.IsAlive);
            Assert.True(target == a || target == c);
        }
    }

    [Fact]
    public void DistributeGold_AndExperience_GoesToLeader()
    {
        var leader = new Player("Leader");
        var member = new Player("Member");
        var leaderGoldBefore = leader.Gold;
        var memberGoldBefore = member.Gold;
        var leaderLevelBefore = leader.Level;

        var party = new Party([leader, member]);
        party.DistributeGold(100);
        party.DistributeExperience(120);

        Assert.Equal(leaderGoldBefore + 100, leader.Gold);
        Assert.Equal(memberGoldBefore, member.Gold);
        Assert.True(leader.Level >= leaderLevelBefore);
        Assert.Equal(1, member.Level);
    }
}
