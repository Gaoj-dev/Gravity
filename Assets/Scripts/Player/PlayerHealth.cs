using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 5f;
    [SerializeField] private float invincibilityDuration = 1f;

    private Rigidbody2D rb;
    private float currentHealth;
    private float invincibleUntil;

    public float CurrentHealth => currentHealth;
    public bool IsInvincible => Time.time < invincibleUntil;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    public bool TryTakeDamage(float damage, Vector2 knockback)
    {
        if (damage <= 0f || IsInvincible)
        {
            return false;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        invincibleUntil = Time.time + invincibilityDuration;

        if (rb != null && knockback != Vector2.zero)
        {
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0f)
        {
            HandleDeath();
        }

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

    // Punto de extension para muerte, animacion o respawn del jugador.
    private void HandleDeath()
    {
    }
}
