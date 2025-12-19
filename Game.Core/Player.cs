namespace Game.Core;

public class Player
{
    public string Name { get; private set; }
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public int Gold { get; set; }

    public Player(string name)
    {
        Name = name;
        Level = 1;
        MaxHp = 100;
        CurrentHp = MaxHp;
        Attack = 10;
        Defense = 5;
        Experience = 0;
        Gold = 50;
    }

    public bool IsAlive => CurrentHp > 0;

    public void TakeDamage(int damage)
    {
        int actualDamage = Math.Max(1, damage - Defense);
        CurrentHp = Math.Max(0, CurrentHp - actualDamage);
    }

    public void Heal(int amount)
    {
        CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
    }

    public void GainExperience(int exp)
    {
        Experience += exp;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        int expNeeded = Level * 100;
        while (Experience >= expNeeded)
        {
            Experience -= expNeeded;
            Level++;
            MaxHp += 20;
            CurrentHp = MaxHp;
            Attack += 3;
            Defense += 2;
            expNeeded = Level * 100;
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;
    }

    public bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            return true;
        }
        return false;
    }
}
