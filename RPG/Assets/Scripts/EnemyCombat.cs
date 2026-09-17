using UnityEngine;
using System;

public class EnemyCombat : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("Enemy configuration asset. Required.")]
    [SerializeField] private EnemyData enemyData;

    [Header("Multipliers")]
    [Tooltip("Multiplies max HP from EnemyData.")]
    [Min(0.1f)] [SerializeField] private float healthMultiplier = 1f;
    [Tooltip("Multiplies attack damage from EnemyData.")]
    [Min(0.1f)] [SerializeField] private float damageMultiplier = 1f;

    [Header("Target")]
    [Tooltip("Optional. Assign player transform manually. If empty, searches by Player tag.")]
    [SerializeField] private Transform target;

    [Header("Health Bar")]
    [SerializeField] private bool    showHealthBar      = true;
    [SerializeField] private Vector2 healthBarOffset    = new Vector2(0f, 0.8f);
    [SerializeField] private Vector2 healthBarSize      = new Vector2(0.7f, 0.08f);
    [SerializeField] private int     healthBarSortingOrder = 20;

    // Runtime stats (written from enemyData in Awake)
    private int   maxHealth;
    private int   attackDamage;
    private float attackCooldown;
    private float attackDistance;
    private float visionDistance;
    private float loseTargetDelay;
    private float moveSpeed;
    private float returnSpeed;
    private float wanderRadius;
    private float wanderPointReachDistance;
    private float wanderPauseDuration;
    private float knockbackResistance;
    private float knockbackDuration;
    private float knockbackDecay;

    private int   currentHealth;
    private bool  isDead;
    private float lastAttackTime = -999f;
    private Vector2 lastFacingDirection = Vector2.down;
    private PlayerHealth cachedPlayerHealth;
    private float attackStateTimer;
    private Rigidbody2D cachedRigidbody;
    private Vector2 movementVelocity;

    private Vector2 spawnPosition;
    private Vector2 wanderTarget;
    private float   nextWanderPickTime;
    private float   lostTargetTimer;
    private bool    isReturningToSpawn;

    private Vector2 knockbackVelocity;
    private float   knockbackTimer;

    private Transform healthBarRoot;
    private Transform healthFillTransform;
    private static Sprite whiteSprite;

    public event Action         AttackPerformed;
    public event Action         Died;
    public event Action<int,int> HealthChanged;

    public static event Action BossKilled;

    public int     CurrentHealth    => currentHealth;
    public int     AttackDamage     => attackDamage;
    public float   AttackCooldown   => attackCooldown;
    public float   AttackDistance   => attackDistance;
    public bool    IsInAttackState  => attackStateTimer > 0f;
    public float   CurrentMoveSpeed => movementVelocity.magnitude;
    public bool    IsMoving         => movementVelocity.sqrMagnitude > 0.0001f;
    public Vector2 FacingDirection  => lastFacingDirection;

    private void Awake()
    {
        if (enemyData == null)
        {
            Debug.LogError($"EnemyCombat on '{gameObject.name}' has no EnemyData assigned.", this);
            enabled = false;
            return;
        }

        maxHealth              = Mathf.Max(1, Mathf.RoundToInt(enemyData.maxHealth  * healthMultiplier));
        attackDamage           = Mathf.Max(1, Mathf.RoundToInt(enemyData.attackDamage * damageMultiplier));
        attackCooldown         = enemyData.attackCooldown;
        attackDistance         = enemyData.attackDistance;
        visionDistance         = enemyData.visionDistance;
        loseTargetDelay        = enemyData.loseTargetDelay;
        moveSpeed              = enemyData.moveSpeed;
        returnSpeed            = enemyData.returnSpeed;
        wanderRadius           = enemyData.wanderRadius;
        wanderPointReachDistance = enemyData.wanderPointReachDistance;
        wanderPauseDuration    = enemyData.wanderPauseDuration;
        knockbackResistance    = enemyData.knockbackResistance;
        knockbackDuration      = enemyData.knockbackDuration;
        knockbackDecay         = enemyData.knockbackDecay;

        currentHealth  = maxHealth;
        spawnPosition  = transform.position;

        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                target = playerObject.transform;
        }

        if (target != null)
            cachedPlayerHealth = target.GetComponent<PlayerHealth>();

        cachedRigidbody = GetComponent<Rigidbody2D>();
        if (cachedRigidbody != null)
            cachedRigidbody.gravityScale = 0f;

        if (showHealthBar)
            CreateHealthBar();

        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (isDead) return;

        if (attackStateTimer > 0f)
            attackStateTimer -= Time.deltaTime;

        UpdateTargetReferenceIfMissing();
        UpdateBehavior();
        UpdateHealthBar();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (knockbackTimer > 0f)
        {
            ApplyMotion(knockbackVelocity);
            knockbackVelocity = Vector2.MoveTowards(knockbackVelocity, Vector2.zero, knockbackDecay * Time.fixedDeltaTime);
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        ApplyMotion(movementVelocity);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || isDead) return;

        currentHealth -= damage;
        HealthChanged?.Invoke(Mathf.Max(currentHealth, 0), maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void ApplyKnockback(Vector2 sourcePosition, float force)
    {
        if (isDead) return;

        Vector2 dir = ((Vector2)transform.position - sourcePosition).normalized;
        if (dir.sqrMagnitude <= 0f) dir = Vector2.up;

        knockbackVelocity = dir * (force * (1f - Mathf.Clamp01(knockbackResistance)));
        knockbackTimer    = Mathf.Max(knockbackDuration, 0.02f);
    }

    public bool CanAttack() => Time.time >= lastAttackTime + attackCooldown;

    public void TryAttack()
    {
        if (!CanAttack() || cachedPlayerHealth == null) return;

        lastAttackTime = Time.time;

        Vector2 knockbackDir = (target.position - transform.position).normalized * 1.5f;
        cachedPlayerHealth.TakeDamage(attackDamage, knockbackDir);
        attackStateTimer = Mathf.Min(attackCooldown, 0.35f);
        AttackPerformed?.Invoke();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (healthBarRoot != null)
            Destroy(healthBarRoot.gameObject);

        Died?.Invoke();

        if (enemyData != null && enemyData.isBoss)
            BossKilled?.Invoke();

        Destroy(gameObject);
    }

    private void UpdateBehavior()
    {
        if (target == null) { UpdateWander(); return; }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        bool  seesPlayer       = distanceToPlayer <= visionDistance;

        if (seesPlayer)
        {
            lostTargetTimer    = 0f;
            isReturningToSpawn = false;

            if (distanceToPlayer <= attackDistance)
            {
                movementVelocity    = Vector2.zero;
                lastFacingDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
                TryAttack();
                return;
            }

            Vector2 chaseDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            lastFacingDirection = chaseDir;
            movementVelocity = chaseDir * moveSpeed;
            return;
        }

        lostTargetTimer += Time.deltaTime;
        if (lostTargetTimer >= loseTargetDelay)
            isReturningToSpawn = true;

        if (isReturningToSpawn)
        {
            Vector2 toSpawn = spawnPosition - (Vector2)transform.position;
            if (toSpawn.magnitude <= wanderPointReachDistance)
            {
                isReturningToSpawn  = false;
                nextWanderPickTime  = Time.time;
                movementVelocity    = Vector2.zero;
                return;
            }
            lastFacingDirection = toSpawn.normalized;
            movementVelocity = toSpawn.normalized * returnSpeed;
            return;
        }

        UpdateWander();
    }

    private void UpdateWander()
    {
        if (Time.time >= nextWanderPickTime)
        {
            wanderTarget       = spawnPosition + UnityEngine.Random.insideUnitCircle * wanderRadius;
            nextWanderPickTime = Time.time + wanderPauseDuration;
        }

        Vector2 toWander = wanderTarget - (Vector2)transform.position;
        if (toWander.magnitude <= wanderPointReachDistance)
        {
            movementVelocity = Vector2.zero;
            return;
        }

        lastFacingDirection = toWander.normalized;
        movementVelocity = toWander.normalized * moveSpeed;
    }

    private void ApplyMotion(Vector2 velocity)
    {
        Vector2 step = velocity * Time.fixedDeltaTime;
        if (cachedRigidbody != null)
        {
            cachedRigidbody.MovePosition(cachedRigidbody.position + step);
            return;
        }
        transform.position += (Vector3)step;
    }

    private void UpdateTargetReferenceIfMissing()
    {
        if (target != null && cachedPlayerHealth != null) return;

        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                target = playerObject.transform;
        }

        if (target != null && cachedPlayerHealth == null)
            cachedPlayerHealth = target.GetComponent<PlayerHealth>();
    }

    private void CreateHealthBar()
    {
        if (whiteSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        GameObject root = new GameObject("EnemyHealthBar");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = (Vector3)healthBarOffset;
        healthBarRoot = root.transform;

        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(healthBarRoot, false);
        SpriteRenderer bgR = bg.AddComponent<SpriteRenderer>();
        bgR.sprite       = whiteSprite;
        bgR.color        = new Color(0f, 0f, 0f, 0.8f);
        bgR.sortingOrder = healthBarSortingOrder;
        bg.transform.localScale = new Vector3(healthBarSize.x, healthBarSize.y, 1f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(healthBarRoot, false);
        SpriteRenderer fillR = fill.AddComponent<SpriteRenderer>();
        fillR.sprite       = whiteSprite;
        fillR.color        = new Color(0.9f, 0.15f, 0.15f, 1f);
        fillR.sortingOrder = healthBarSortingOrder + 1;
        fill.transform.localScale = new Vector3(healthBarSize.x, healthBarSize.y * 0.75f, 1f);

        healthFillTransform = fill.transform;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (!showHealthBar || healthFillTransform == null) return;

        float ratio = maxHealth <= 0 ? 0f : Mathf.Clamp01((float)currentHealth / maxHealth);
        float width = healthBarSize.x * ratio;
        healthFillTransform.localScale    = new Vector3(width, healthBarSize.y * 0.75f, 1f);
        healthFillTransform.localPosition = new Vector3(-(healthBarSize.x - width) * 0.5f, 0f, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)spawnPosition : transform.position, wanderRadius);
    }
}
