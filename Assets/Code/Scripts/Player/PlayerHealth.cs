using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 5f;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float hitAnimationDuration = 0.2f;
    [SerializeField] private float deathAnimationDuration = 1f;

    private Rigidbody2D rb;
    private float currentHealth;
    private float invincibleUntil;
    private float hitAnimationUntil;
    private bool isDead;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
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

    public bool TryTakeDamage(float damage, Vector2 knockback)
    {
        if (damage <= 0f || IsInvincible || isDead)
        {
            return false;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);
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

    public void SetCurrentHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        isDead = currentHealth <= 0f;
    }

    private void HandleDeath()
    {
        Died?.Invoke();
    }
}
