namespace Game.Core;

public class GameManager
{
    private readonly Random _random = new();

    // ── Tick timing ──
    private double _actionTimer;
    private double _actionInterval = 1.0; // seconds per action

    // ── Public state (read-only) ──
    public Player Player { get; private set; }
    public Enemy? CurrentEnemy { get; private set; }
    public GameState CurrentState { get; private set; }
    public int CurrentFloor { get; private set; }
    public int RoomsExplored { get; private set; }
    public List<string> GameLog { get; private set; } = new();

    public GameManager()
    {
        Player = new Player("冒険者");
        CurrentState = GameState.MainMenu;
        CurrentFloor = 1;
    }

    // ══════════════════════════════════════════════
    //  Public API
    // ══════════════════════════════════════════════

    /// <summary>
    /// Initialize a new game and transition to InDungeon.
    /// The game will then advance only through Tick().
    /// </summary>
    public void StartNewGame(string playerName)
    {
        Player = new Player(playerName);
        CurrentFloor = 1;
        RoomsExplored = 0;
        _actionTimer = 0;
        GameLog.Clear();
        CurrentState = GameState.InDungeon;
        AddToLog($"{playerName}の冒険が始まった！");
        AddToLog($"ダンジョン 第{CurrentFloor}階に入った。");
    }

    /// <summary>
    /// Main loop entry point. Called every frame (60fps).
    /// Accumulates dt and executes one action when enough time has passed.
    /// </summary>
    public void Tick(double dt)
    {
        // Only tick in active play states
        if (CurrentState != GameState.InDungeon
            && CurrentState != GameState.InCombat
            && CurrentState != GameState.Returning)
        {
            return;
        }

        _actionTimer += dt;

        if (_actionTimer >= _actionInterval)
        {
            _actionTimer -= _actionInterval;
            ExecuteAction();
        }
    }

    /// <summary>
    /// Potion can be used at any time by the player.
    /// </summary>
    public void UsePotion()
    {
        int potionCost = 30;
        if (Player.SpendGold(potionCost))
        {
            int healAmount = 50;
            Player.Heal(healAmount);
            AddToLog($"ポーションを使用！ HP {healAmount} 回復！（-{potionCost} ゴールド）");
        }
        else
        {
            AddToLog("ゴールドが足りません！");
        }
    }

    public void AddToLog(string message)
    {
        GameLog.Add(message);
    }

    // ══════════════════════════════════════════════
    //  Private — action dispatch
    // ══════════════════════════════════════════════

    private void ExecuteAction()
    {
        switch (CurrentState)
        {
            case GameState.InDungeon:
                var room = ExploreRoom();
                HandleExploreResult(room);
                break;

            case GameState.InCombat:
                PlayerAttack();
                break;

            case GameState.Returning:
                // Phase 0 placeholder: immediate return to base
                AddToLog("帰還中… （未実装：即座に帰還）");
                CurrentState = GameState.InBase;
                break;
        }
    }

    // ══════════════════════════════════════════════
    //  Private — exploration
    // ══════════════════════════════════════════════

    private DungeonRoom ExploreRoom()
    {
        RoomsExplored++;

        // Boss every 5 rooms
        if (RoomsExplored % 5 == 0)
        {
            return DungeonRoom.Boss;
        }

        int roll = _random.Next(100);
        if (roll < 50)
            return DungeonRoom.Enemy;
        else if (roll < 70)
            return DungeonRoom.Treasure;
        else if (roll < 85)
            return DungeonRoom.Shop;
        else
            return DungeonRoom.Empty;
    }

    private void HandleExploreResult(DungeonRoom room)
    {
        switch (room)
        {
            case DungeonRoom.Enemy:
                EnterCombat(false);
                break;
            case DungeonRoom.Boss:
                EnterCombat(true);
                break;
            case DungeonRoom.Treasure:
                OpenTreasure();
                break;
            case DungeonRoom.Shop:
                AddToLog("ショップを見つけた！");
                break;
            case DungeonRoom.Empty:
                AddToLog("空の部屋だ。");
                break;
        }
    }

    // ══════════════════════════════════════════════
    //  Private — combat
    // ══════════════════════════════════════════════

    private void EnterCombat(bool isBoss)
    {
        CurrentState = GameState.InCombat;

        if (isBoss)
        {
            CurrentEnemy = Enemy.CreateDragon(CurrentFloor);
            AddToLog($"ボス【{CurrentEnemy.Name}】が現れた！");
        }
        else
        {
            int enemyType = _random.Next(3);
            CurrentEnemy = enemyType switch
            {
                0 => Enemy.CreateSlime(CurrentFloor),
                1 => Enemy.CreateGoblin(CurrentFloor),
                _ => Enemy.CreateOrc(CurrentFloor)
            };
            AddToLog($"【{CurrentEnemy.Name}】が現れた！");
        }
    }

    private void PlayerAttack()
    {
        if (CurrentEnemy == null || CurrentState != GameState.InCombat)
            return;

        // Player attacks
        int damage = Math.Max(1, Player.Attack - CurrentEnemy.Defense);
        damage += _random.Next(-2, 3);
        damage = Math.Max(1, damage);

        CurrentEnemy.TakeDamage(damage);
        AddToLog($"{Player.Name}の攻撃！ {CurrentEnemy.Name}に{damage}ダメージ！");

        if (!CurrentEnemy.IsAlive)
        {
            AddToLog($"{CurrentEnemy.Name}を倒した！");

            Player.GainExperience(CurrentEnemy.ExpReward);
            Player.AddGold(CurrentEnemy.GoldReward);
            AddToLog($"経験値 {CurrentEnemy.ExpReward} と ゴールド {CurrentEnemy.GoldReward} を獲得！");

            if (RoomsExplored % 5 == 0) // Boss defeated
            {
                CurrentFloor++;
                RoomsExplored = 0;
                AddToLog($"ダンジョン 第{CurrentFloor}階に進んだ！");
            }

            CurrentEnemy = null;
            CurrentState = GameState.InDungeon;
            return;
        }

        // Enemy counterattack
        int enemyDamage = Math.Max(1, CurrentEnemy.Attack - Player.Defense);
        enemyDamage += _random.Next(-2, 3);
        enemyDamage = Math.Max(1, enemyDamage);

        Player.TakeDamage(enemyDamage);
        AddToLog($"{CurrentEnemy.Name}の攻撃！ {Player.Name}に{enemyDamage}ダメージ！");

        if (!Player.IsAlive)
        {
            CurrentState = GameState.GameOver;
            AddToLog("冒険者は力尽きた...");
        }
    }

    // ══════════════════════════════════════════════
    //  Private — misc actions
    // ══════════════════════════════════════════════

    private void OpenTreasure()
    {
        int goldFound = _random.Next(20, 50) + CurrentFloor * 10;
        Player.AddGold(goldFound);
        AddToLog($"宝箱を発見！ ゴールド {goldFound} を獲得！");
    }
}
