using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GravityReceiver))]
[RequireComponent(typeof(GroundDetector))]
public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    [Min(0f)] public float maxHorizontalSpeed = 10f;
    [Min(0f)] public float groundAcceleration = 30f;
    [Min(0f)] public float airAcceleration = 10f;
    [Min(0f)] public float groundDeceleration = 35f;
    [Min(0f)] public float salto = 10f;
    [Min(0f)] public float jumpBufferTime = 0.12f;
    [Min(0f)] public float coyoteTime = 0.1f;

    private GravityReceiver gravityReceiver;
    private GroundDetector groundDetector;
    private SpriteRenderer sr;
    private Color originalColor;
    private float moveInput;
    private float lastJumpPressedTime = float.NegativeInfinity;
    private float lastGroundedTime = float.NegativeInfinity;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        gravityReceiver = GetComponent<GravityReceiver>();
        groundDetector = GetComponent<GroundDetector>();
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            moveInput = 0f;
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
            lastJumpPressedTime = Time.time;
        }
    }

    private void FixedUpdate()
    {
        Vector2 gravityDir = gravityReceiver.GetGravityDirection();
        if (gravityDir == Vector2.zero)
        {
            return;
        }

        if (groundDetector.EstaSuelo)
        {
            lastGroundedTime = Time.time;
        }

        Vector2 tangentDir = GetTangentDirection(gravityDir);
        ApplyLateralMovement(tangentDir);

        if (CanJump())
        {
            PerformJump(gravityDir);
        }
    }

    private Vector2 GetTangentDirection(Vector2 gravityDir)
    {
        return Vector2.Perpendicular(gravityDir).normalized;
    }

    private void ApplyLateralMovement(Vector2 tangentDir)
    {
        Vector2 velocity = rb.linearVelocity;
        float tangentialSpeed = Vector2.Dot(velocity, tangentDir);
        float targetSpeed = moveInput * maxHorizontalSpeed;

        float acceleration = groundDetector.EstaSuelo ? groundAcceleration : airAcceleration;
        float newTangentialSpeed;

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            newTangentialSpeed = Mathf.MoveTowards(tangentialSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else if (groundDetector.EstaSuelo)
        {
            newTangentialSpeed = Mathf.MoveTowards(tangentialSpeed, 0f, groundDeceleration * Time.fixedDeltaTime);
        }
        else
        {
            newTangentialSpeed = tangentialSpeed;
        }

        Vector2 tangentialVelocity = tangentDir * newTangentialSpeed;
        Vector2 radialVelocity = velocity - (tangentDir * tangentialSpeed);

        rb.linearVelocity = radialVelocity + tangentialVelocity;
    }

    private bool CanJump()
    {
        bool bufferedJump = Time.time - lastJumpPressedTime <= jumpBufferTime;
        bool coyoteJump = Time.time - lastGroundedTime <= coyoteTime;
        return bufferedJump && coyoteJump;
    }

    private void PerformJump(Vector2 gravityDir)
    {
        lastJumpPressedTime = float.NegativeInfinity;
        lastGroundedTime = float.NegativeInfinity;
        StartCoroutine(FlashRed());

        Vector2 jumpDir = -gravityDir;
        Vector2 velocity = rb.linearVelocity;
        float currentOutwardSpeed = Vector2.Dot(velocity, jumpDir);

        if (currentOutwardSpeed < salto)
        {
            velocity += jumpDir * (salto - currentOutwardSpeed);
        }

        rb.linearVelocity = velocity;
        Debug.DrawRay(transform.position, jumpDir * salto, Color.green, 1f);
    }

    private IEnumerator FlashRed()
    {
        if (sr == null)
        {
            yield break;
        }

        sr.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        sr.color = originalColor;
    }
}
