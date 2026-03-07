using Game.Core.Save;

namespace Game.Core.States;

/// <summary>
/// Combat mode — player and enemy exchange attacks each action tick.
/// </summary>
public class InCombatMode : IGameMode
{
    public GameState ModeId => GameState.InCombat;
    public double ActionInterval => 1.0;
    public double ActionTimer { get; set; }

    /// <summary>Number of combat turns elapsed.</summary>
    public int TurnCount { get; set; }

    /// <summary>Local reference to the current enemy for ToSaveData().</summary>
    private Enemy? _enemy;

    public void Enter(GameSession session, GameEngine engine)
    {
        ActionTimer = 0;
        TurnCount = 0;
        _enemy = session.CurrentEnemy;
    }

    public void ExecuteAction(GameSession session, GameEngine engine)
    {
        if (session.CurrentEnemy == null)
            return;

        TurnCount++;
        _enemy = session.CurrentEnemy;
        var enemy = session.CurrentEnemy;
        var combatants = session.Party.GetCombatants();

        if (combatants.Count == 0)
        {
            session.CurrentStateId = GameState.GameOver;
            session.AddToLog("パーティは全滅した...");
            session.CurrentEnemy = null;
            _enemy = null;
            return;
        }

        // Party attacks
        foreach (var combatant in combatants)
        {
            int damage = Math.Max(1, combatant.Attack - enemy.Defense);
            damage += engine.Rng.Next(-2, 3);
            damage = Math.Max(1, damage);

            enemy.TakeDamage(damage);
            session.AddToLog($"{combatant.Name}の攻撃！ {enemy.Name}に{damage}ダメージ！");

            if (!enemy.IsAlive)
            {
                HandleEnemyDefeated(session, engine);
                return;
            }
        }

        var incomingTarget = session.Party.SelectIncomingTarget(engine.Rng);

        // Enemy counterattack
        int enemyDamage = Math.Max(1, enemy.Attack - incomingTarget.Defense);
        enemyDamage += engine.Rng.Next(-2, 3);
        enemyDamage = Math.Max(1, enemyDamage);

        incomingTarget.TakeDamage(enemyDamage);
        session.AddToLog($"{enemy.Name}の攻撃！ {incomingTarget.Name}に{enemyDamage}ダメージ！");

        if (session.Party.AllDown)
        {
            session.CurrentStateId = GameState.GameOver;
            session.AddToLog("パーティは力尽きた...");
            session.CurrentEnemy = null;
            _enemy = null;
        }
        else if (session.Party.ShouldRetreat)
        {
            session.AddToLog("HP危険！ 戦闘を離脱して撤退を開始する！");
            session.CurrentEnemy = null;
            _enemy = null;
            engine.TransitionTo(new ReturningMode(session.CurrentFloor));
        }
    }

    public void Exit(GameSession session, GameEngine engine)
    {
        _enemy = null;
    }

    public ModeStateData ToSaveData() => new InCombatModeData
    {
        ActionTimer = ActionTimer,
        TurnCount = TurnCount,
        CurrentEnemy = _enemy?.ToSaveData()
    };

    // ── Private ──

    private void HandleEnemyDefeated(GameSession session, GameEngine engine)
    {
        var enemy = session.CurrentEnemy!;

        session.AddToLog($"{enemy.Name}を倒した！");
        session.Party.DistributeExperience(enemy.ExpReward);
        session.Party.DistributeGold(enemy.GoldReward);
        session.AddToLog($"経験値 {enemy.ExpReward} と ゴールド {enemy.GoldReward} を獲得！");

        if (session.RoomsExplored % 5 == 0) // Boss defeated
        {
            session.CurrentFloor++;
            session.RoomsExplored = 0;
            session.AddToLog($"ダンジョン 第{session.CurrentFloor}階に進んだ！");
        }

        session.CurrentEnemy = null;
        _enemy = null;

        // Retreat if HP is critical after victory
        if (session.Party.ShouldRetreat)
        {
            session.AddToLog("HP危険！ 撤退を開始する！");
            engine.TransitionTo(new ReturningMode(session.CurrentFloor));
        }
        else
        {
            engine.TransitionTo(new InDungeonMode());
        }
    }
}
