using Game.Core.Save;
using Game.Core.States;

namespace Game.Core;

/// <summary>
/// Game engine — owns progression logic, RNG, and mode transitions.
/// Replaces the old GameManager class.
/// </summary>
public class GameEngine
{
    public GameSession Session { get; private set; }
    public IGameMode CurrentMode { get; private set; }
    public List<IGameComponent> Components { get; } = [];
    public XoshiroRng Rng { get; private set; }
    public ulong RunSeed { get; private set; }

    public GameEngine()
    {
        // Initialized to a stub session in MainMenu state
        Session = new GameSession(new Player("冒険者"));
        Rng = new XoshiroRng(0);
        CurrentMode = new InDungeonMode(); // Placeholder, not entered yet
    }

    // ══════════════════════════════════════════════
    //  Public API
    // ══════════════════════════════════════════════

    /// <summary>
    /// Start a new game run. Optionally specify a seed for reproducibility.
    /// </summary>
    public void StartNewGame(string playerName, ulong? seed = null)
    {
        RunSeed = seed ?? (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Rng = new XoshiroRng(RunSeed);

        Session = new GameSession(new Player(playerName))
        {
            CurrentStateId = GameState.InDungeon
        };

        Session.AddToLog($"{playerName}の冒険が始まった！");
        Session.AddToLog($"ダンジョン 第{Session.CurrentFloor}階に入った。");

        var startMode = new InDungeonMode();
        CurrentMode = startMode;
        startMode.Enter(Session, this);
    }

    /// <summary>
    /// Main loop entry point. Called every frame (~60fps).
    /// Accumulates dt and fires mode actions when interval is reached.
    /// </summary>
    public void Tick(double dt)
    {
        // Don't tick in terminal/menu states
        var stateId = CurrentMode.ModeId;
        if (Session.CurrentStateId == GameState.MainMenu
            || Session.CurrentStateId == GameState.GameOver
            || Session.CurrentStateId == GameState.Victory)
        {
            return;
        }

        // Tick parallel components
        foreach (var component in Components)
        {
            if (component.IsActive)
                component.Tick(dt, Session, this);
        }

        // Tick the current exclusive mode
        CurrentMode.ActionTimer += dt;
        if (CurrentMode.ActionTimer >= CurrentMode.ActionInterval)
        {
            CurrentMode.ActionTimer -= CurrentMode.ActionInterval;
            CurrentMode.ExecuteAction(Session, this);
        }
    }

    /// <summary>
    /// Transition to a new exclusive mode.
    /// </summary>
    public void TransitionTo(IGameMode newMode)
    {
        CurrentMode.Exit(Session, this);
        CurrentMode = newMode;
        Session.CurrentStateId = newMode.ModeId;
        newMode.Enter(Session, this);
    }

    /// <summary>
    /// Use a potion (player action, available in any active state).
    /// </summary>
    public void UsePotion()
    {
        int potionCost = 30;
        if (Session.Player.SpendGold(potionCost))
        {
            int healAmount = 50;
            Session.Player.Heal(healAmount);
            Session.AddToLog($"ポーションを使用！ HP {healAmount} 回復！（-{potionCost} ゴールド）");
        }
        else
        {
            Session.AddToLog("ゴールドが足りません！");
        }
    }

    // ══════════════════════════════════════════════
    //  Save / Load (DTO conversion only — no I/O)
    // ══════════════════════════════════════════════

    /// <summary>
    /// Create a full save snapshot.
    /// </summary>
    public GameSaveData CreateSaveData()
    {
        return new GameSaveData
        {
            SaveFormatVersion = 1,
            RunSeed = RunSeed,
            RngState = Rng.GetState(),
            SessionState = Session.ToSnapshot(),
            ModeState = CurrentMode.ToSaveData(),
            ComponentStates = Components
                .Select(c => c.ToSaveData())
                .ToList(),
            AutomationState = new AutomationStateData()  // Stub
        };
    }

    /// <summary>
    /// Restore engine state from save data.
    /// </summary>
    public void LoadSaveData(GameSaveData data)
    {
        RunSeed = data.RunSeed;
        Rng = XoshiroRng.FromState(data.RngState);
        Session = GameSession.FromSnapshot(data.SessionState);

        // Restore mode from polymorphic DTO
        CurrentMode = data.ModeState switch
        {
            InCombatModeData combat => RestoreCombatMode(combat),
            ReturningModeData returning => RestoreReturningMode(returning),
            InBaseModeData inBase => RestoreInBaseMode(inBase),
            InDungeonModeData dungeon => RestoreInDungeonMode(dungeon),
            _ => RestoreInDungeonMode(new InDungeonModeData())
        };

        Session.CurrentStateId = CurrentMode.ModeId;
    }

    // ── Restore helpers ──

    private static InDungeonMode RestoreInDungeonMode(InDungeonModeData data)
    {
        return new InDungeonMode { ActionTimer = data.ActionTimer };
    }

    private InCombatMode RestoreCombatMode(InCombatModeData data)
    {
        var mode = new InCombatMode
        {
            ActionTimer = data.ActionTimer,
            TurnCount = data.TurnCount
        };

        if (data.CurrentEnemy != null)
        {
            Session.CurrentEnemy = Enemy.FromSaveData(data.CurrentEnemy);
        }

        return mode;
    }

    private static ReturningMode RestoreReturningMode(ReturningModeData data)
    {
        return new ReturningMode(data.RemainingTime)
        {
            ActionTimer = data.ActionTimer
        };
    }

    private static InBaseMode RestoreInBaseMode(InBaseModeData data)
    {
        return new InBaseMode
        {
            ActionTimer = data.ActionTimer,
            RecoveryTimer = data.RecoveryTimer
        };
    }
}
