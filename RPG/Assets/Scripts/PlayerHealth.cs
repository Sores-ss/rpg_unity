using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Maximum HP of the player.")]
    [Min(1)]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private bool isDead;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector2.zero);
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (damage <= 0 || isDead)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        HealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log(gameObject.name + " took " + damage + " damage. HP: " + currentHealth + "/" + maxHealth);

        if (knockbackDirection.sqrMagnitude > 0.01f)
        {
            Move_Player movePlayer = GetComponent<Move_Player>();
            if (movePlayer != null)
                movePlayer.ApplyKnockback(knockbackDirection, 0.15f);
        }

        if (currentHealth == 0)
            Die();
    }

    public void SetMaxHealth(int newMax, bool healToFull = false)
    {
        maxHealth = Mathf.Max(1, newMax);
        currentHealth = healToFull ? maxHealth : Mathf.Clamp(currentHealth, 1, maxHealth);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Heal(int healAmount)
    {
        if (isDead || healAmount <= 0)
            return;

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        HealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log(gameObject.name + " healed for " + healAmount + " HP. HP: " + currentHealth + "/" + maxHealth);
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Move_Player movePlayer = GetComponent<Move_Player>();
        if (movePlayer != null)
            movePlayer.enabled = false;

        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
            playerAttack.enabled = false;

        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        if (rigidbody2D != null)
        {
            rigidbody2D.linearVelocity = Vector2.zero;
            rigidbody2D.simulated = false;
        }

        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
            collider2D.enabled = false;

        Animator animator = GetComponent<Animator>();
        if (animator != null)
            animator.enabled = false;

        GameOverUI gameOverUI = FindFirstObjectByType<GameOverUI>();
        if (gameOverUI != null)
            gameOverUI.SendMessage("OnPlayerDied", SendMessageOptions.DontRequireReceiver);

        Debug.Log(gameObject.name + " died.");
        Died?.Invoke();
    }
}
