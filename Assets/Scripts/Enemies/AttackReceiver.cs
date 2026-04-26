using System.Collections;
using UnityEngine;

public class AttackReceiver : MonoBehaviour
{
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float destroyDelay = 2f;

    private float currentHealth;
    private bool isDead;
    private Rigidbody2D rb;
    private EnemyChaseController chaseController;
    private EnemyPatrolController patrolController;
    private EnemyContactDamage contactDamage;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        chaseController = GetComponent<EnemyChaseController>();
        patrolController = GetComponent<EnemyPatrolController>();
        contactDamage = GetComponent<EnemyContactDamage>();
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
        DisableEnemyBehaviours();

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

    private void DisableEnemyBehaviours()
    {
        if (chaseController != null)
        {
            chaseController.enabled = false;
        }

        if (patrolController != null)
        {
            patrolController.enabled = false;
        }

        if (contactDamage != null)
        {
            contactDamage.enabled = false;
        }
    }
}
