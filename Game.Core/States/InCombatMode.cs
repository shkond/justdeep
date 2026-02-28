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
        var player = session.Player;
        var enemy = session.CurrentEnemy;

        // Player attacks
        int damage = Math.Max(1, player.Attack - enemy.Defense);
        damage += engine.Rng.Next(-2, 3);
        damage = Math.Max(1, damage);

        enemy.TakeDamage(damage);
        session.AddToLog($"{player.Name}の攻撃！ {enemy.Name}に{damage}ダメージ！");

        if (!enemy.IsAlive)
        {
            HandleEnemyDefeated(session, engine);
            return;
        }

        // Enemy counterattack
        int enemyDamage = Math.Max(1, enemy.Attack - player.Defense);
        enemyDamage += engine.Rng.Next(-2, 3);
        enemyDamage = Math.Max(1, enemyDamage);

        player.TakeDamage(enemyDamage);
        session.AddToLog($"{enemy.Name}の攻撃！ {player.Name}に{enemyDamage}ダメージ！");

        if (!player.IsAlive)
        {
            session.CurrentStateId = GameState.GameOver;
            session.AddToLog("冒険者は力尽きた...");
            session.CurrentEnemy = null;
            _enemy = null;
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
        session.Player.GainExperience(enemy.ExpReward);
        session.Player.AddGold(enemy.GoldReward);
        session.AddToLog($"経験値 {enemy.ExpReward} と ゴールド {enemy.GoldReward} を獲得！");

        if (session.RoomsExplored % 5 == 0) // Boss defeated
        {
            session.CurrentFloor++;
            session.RoomsExplored = 0;
            session.AddToLog($"ダンジョン 第{session.CurrentFloor}階に進んだ！");
        }

        session.CurrentEnemy = null;
        _enemy = null;
        engine.TransitionTo(new InDungeonMode());
    }
}
