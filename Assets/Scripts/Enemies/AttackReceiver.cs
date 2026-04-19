using System.Collections;
using UnityEngine;

public class AttackReceiver : MonoBehaviour
{
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float destroyDelay = 2f;

    private float currentHealth;
    private bool isDead;
    private Rigidbody2D rb;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    public void ReceiveAttack(float damage, Vector2 attackDirection, float knockbackForce)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        if (rb != null)
        {
            Vector2 knockbackDirection = new Vector2(Mathf.Sign(attackDirection.x), 0.35f).normalized;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        if (rb != null)
        {
            rb.freezeRotation = false;
        }

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
