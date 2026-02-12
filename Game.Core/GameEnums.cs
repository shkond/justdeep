namespace Game.Core;

public enum GameState
{
    MainMenu,
    InDungeon,
    InCombat,
    Shopping,
    GameOver,
    Victory,
    Returning,
    InBase
}

public enum DungeonRoom
{
    Empty,
    Enemy,
    Treasure,
    Shop,
    Boss
}

public class CombatResult
{
    public bool PlayerWon { get; set; }
    public int DamageDealt { get; set; }
    public int DamageTaken { get; set; }
    public string Message { get; set; } = string.Empty;
}
