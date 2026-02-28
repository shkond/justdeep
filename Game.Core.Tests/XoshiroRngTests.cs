using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public class XoshiroRngTests
{
    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var rng1 = new XoshiroRng(42);
        var rng2 = new XoshiroRng(42);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rng1.NextUint(), rng2.NextUint());
        }
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentSequences()
    {
        var rng1 = new XoshiroRng(1);
        var rng2 = new XoshiroRng(2);

        // At least one value out of 10 should differ
        bool anyDiffer = false;
        for (int i = 0; i < 10; i++)
        {
            if (rng1.NextUint() != rng2.NextUint())
            {
                anyDiffer = true;
                break;
            }
        }
        Assert.True(anyDiffer);
    }

    [Fact]
    public void GetState_FromState_Roundtrip()
    {
        var rng = new XoshiroRng(12345);

        // Advance a few steps
        for (int i = 0; i < 10; i++)
            rng.NextUint();

        // Snapshot
        uint[] state = rng.GetState();
        Assert.Equal(4, state.Length);

        // Restore
        var restored = XoshiroRng.FromState(state);

        // Both should produce the same sequence from here
        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rng.NextUint(), restored.NextUint());
        }
    }

    [Fact]
    public void Next_Max_ReturnsValuesInRange()
    {
        var rng = new XoshiroRng(99);

        for (int i = 0; i < 1000; i++)
        {
            int value = rng.Next(10);
            Assert.InRange(value, 0, 9);
        }
    }

    [Fact]
    public void Next_MinMax_ReturnsValuesInRange()
    {
        var rng = new XoshiroRng(77);

        for (int i = 0; i < 1000; i++)
        {
            int value = rng.Next(-5, 5);
            Assert.InRange(value, -5, 4); // [min, max)
        }
    }

    [Fact]
    public void Next_Max_ThrowsOnInvalidArg()
    {
        var rng = new XoshiroRng(1);
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(-1));
    }

    [Fact]
    public void Next_MinMax_ThrowsWhenMinEqualsMax()
    {
        var rng = new XoshiroRng(1);
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(5, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(5, 3));
    }

    [Fact]
    public void FromState_ThrowsOnInvalidState()
    {
        Assert.Throws<ArgumentException>(() => XoshiroRng.FromState(null!));
        Assert.Throws<ArgumentException>(() => XoshiroRng.FromState(new uint[] { 1, 2, 3 }));
        Assert.Throws<ArgumentException>(() => XoshiroRng.FromState(new uint[] { 1, 2, 3, 4, 5 }));
    }
}
