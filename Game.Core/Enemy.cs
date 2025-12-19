namespace Game.Core;

public class Enemy
{
    public string Name { get; set; }
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int ExpReward { get; set; }
    public int GoldReward { get; set; }

    public Enemy(string name, int hp, int attack, int defense, int expReward, int goldReward)
    {
        Name = name;
        MaxHp = hp;
        CurrentHp = hp;
        Attack = attack;
        Defense = defense;
        ExpReward = expReward;
        GoldReward = goldReward;
    }

    public bool IsAlive => CurrentHp > 0;

    public void TakeDamage(int damage)
    {
        int actualDamage = Math.Max(1, damage - Defense);
        CurrentHp = Math.Max(0, CurrentHp - actualDamage);
    }

    public static Enemy CreateSlime(int floor)
    {
        return new Enemy(
            "スライム",
            30 + floor * 10,
            5 + floor * 2,
            2 + floor,
            10 + floor * 5,
            10 + floor * 5
        );
    }

    public static Enemy CreateGoblin(int floor)
    {
        return new Enemy(
            "ゴブリン",
            50 + floor * 15,
            8 + floor * 3,
            3 + floor,
            20 + floor * 8,
            15 + floor * 7
        );
    }

    public static Enemy CreateOrc(int floor)
    {
        return new Enemy(
            "オーク",
            80 + floor * 20,
            12 + floor * 4,
            5 + floor * 2,
            35 + floor * 12,
            25 + floor * 10
        );
    }

    public static Enemy CreateDragon(int floor)
    {
        return new Enemy(
            "ドラゴン",
            150 + floor * 30,
            20 + floor * 5,
            8 + floor * 2,
            100 + floor * 20,
            50 + floor * 15
        );
    }
}
