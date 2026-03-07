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
    public Party Party { get; set; }

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

    public GameSession(Party party)
    {
        Party = party;
        CurrentFloor = 1;
        RoomsExplored = 0;
        CurrentStateId = GameState.MainMenu;

        // Default containers
        var leader = Party.Members[0];
        Inventories[InventoryKind.Player] =
            InventoryContainer.Create(InventoryKind.Player, leader.CarryCapacity);
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
            Players = Party.Members.Select(player => new PlayerData
            {
                Id = player.Id.ToString(),
                Name = player.Name,
                Level = player.Level,
                MaxHp = player.MaxHp,
                CurrentHp = player.CurrentHp,
                Attack = player.Attack,
                Defense = player.Defense,
                Experience = player.Experience,
                Gold = player.Gold,
                CarryCapacity = player.CarryCapacity,
            }).ToList(),

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
        if (data.Players.Count == 0)
        {
            throw new InvalidOperationException("Invalid session snapshot: no players found.");
        }

        var members = data.Players.Select(p =>
        {
            var id = Guid.TryParse(p.Id, out var parsedId) ? parsedId : Guid.NewGuid();
            return new Player(id, p.Name)
            {
                Level = p.Level,
                MaxHp = p.MaxHp,
                CurrentHp = p.CurrentHp,
                Attack = p.Attack,
                Defense = p.Defense,
                Experience = p.Experience,
                Gold = p.Gold,
                CarryCapacity = p.CarryCapacity,
            };
        }).ToList();

        var session = new GameSession(new Party(members))
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
