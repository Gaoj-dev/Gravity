using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float knockbackForce = 4f;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask hittableLayers = ~0;

    private Vector2 direction = Vector2.right;
    private Rigidbody2D rb;
    private bool hasLaunched;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 launchDirection)
    {
        hasLaunched = true;
        direction = launchDirection.normalized == Vector2.zero ? Vector2.right : launchDirection.normalized;

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (rb == null)
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHit(collision.collider);
    }

    private void TryHit(Collider2D other)
    {
        if (!hasLaunched)
        {
            return;
        }

        if (((1 << other.gameObject.layer) & hittableLayers.value) == 0)
        {
            return;
        }

        AttackReceiver receiver = ResolveReceiver(other);
        if (receiver != null)
        {
            receiver.ReceiveAttack(damage, direction, knockbackForce);
        }

        Destroy(gameObject);
    }

    private AttackReceiver ResolveReceiver(Collider2D hit)
    {
        if (hit == null)
        {
            return null;
        }

        AttackReceiver receiver = hit.GetComponent<AttackReceiver>();
        if (receiver != null)
        {
            return receiver;
        }

        if (hit.attachedRigidbody != null)
        {
            receiver = hit.attachedRigidbody.GetComponent<AttackReceiver>();
            if (receiver != null)
            {
                return receiver;
            }
        }

        return hit.GetComponentInParent<AttackReceiver>();
    }
}
