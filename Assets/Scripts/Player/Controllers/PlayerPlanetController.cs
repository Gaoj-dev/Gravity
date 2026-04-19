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

    private GroundDetector groundDetector;
    private float moveInput;
    private bool jumpRequested;
    private Vector2 facingDirection = Vector2.right;

    public Vector2 FacingDirection => facingDirection;

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
            rb.gravityScale = Mathf.Max(1f, gravityScale);
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

        if (moveInput > 0.01f)
        {
            facingDirection = Vector2.right;
        }
        else if (moveInput < -0.01f)
        {
            facingDirection = Vector2.left;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        transform.rotation = Quaternion.identity;

        Vector2 velocity = rb.linearVelocity;
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
}
