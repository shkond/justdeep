namespace Game.Core.Save;

public class PlayerData
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Level { get; set; }
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Experience { get; set; }
    public int Gold { get; set; }
    public double CarryCapacity { get; set; }
}
