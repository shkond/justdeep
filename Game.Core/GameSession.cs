using Game.Core.Save;

namespace Game.Core;

/// <summary>
/// Pure domain data aggregate — the "save target."
/// Contains no logic beyond snapshot/restore and log helper.
/// Does NOT hold RNG or seed (those belong to GameEngine).
/// </summary>
public class GameSession
{
    public Player Player { get; set; }
    public List<string> Inventory { get; set; } = [];   // Stub for future
    public int CurrentFloor { get; set; }
    public int RoomsExplored { get; set; }
    public Enemy? CurrentEnemy { get; set; }
    public List<string> GameLog { get; set; } = [];
    public GameState CurrentStateId { get; set; }

    public GameSession(Player player)
    {
        Player = player;
        CurrentFloor = 1;
        RoomsExplored = 0;
        CurrentStateId = GameState.MainMenu;
    }

    public void AddToLog(string message)
    {
        GameLog.Add(message);
    }

    /// <summary>Convert to a flat DTO for serialization.</summary>
    public SessionStateData ToSnapshot()
    {
        return new SessionStateData
        {
            PlayerName = Player.Name,
            Level = Player.Level,
            MaxHp = Player.MaxHp,
            CurrentHp = Player.CurrentHp,
            Attack = Player.Attack,
            Defense = Player.Defense,
            Experience = Player.Experience,
            Gold = Player.Gold,
            Inventory = new List<string>(Inventory),
            CurrentFloor = CurrentFloor,
            RoomsExplored = RoomsExplored,
            GameLog = new List<string>(GameLog)
        };
    }

    /// <summary>Restore a GameSession from a DTO snapshot.</summary>
    public static GameSession FromSnapshot(SessionStateData data)
    {
        var player = new Player(data.PlayerName)
        {
            Level = data.Level,
            MaxHp = data.MaxHp,
            CurrentHp = data.CurrentHp,
            Attack = data.Attack,
            Defense = data.Defense,
            Experience = data.Experience,
            Gold = data.Gold
        };

        return new GameSession(player)
        {
            Inventory = new List<string>(data.Inventory),
            CurrentFloor = data.CurrentFloor,
            RoomsExplored = data.RoomsExplored,
            GameLog = new List<string>(data.GameLog)
        };
    }
}
