using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float hitAnimationDuration = 0.2f;
    [SerializeField] private float deathAnimationDuration = 1f;

    private Rigidbody2D rb;
    private int currentHealth;
    private float invincibleUntil;
    private float hitAnimationUntil;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible => Time.time < invincibleUntil;
    public bool IsHit => Time.time < hitAnimationUntil;
    public bool IsDead => isDead;
    public float DeathAnimationDuration => deathAnimationDuration;

    public event Action Damaged;
    public event Action Died;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    public bool TryTakeDamage(int damage, Vector2 knockback)
    {
        if (damage <= 0 || IsInvincible || isDead)
        {
            return false;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        invincibleUntil = Time.time + invincibilityDuration;
        hitAnimationUntil = Time.time + hitAnimationDuration;

        if (rb != null && knockback != Vector2.zero)
        {
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0f)
        {
            isDead = true;
            HandleDeath();
            return true;
        }

        Damaged?.Invoke();

        return true;
    }

    public void ForceInvincibility(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        invincibleUntil = Mathf.Max(invincibleUntil, Time.time + duration);
    }

    public void SetCurrentHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        isDead = currentHealth <= 0f;
    }

    private void HandleDeath()
    {
        int count = Died != null ? Died.GetInvocationList().Length : 0;
        Debug.Log($"[PlayerHealth] HandleDeath invocado instanceID={GetInstanceID()} Suscriptores: {count}");
        Died?.Invoke();
    }
}
