using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrolController : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float waitAtPointTime = 1.2f;
    [SerializeField] private float arrivalTolerance = 0.15f;

    [Header("Platform Safety")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private Vector2 ledgeCheckOffset = new Vector2(0.35f, -0.4f);
    [SerializeField] private float ledgeCheckDistance = 0.7f;
    [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.45f, 0f);
    [SerializeField] private float wallCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private Vector2 patrolCenter;
    private Vector2 currentTarget;
    private float nextTargetTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolCenter = transform.position;
        currentTarget = patrolCenter;
    }

    private void FixedUpdate()
    {
        if (Time.time >= nextTargetTime && Vector2.Distance(transform.position, currentTarget) <= arrivalTolerance)
        {
            PickNextTarget();
        }

        MoveTowardsTarget();
    }

    private void PickNextTarget()
    {
        float randomOffset = Random.Range(-patrolRadius, patrolRadius);
        currentTarget = new Vector2(patrolCenter.x + randomOffset, transform.position.y);
        nextTargetTime = Time.time + waitAtPointTime;
    }

    private void MoveTowardsTarget()
    {
        float deltaX = currentTarget.x - transform.position.x;
        if (Mathf.Abs(deltaX) <= arrivalTolerance)
        {
            SetHorizontalVelocity(0f);
            return;
        }

        float direction = Mathf.Sign(deltaX);
        if (!CanMove(direction))
        {
            SetHorizontalVelocity(0f);
            nextTargetTime = 0f;
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

    private void OnDrawGizmosSelected()
    {
        Vector2 center;
        if (Application.isPlaying) center = patrolCenter;
        else center = transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, patrolRadius);
    }
}
