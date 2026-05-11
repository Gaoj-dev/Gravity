using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float contactInterval = 0.2f;

    private float nextDamageTime;
    private AttackReceiver attackReceiver;

    private void Awake()
    {
        attackReceiver = GetComponent<AttackReceiver>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider2D target)
    {
        if (attackReceiver != null && attackReceiver.IsDead)
        {
            return;
        }

        if (Time.time < nextDamageTime)
        {
            return;
        }

        PlayerHealth playerHealth = ResolvePlayerHealth(target);
        if (playerHealth == null)
        {
            return;
        }

        Vector2 direction = (target.transform.position.x >= transform.position.x) ? Vector2.right : Vector2.left;
        Vector2 knockback = new Vector2(direction.x * knockbackForce, knockbackForce * 0.35f);

        if (playerHealth.TryTakeDamage(damage, knockback))
        {
            nextDamageTime = Time.time + contactInterval;
        }
    }

    private PlayerHealth ResolvePlayerHealth(Collider2D target)
    {
        if (target == null)
        {
            return null;
        }

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            return playerHealth;
        }

        if (target.attachedRigidbody != null)
        {
            playerHealth = target.attachedRigidbody.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                return playerHealth;
            }
        }

        return target.GetComponentInParent<PlayerHealth>();
    }
}
