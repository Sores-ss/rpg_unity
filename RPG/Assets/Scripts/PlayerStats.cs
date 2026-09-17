using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats (level 1)")]
    [Min(1)]  [SerializeField] private int   baseMaxHp    = 100;
    [Min(1)]  [SerializeField] private int   baseAttack   = 10;
    [Min(0f)] [SerializeField] private float baseSpeed    = 5f;

    [Header("Growth per level")]
    [Min(0)]  [SerializeField] private int   hpPerLevel     = 20;
    [Min(0)]  [SerializeField] private int   attackPerLevel = 5;
    [Min(0f)] [SerializeField] private float speedPerLevel  = 0.2f;

    [Header("Level-up gold cost  —  cost(n) = base * multiplier^(n-1)")]
    [Min(1)]    [SerializeField] private int   baseGoldCost       = 100;
    [Min(1.01f)][SerializeField] private float goldCostMultiplier = 1.5f;

    public static PlayerStats Instance { get; private set; }

    public int   Level { get; private set; } = 1;
    public int   Gold  { get; private set; } = 0;

    // Current level stats
    public int   MaxHp   => baseMaxHp  + (Level - 1) * hpPerLevel;
    public int   Attack  => baseAttack + (Level - 1) * attackPerLevel;
    public float Speed   => baseSpeed  + (Level - 1) * speedPerLevel;

    // Stats after the next level up (used for UI preview)
    public int   NextMaxHp   => baseMaxHp  + Level * hpPerLevel;
    public int   NextAttack  => baseAttack + Level * attackPerLevel;
    public float NextSpeed   => baseSpeed  + Level * speedPerLevel;

    public int GoldForNextLevel =>
        Mathf.RoundToInt(baseGoldCost * Mathf.Pow(goldCostMultiplier, Level - 1));

    public event Action<int> LevelChanged;
    public event Action<int> GoldChanged;

    private PlayerHealth playerHealth;
    private PlayerAttack playerAttack;
    private Move_Player  movePlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        playerHealth = GetComponent<PlayerHealth>();
        playerAttack = GetComponent<PlayerAttack>();
        movePlayer   = GetComponent<Move_Player>();

        ApplyStats(healToFull: true);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        Gold += amount;
        GoldChanged?.Invoke(Gold);
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0 || Gold < amount)
            return false;

        Gold -= amount;
        GoldChanged?.Invoke(Gold);
        return true;
    }

    public bool TryLevelUp()
    {
        int cost = GoldForNextLevel;
        if (Gold < cost)
            return false;

        Gold -= cost;
        Level++;
        ApplyStats(healToFull: false);

        LevelChanged?.Invoke(Level);
        GoldChanged?.Invoke(Gold);
        return true;
    }

    private void ApplyStats(bool healToFull)
    {
        if (playerHealth != null)
            playerHealth.SetMaxHealth(MaxHp, healToFull);

        if (playerAttack != null)
            playerAttack.SetAttackDamage(Attack);

        if (movePlayer != null)
            movePlayer.SetMoveSpeed(Speed);
    }
}
