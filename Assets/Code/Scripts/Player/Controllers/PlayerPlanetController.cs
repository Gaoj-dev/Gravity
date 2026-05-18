using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GroundDetector))]
public class PlayerPlanetController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float groundAcceleration = 40f;
    [SerializeField] private float airAcceleration = 20f;
    [SerializeField] private float groundDeceleration = 45f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpHorizontalBoost = 8f;
    [SerializeField] private float gravityScale = 3f;
    public bool IsJumping => !groundDetector.EstaSuelo;

    private GroundDetector groundDetector;
    private float moveInput;
    private bool jumpRequested;
    private Vector2 facingDirection = Vector2.right;
    private bool movementLocked;

    public Vector2 FacingDirection => facingDirection;
    public Rigidbody2D Rigidbody => rb;
    public GroundDetector GroundDetector => groundDetector;
    public float MoveInput => moveInput;
    public bool IsGrounded => groundDetector != null && groundDetector.EstaSuelo;
    public float ForcedGravityScale => Mathf.Max(1f, gravityScale);
    [SerializeField] private SpriteRenderer spriteRenderer;


    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        groundDetector = GetComponent<GroundDetector>();
    }

    private void OnEnable()
    {
        if (rb != null)
        {
            rb.gravityScale = ForcedGravityScale;
            rb.rotation = 0f;
            rb.angularVelocity = 0f;
        }

        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        transform.rotation = Quaternion.identity;

        if (Keyboard.current == null)
        {
            moveInput = 0f;
            jumpRequested = false;
            return;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            moveInput = -1f;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            moveInput = 1f;
        }
        else
        {
            moveInput = 0f;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

        if (moveInput > 0.01f)
        {
            facingDirection = Vector2.right;
            spriteRenderer.flipX = false;
        }
        else if (moveInput < -0.01f)
        {
            facingDirection = Vector2.left;
            spriteRenderer.flipX = true;
        }
    }

    private void FixedUpdate()
    {
        transform.rotation = Quaternion.identity;

        if (rb.gravityScale != ForcedGravityScale)
        {
            rb.gravityScale = ForcedGravityScale;
        }

        Vector2 velocity = rb.linearVelocity;
        if (movementLocked)
        {
            rb.linearVelocity = velocity;
            jumpRequested = false;
            return;
        }

        float targetSpeed = moveInput * moveSpeed;
        float acceleration = groundDetector.EstaSuelo ? groundAcceleration : airAcceleration;

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else if (groundDetector.EstaSuelo)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, 0f, groundDeceleration * Time.fixedDeltaTime);
        }

        rb.linearVelocity = velocity;

        if (jumpRequested && groundDetector.EstaSuelo)
        {
            velocity = rb.linearVelocity;
            velocity.x = moveInput * jumpHorizontalBoost;
            velocity.y = jumpForce;
            rb.linearVelocity = velocity;
        }

        jumpRequested = false;
    }

    public void SetMovementLocked(bool isLocked)
    {
        movementLocked = isLocked;
    }

    public void ConsumeJumpRequest()
    {
        jumpRequested = false;
    }

    public bool ConsumeJumpPressedThisFrame()
    {
        if (!jumpRequested)
        {
            return false;
        }

        jumpRequested = false;
        return true;
    }

    public void ForceFacingDirection(float horizontalDirection)
    {
        if (horizontalDirection > 0.01f)
        {
            facingDirection = Vector2.right;
        }
        else if (horizontalDirection < -0.01f)
        {
            facingDirection = Vector2.left;
        }
    }
}
