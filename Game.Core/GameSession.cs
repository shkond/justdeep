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
    public Dictionary<InventoryKind, InventoryContainer> SharedInventories { get; set; } = [];

    public GameSession(Party party)
    {
        Party = party;
        CurrentFloor = 1;
        RoomsExplored = 0;
        CurrentStateId = GameState.MainMenu;

        // Default shared containers
        SharedInventories[InventoryKind.Stash] =
            InventoryContainer.Create(InventoryKind.Stash); // unlimited
    }

    public InventoryContainer GetSharedInventory(InventoryKind kind)
    {
        if (!IsSharedKind(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind,
                "Only shared inventory kinds are allowed.");
        }

        if (!SharedInventories.TryGetValue(kind, out var container))
        {
            throw new KeyNotFoundException($"Shared inventory '{kind}' was not found.");
        }

        return container;
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
                PersonalInventory = ToContainerData(player.PersonalInventory),
            }).ToList(),

            CurrentFloor = CurrentFloor,
            RoomsExplored = RoomsExplored,
            GameLog = new List<string>(GameLog),
        };

        // Serialize shared inventories only
        foreach (var (kind, container) in SharedInventories)
        {
            if (!IsSharedKind(kind))
            {
                throw new InvalidOperationException(
                    $"Non-shared inventory kind '{kind}' cannot be serialized as shared inventory.");
            }

            data.SharedInventories.Add(ToContainerData(container));
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
            var player = new Player(id, p.Name)
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

            // Resilience for broken/incomplete saves:
            // if personal inventory is missing, create a safe empty one.
            if (p.PersonalInventory is null)
            {
                player.PersonalInventory = InventoryContainer.Create(
                    InventoryKind.Personal, player.CarryCapacity);
            }
            else
            {
                if (p.PersonalInventory.Kind != InventoryKind.Personal)
                {
                    throw new InvalidOperationException(
                        $"Invalid personal inventory kind '{p.PersonalInventory.Kind}' for player '{p.Name}'.");
                }

                player.PersonalInventory = FromContainerData(p.PersonalInventory);
            }

            return player;
        }).ToList();

        var session = new GameSession(new Party(members))
        {
            CurrentFloor = data.CurrentFloor,
            RoomsExplored = data.RoomsExplored,
            GameLog = new List<string>(data.GameLog),
        };

        // Restore shared inventories (replace the defaults created by the constructor)
        if (data.SharedInventories.Count > 0)
        {
            session.SharedInventories.Clear();
            foreach (var containerData in data.SharedInventories)
            {
                if (!IsSharedKind(containerData.Kind))
                {
                    throw new InvalidOperationException(
                        $"Invalid shared inventory kind '{containerData.Kind}' in session snapshot.");
                }

                session.SharedInventories[containerData.Kind] = FromContainerData(containerData);
            }
        }

        return session;
    }

    private static bool IsSharedKind(InventoryKind kind)
        => kind is InventoryKind.Stash or InventoryKind.Loot;

    private static InventoryContainerData ToContainerData(InventoryContainer container)
    {
        var containerData = new InventoryContainerData
        {
            Kind = container.Kind,
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

        return containerData;
    }

    private static InventoryContainer FromContainerData(InventoryContainerData containerData)
    {
        var container = InventoryContainer.Create(containerData.Kind, containerData.MaxWeight);
        foreach (var entryData in containerData.Entries)
        {
            container.Entries.Add(InventoryEntry.Of(entryData.DefinitionId, entryData.Quantity));
        }

        return container;
    }
}
