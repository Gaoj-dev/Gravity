using System.Collections;
using UnityEngine;

public class AttackReceiver : MonoBehaviour
{
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private int goldDropCount = 0;
    [SerializeField] private float goldSpawnRadius = 0.35f;
    [SerializeField] private float goldSpawnImpulse = 2f;

    private float currentHealth;
    private bool isDead;
    private Rigidbody2D rb;
    private Collider2D[] ownColliders;
    private EnemyChaseController chaseController;
    private EnemyPatrolController patrolController;
    private EnemyContactDamage contactDamage;

    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        ownColliders = GetComponentsInChildren<Collider2D>();
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
        IgnorePlayerCollisions();
        SpawnGoldDrops();

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

    private void IgnorePlayerCollisions()
    {
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        Collider2D[] playerColliders = playerHealth.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D enemyCollider in ownColliders)
        {
            if (enemyCollider == null)
            {
                continue;
            }

            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider == null)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(enemyCollider, playerCollider, true);
            }
        }
    }

    private void SpawnGoldDrops()
    {
        if (goldPrefab == null || goldDropCount <= 0)
        {
            return;
        }

        for (int i = 0; i < goldDropCount; i++)
        {
            Vector2 spawnOffset = Random.insideUnitCircle * goldSpawnRadius;
            GameObject goldInstance = Instantiate(goldPrefab, (Vector2)transform.position + spawnOffset, Quaternion.identity);

            if (goldInstance.GetComponent<GoldPickup>() == null)
            {
                goldInstance.AddComponent<GoldPickup>();
            }

            Rigidbody2D goldRb = goldInstance.GetComponent<Rigidbody2D>();
            if (goldRb != null)
            {
                Vector2 impulseDirection = (spawnOffset == Vector2.zero ? Random.insideUnitCircle : spawnOffset).normalized;
                goldRb.AddForce(impulseDirection * goldSpawnImpulse, ForceMode2D.Impulse);
            }
        }
    }
}
