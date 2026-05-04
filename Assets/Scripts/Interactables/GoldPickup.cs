using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    [SerializeField] private int goldAmount = 1;
    [SerializeField] private float attractionRadius = 3f;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float pickupDistance = 0.2f;
    [SerializeField] private float lifetime = 10f;

    private Rigidbody2D rb;
    private PlayerGoldWallet playerWallet;
    private float destroyAtTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        destroyAtTime = Time.time + lifetime;
    }

    private void Update()
    {
        if (Time.time >= destroyAtTime)
        {
            Destroy(gameObject);
            return;
        }

        if (playerWallet == null)
        {
            playerWallet = FindFirstObjectByType<PlayerGoldWallet>();
            if (playerWallet == null)
            {
                return;
            }
        }

        Vector2 targetPosition = playerWallet.transform.position;
        float distanceToPlayer = Vector2.Distance(transform.position, targetPosition);

        if (distanceToPlayer <= pickupDistance)
        {
            Collect();
            return;
        }

        if (distanceToPlayer <= attractionRadius)
        {
            Vector2 nextPosition = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (rb != null)
            {
                rb.MovePosition(nextPosition);
            }
            else
            {
                transform.position = nextPosition;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollectFromCollider(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryCollectFromCollider(collision.collider);
    }

    private void TryCollectFromCollider(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        PlayerGoldWallet wallet = other.GetComponent<PlayerGoldWallet>();
        if (wallet == null && other.attachedRigidbody != null)
        {
            wallet = other.attachedRigidbody.GetComponent<PlayerGoldWallet>();
        }

        if (wallet == null)
        {
            wallet = other.GetComponentInParent<PlayerGoldWallet>();
        }

        if (wallet == null)
        {
            return;
        }

        playerWallet = wallet;
        Collect();
    }

    private void Collect()
    {
        if (playerWallet == null)
        {
            return;
        }

        playerWallet.AddGold(goldAmount);
        Destroy(gameObject);
    }
}
