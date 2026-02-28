using System.Text.Json.Serialization;

namespace Game.Core.Save;

/// <summary>
/// Polymorphic base class for exclusive mode state data.
/// Uses System.Text.Json type discriminator for serialization.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$mode")]
[JsonDerivedType(typeof(InDungeonModeData), "InDungeon")]
[JsonDerivedType(typeof(InCombatModeData), "InCombat")]
[JsonDerivedType(typeof(ReturningModeData), "Returning")]
[JsonDerivedType(typeof(InBaseModeData), "InBase")]
public abstract class ModeStateData
{
    public double ActionTimer { get; set; }
}

public class InDungeonModeData : ModeStateData { }

public class InCombatModeData : ModeStateData
{
    public int TurnCount { get; set; }
    public EnemySaveData? CurrentEnemy { get; set; }
}

public class ReturningModeData : ModeStateData
{
    public double RemainingTime { get; set; }
}

public class InBaseModeData : ModeStateData
{
    public double RecoveryTimer { get; set; }
}

/// <summary>
/// Enemy snapshot for save data.
/// </summary>
public class EnemySaveData
{
    public string Name { get; set; } = "";
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int ExpReward { get; set; }
    public int GoldReward { get; set; }
}
