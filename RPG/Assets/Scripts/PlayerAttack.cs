using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [Tooltip("Damage dealt to each enemy hit.")]
    [Min(1)]
    [SerializeField] private int attackDamage = 10;
    [Tooltip("Radius of melee hit area.")]
    [Min(0.1f)]
    [SerializeField] private float attackRange = 1.2f;
    [Tooltip("Time between two player attacks.")]
    [Min(0.05f)]
    [SerializeField] private float attackCooldown = 0.5f;
    [Tooltip("Distance in front of player where attack area is centered.")]
    [SerializeField] private float attackForwardOffset = 0.5f;
    [Tooltip("Force sent to enemy knockback logic.")]
    [Min(0f)]
    [SerializeField] private float knockbackForce = 3f;
    [Tooltip("Only colliders on these layers can be hit.")]
    [SerializeField] private LayerMask enemyLayers;
    [Tooltip("Layers containing destructible objects (trees, stumps…). Only the axe can hit these.")]
    [SerializeField] private LayerMask destructibleLayers;

    [Header("Input")]
    [Tooltip("Allow Space key to trigger attack.")]
    [SerializeField] private bool useSpace = true;
    [Tooltip("Allow J key to trigger attack.")]
    [SerializeField] private bool useJ = true;

    private float nextAttackTime;
    private Move_Player movePlayer;

    private void Awake()
    {
        movePlayer = GetComponent<Move_Player>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        bool attackPressed = (useSpace && Keyboard.current.spaceKey.wasPressedThisFrame)
            || (useJ && Keyboard.current.jKey.wasPressedThisFrame);

        if (!attackPressed)
            return;

        TryAttack();
    }

    public void SetAttackDamage(int damage)
    {
        attackDamage = Mathf.Max(1, damage);
    }

    private void TryAttack()
    {
        ItemAnimationType held = InventoryUI.SelectedAnimationType;

        bool canHitEnemies     = held == ItemAnimationType.Axe
                              || held == ItemAnimationType.Knife
                              || held == ItemAnimationType.Hammer;

        bool canHitDestructible = held == ItemAnimationType.Axe
                               || held == ItemAnimationType.Pickaxe
                               || held == ItemAnimationType.Hammer;

        if (!canHitEnemies && !canHitDestructible)
            return;

        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;

        Vector2 facingDirection = GetFacingDirection();
        Vector2 attackCenter    = (Vector2)transform.position + facingDirection * attackForwardOffset;

        if (canHitEnemies)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, attackRange, enemyLayers);
            for (int i = 0; i < hits.Length; i++)
            {
                EnemyCombat enemyCombat = hits[i].GetComponentInParent<EnemyCombat>();
                if (enemyCombat != null)
                {
                    enemyCombat.TakeDamage(attackDamage);
                    enemyCombat.ApplyKnockback(transform.position, knockbackForce);
                }
            }
        }

        if (canHitDestructible && destructibleLayers != 0)
        {
            Collider2D[] destructHits = Physics2D.OverlapCircleAll(attackCenter, attackRange, destructibleLayers);
            for (int i = 0; i < destructHits.Length; i++)
            {
                DestructibleObject obj = destructHits[i].GetComponentInParent<DestructibleObject>();
                if (obj != null && obj.RequiredTool == held)
                    obj.TakeHit();
            }
        }
    }

    private Vector2 GetFacingDirection()
    {
        if (movePlayer != null && movePlayer.FacingDirection.sqrMagnitude > 0f)
            return movePlayer.FacingDirection.normalized;

        return Vector2.down;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 facingDirection = Application.isPlaying && movePlayer != null
            ? movePlayer.FacingDirection.normalized
            : Vector2.down;

        Vector2 attackCenter = (Vector2)transform.position + facingDirection * attackForwardOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackCenter, attackRange);
    }
}
