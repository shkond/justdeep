using Game.Core.Items;
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

    public int CurrentFloor { get; set; }
    public int RoomsExplored { get; set; }
    public Enemy? CurrentEnemy { get; set; }
    public List<string> GameLog { get; set; } = [];
    public GameState CurrentStateId { get; set; }

    // ── Inventory containers ──
    /// <summary>
    /// All inventory containers keyed by kind.
    /// Populated at construction with Player and Stash containers.
    /// Loot containers are added/removed as needed during gameplay.
    /// </summary>
    public Dictionary<InventoryKind, InventoryContainer> Inventories { get; set; } = [];

    public GameSession(Player player)
    {
        Player = player;
        CurrentFloor = 1;
        RoomsExplored = 0;
        CurrentStateId = GameState.MainMenu;

        // Default containers
        Inventories[InventoryKind.Player] =
            InventoryContainer.Create(InventoryKind.Player, player.CarryCapacity);
        Inventories[InventoryKind.Stash] =
            InventoryContainer.Create(InventoryKind.Stash); // unlimited
    }

    public void AddToLog(string message)
    {
        GameLog.Add(message);
    }

    /// <summary>Convert to a flat DTO for serialization.</summary>
    public SessionStateData ToSnapshot()
    {
        var data = new SessionStateData
        {
            PlayerName = Player.Name,
            Level = Player.Level,
            MaxHp = Player.MaxHp,
            CurrentHp = Player.CurrentHp,
            Attack = Player.Attack,
            Defense = Player.Defense,
            Experience = Player.Experience,
            Gold = Player.Gold,
            CarryCapacity = Player.CarryCapacity,

            CurrentFloor = CurrentFloor,
            RoomsExplored = RoomsExplored,
            GameLog = new List<string>(GameLog),
        };

        // Serialize inventories
        foreach (var (kind, container) in Inventories)
        {
            var containerData = new InventoryContainerData
            {
                Kind = kind,
                MaxWeight = container.Rules.MaxWeight,
            };
            foreach (var entry in container.Entries)
            {
                containerData.Entries.Add(new InventoryEntryData
                {
                    DefinitionId = entry.DefinitionId,
                    Quantity = entry.Quantity,
                });
            }
            data.Inventories.Add(containerData);
        }

        return data;
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
            Gold = data.Gold,
            CarryCapacity = data.CarryCapacity,
        };

        var session = new GameSession(player)
        {
            CurrentFloor = data.CurrentFloor,
            RoomsExplored = data.RoomsExplored,
            GameLog = new List<string>(data.GameLog),
        };

        // Restore inventories (replace the defaults created by the constructor)
        if (data.Inventories.Count > 0)
        {
            session.Inventories.Clear();
            foreach (var containerData in data.Inventories)
            {
                var container = InventoryContainer.Create(
                    containerData.Kind, containerData.MaxWeight);

                foreach (var entryData in containerData.Entries)
                {
                    container.Entries.Add(
                        InventoryEntry.Of(entryData.DefinitionId, entryData.Quantity));
                }

                session.Inventories[containerData.Kind] = container;
            }
        }

        return session;
    }
}
