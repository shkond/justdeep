namespace Game.Core;

public class GameManager
{
    private readonly Random _random = new();

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

    public void StartNewGame(string playerName)
    {
        Player = new Player(playerName);
        CurrentFloor = 1;
        RoomsExplored = 0;
        GameLog.Clear();
        CurrentState = GameState.InDungeon;
        AddToLog($"{playerName}の冒険が始まった！");
        AddToLog($"ダンジョン 第{CurrentFloor}階に入った。");
    }

    public void AddToLog(string message)
    {
        GameLog.Add(message);
    }

    public DungeonRoom ExploreRoom()
    {
        if (CurrentState != GameState.InDungeon)
            return DungeonRoom.Empty;

        RoomsExplored++;

        // 5部屋ごとにボス
        if (RoomsExplored % 5 == 0)
        {
            return DungeonRoom.Boss;
        }

        // ランダムな部屋タイプ
        int roll = _random.Next(100);
        if (roll < 50) // 50% 敵
        {
            return DungeonRoom.Enemy;
        }
        else if (roll < 70) // 20% 宝箱
        {
            return DungeonRoom.Treasure;
        }
        else if (roll < 85) // 15% ショップ
        {
            return DungeonRoom.Shop;
        }
        else // 15% 空部屋
        {
            return DungeonRoom.Empty;
        }
    }

    public void EnterCombat(bool isBoss = false)
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

    public CombatResult PlayerAttack()
    {
        if (CurrentEnemy == null || CurrentState != GameState.InCombat)
        {
            return new CombatResult { Message = "戦闘中ではありません。" };
        }

        var result = new CombatResult();
        
        // プレイヤーの攻撃
        int damage = Math.Max(1, Player.Attack - CurrentEnemy.Defense);
        damage += _random.Next(-2, 3); // ランダム性
        damage = Math.Max(1, damage);
        
        CurrentEnemy.TakeDamage(damage);
        result.DamageDealt = damage;
        AddToLog($"{Player.Name}の攻撃！ {CurrentEnemy.Name}に{damage}ダメージ！");

        if (!CurrentEnemy.IsAlive)
        {
            result.PlayerWon = true;
            AddToLog($"{CurrentEnemy.Name}を倒した！");
            
            Player.GainExperience(CurrentEnemy.ExpReward);
            Player.AddGold(CurrentEnemy.GoldReward);
            AddToLog($"経験値 {CurrentEnemy.ExpReward} と ゴールド {CurrentEnemy.GoldReward} を獲得！");
            
            if (RoomsExplored % 5 == 0) // ボス撃破
            {
                CurrentFloor++;
                RoomsExplored = 0;
                AddToLog($"ダンジョン 第{CurrentFloor}階に進んだ！");
            }
            
            CurrentEnemy = null;
            CurrentState = GameState.InDungeon;
            result.Message = "戦闘に勝利した！";
            return result;
        }

        // 敵の反撃
        int enemyDamage = Math.Max(1, CurrentEnemy.Attack - Player.Defense);
        enemyDamage += _random.Next(-2, 3);
        enemyDamage = Math.Max(1, enemyDamage);
        
        Player.TakeDamage(enemyDamage);
        result.DamageTaken = enemyDamage;
        AddToLog($"{CurrentEnemy.Name}の攻撃！ {Player.Name}に{enemyDamage}ダメージ！");

        if (!Player.IsAlive)
        {
            result.PlayerWon = false;
            CurrentState = GameState.GameOver;
            AddToLog("冒険者は力尽きた...");
            result.Message = "Game Over";
            return result;
        }

        result.Message = "戦闘続行中";
        return result;
    }

    public bool RunAway()
    {
        if (CurrentState != GameState.InCombat || CurrentEnemy == null)
            return false;

        int escapeChance = _random.Next(100);
        if (escapeChance < 60) // 60% 成功率
        {
            AddToLog("逃げ出した！");
            CurrentEnemy = null;
            CurrentState = GameState.InDungeon;
            return true;
        }
        else
        {
            AddToLog("逃げられなかった！");
            // 敵の攻撃を受ける
            int enemyDamage = Math.Max(1, CurrentEnemy.Attack - Player.Defense);
            Player.TakeDamage(enemyDamage);
            AddToLog($"{CurrentEnemy.Name}の攻撃！ {Player.Name}に{enemyDamage}ダメージ！");
            
            if (!Player.IsAlive)
            {
                CurrentState = GameState.GameOver;
                AddToLog("冒険者は力尽きた...");
            }
            return false;
        }
    }

    public void OpenTreasure()
    {
        int goldFound = _random.Next(20, 50) + CurrentFloor * 10;
        Player.AddGold(goldFound);
        AddToLog($"宝箱を発見！ ゴールド {goldFound} を獲得！");
    }

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

    public void Rest()
    {
        int healAmount = Player.MaxHp / 4;
        Player.Heal(healAmount);
        AddToLog($"休息した。HP {healAmount} 回復！");
    }
}
