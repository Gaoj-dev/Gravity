using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaseController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float returnTolerance = 0.1f;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private float loseTargetDelay = 2f;
    [SerializeField] private LayerMask playerLayers = ~0;

    [Header("Platform Safety")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private Vector2 ledgeCheckOffset = new Vector2(0.35f, -0.4f);
    [SerializeField] private float ledgeCheckDistance = 0.7f;
    [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.45f, 0f);
    [SerializeField] private float wallCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private Vector2 homePosition;
    private Transform playerTarget;
    private float lastSeenPlayerTime = float.NegativeInfinity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        homePosition = transform.position;
    }

    private void Update()
    {
        UpdateTarget();
    }

    private void FixedUpdate()
    {
        float targetX = homePosition.x;

        if (playerTarget != null)
        {
            targetX = playerTarget.position.x;
        }

        MoveTowardsX(targetX);
    }

    private void UpdateTarget()
    {
        Transform detectedPlayer = FindPlayerInRange();
        if (detectedPlayer != null)
        {
            playerTarget = detectedPlayer;
            lastSeenPlayerTime = Time.time;
            return;
        }

        if (playerTarget != null && Time.time - lastSeenPlayerTime >= loseTargetDelay)
        {
            playerTarget = null;
        }
    }

    private Transform FindPlayerInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayers);
        foreach (Collider2D hit in hits)
        {
            PlayerHealth playerHealth = ResolvePlayerHealth(hit);
            if (playerHealth != null)
            {
                return playerHealth.transform;
            }
        }

        return null;
    }

    // Se mueve solo si hay suelo delante y no hay pared inmediata, para no caerse de plataformas.
    private void MoveTowardsX(float targetX)
    {
        float deltaX = targetX - transform.position.x;
        if (Mathf.Abs(deltaX) <= returnTolerance)
        {
            SetHorizontalVelocity(0f);
            return;
        }

        float direction = Mathf.Sign(deltaX);
        if (!CanMove(direction))
        {
            SetHorizontalVelocity(0f);
            return;
        }

        SetHorizontalVelocity(direction * moveSpeed);
    }

    private void SetHorizontalVelocity(float targetSpeed)
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = velocity;
    }

    private bool CanMove(float direction)
    {
        Vector2 ledgeOrigin = (Vector2)transform.position + new Vector2(ledgeCheckOffset.x * direction, ledgeCheckOffset.y);
        bool hasGroundAhead = Physics2D.Raycast(ledgeOrigin, Vector2.down, ledgeCheckDistance, groundLayers);

        Vector2 wallOrigin = (Vector2)transform.position + new Vector2(wallCheckOffset.x * direction, wallCheckOffset.y);
        bool hasWallAhead = Physics2D.Raycast(wallOrigin, Vector2.right * direction, wallCheckDistance, groundLayers);

        return hasGroundAhead && !hasWallAhead;
    }

    private PlayerHealth ResolvePlayerHealth(Collider2D hit)
    {
        if (hit == null)
        {
            return null;
        }

        PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            return playerHealth;
        }

        if (hit.attachedRigidbody != null)
        {
            playerHealth = hit.attachedRigidbody.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                return playerHealth;
            }
        }

        return hit.GetComponentInParent<PlayerHealth>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? homePosition : (Vector2)transform.position, 0.2f);
    }
}
