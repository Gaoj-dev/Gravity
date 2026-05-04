using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(PlayerPlanetController))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(Collider2D))]
public class PlayerPlanetAbilities : MonoBehaviour
{
    [System.Serializable]
    private class ActiveAbilityConfig
    {
        public bool unlocked;
        public Key activationKey = Key.None;
    }

    [System.Serializable]
    private class PassiveAbilityConfig
    {
        public bool unlocked;
    }

    public enum PlanetAbility
    {
        DoubleJump,
        Dash,
        WallJump,
        AirDashUpgrade,
        Shoot
    }

    [Header("Ability Unlocks")]
    [SerializeField] private ActiveAbilityConfig doubleJump = new ActiveAbilityConfig { unlocked = false, activationKey = Key.Space };
    [SerializeField] private ActiveAbilityConfig dash = new ActiveAbilityConfig { unlocked = false, activationKey = Key.LeftShift };
    [SerializeField] private ActiveAbilityConfig wallJump = new ActiveAbilityConfig { unlocked = false, activationKey = Key.Space };
    [SerializeField] private PassiveAbilityConfig airDashUpgrade = new PassiveAbilityConfig { unlocked = false };
    [SerializeField] private ActiveAbilityConfig shoot = new ActiveAbilityConfig { unlocked = false, activationKey = Key.F };

    [Header("Double Jump")]
    [SerializeField] private float doubleJumpForce = 12f;
    [SerializeField] private float doubleJumpHorizontalBoost = 8f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] private LayerMask enemyLayers = ~0;

    [Header("Wall Jump")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float wallCheckDistance = 0.15f;
    [SerializeField] private float wallSlideSpeed = -2f;
    [SerializeField] private float wallJumpHorizontalForce = 9f;
    [SerializeField] private float wallJumpVerticalForce = 12f;

    [Header("Shoot")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float shootCooldown = 0.25f;

    private PlayerPlanetController controller;
    private PlayerHealth playerHealth;
    private Collider2D playerCollider;
    private Collider2D[] enemyColliders = new Collider2D[0];

    private bool extraJumpAvailable;
    private bool dashOnCooldown;
    private bool isDashing;
    private bool shootOnCooldown;
    private bool jumpInputConsumedThisFrame;

    private void Awake()
    {
        controller = GetComponent<PlayerPlanetController>();
        playerHealth = GetComponent<PlayerHealth>();
        playerCollider = GetComponent<Collider2D>();

        if (projectileSpawnPoint == null)
        {
            projectileSpawnPoint = transform;
        }
    }

    private void OnEnable()
    {
        extraJumpAvailable = true;
        isDashing = false;
        controller.SetMovementLocked(false);
    }

    private void Update()
    {
        if (controller.IsGrounded)
        {
            extraJumpAvailable = true;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        jumpInputConsumedThisFrame = false;
        HandleDoubleJump();
        HandleWallJumpAndSlide();
        HandleDash();
        HandleShoot();
    }

    public bool IsAbilityUnlocked(PlanetAbility ability)
    {
        return ability switch
        {
            PlanetAbility.DoubleJump => doubleJump.unlocked,
            PlanetAbility.Dash => dash.unlocked,
            PlanetAbility.WallJump => wallJump.unlocked,
            PlanetAbility.AirDashUpgrade => airDashUpgrade.unlocked,
            PlanetAbility.Shoot => shoot.unlocked,
            _ => false
        };
    }

    // Permite desbloquear o desactivar habilidades desde pickups, UI o eventos.
    public void SetAbilityUnlocked(PlanetAbility ability, bool unlocked)
    {
        switch (ability)
        {
            case PlanetAbility.DoubleJump:
                doubleJump.unlocked = unlocked;
                break;
            case PlanetAbility.Dash:
                dash.unlocked = unlocked;
                break;
            case PlanetAbility.WallJump:
                wallJump.unlocked = unlocked;
                break;
            case PlanetAbility.AirDashUpgrade:
                airDashUpgrade.unlocked = unlocked;
                break;
            case PlanetAbility.Shoot:
                shoot.unlocked = unlocked;
                break;
        }
    }

    private void HandleDoubleJump()
    {
        if (!doubleJump.unlocked || controller.IsGrounded || !extraJumpAvailable)
        {
            return;
        }

        if (!ConsumeAbilityPress(doubleJump))
        {
            return;
        }

        Vector2 velocity = controller.Rigidbody.linearVelocity;
        if (Mathf.Abs(controller.MoveInput) > 0.01f)
        {
            velocity.x = controller.MoveInput * doubleJumpHorizontalBoost;
        }

        velocity.y = doubleJumpForce;
        controller.Rigidbody.linearVelocity = velocity;
        extraJumpAvailable = false;
    }

    private void HandleDash()
    {
        if (!dash.unlocked || isDashing || dashOnCooldown)
        {
            return;
        }

        if (!ConsumeAbilityPress(dash))
        {
            return;
        }

        if (!controller.IsGrounded && !airDashUpgrade.unlocked)
        {
            return;
        }

        StartCoroutine(DashRoutine());
    }

    private void HandleWallJumpAndSlide()
    {
        if (!wallJump.unlocked || controller.IsGrounded)
        {
            return;
        }

        if (!TryGetWallSlideDirection(out int wallDirection))
        {
            return;
        }

        Vector2 velocity = controller.Rigidbody.linearVelocity;
        bool isPressingTowardsWall =
            (wallDirection < 0 && controller.MoveInput < -0.01f) ||
            (wallDirection > 0 && controller.MoveInput > 0.01f);
        bool isFalling = velocity.y < 0f;

        // Solo frena la caida cuando realmente estamos deslizandonos contra la pared.
        if (isFalling && isPressingTowardsWall && velocity.y < wallSlideSpeed)
        {
            velocity.y = wallSlideSpeed;
            controller.Rigidbody.linearVelocity = velocity;
        }

        if (!ConsumeAbilityPress(wallJump))
        {
            return;
        }

        velocity = controller.Rigidbody.linearVelocity;
        velocity.x = -wallDirection * wallJumpHorizontalForce;
        velocity.y = wallJumpVerticalForce;
        controller.Rigidbody.linearVelocity = velocity;
        controller.ForceFacingDirection(-wallDirection);
        extraJumpAvailable = doubleJump.unlocked;
    }

    private void HandleShoot()
    {
        if (!shoot.unlocked || shootOnCooldown)
        {
            return;
        }

        if (!ConsumeAbilityPress(shoot))
        {
            return;
        }

        if (projectilePrefab == null)
        {
            return;
        }

        GameObject projectileInstance = Instantiate(projectilePrefab, GetProjectileSpawnPosition(), Quaternion.identity);
        PlayerProjectile projectile = projectileInstance.GetComponent<PlayerProjectile>();
        if (projectile == null)
        {
            projectile = projectileInstance.AddComponent<PlayerProjectile>();
        }

        projectile.Launch(controller.FacingDirection);
        StartCoroutine(ShootCooldownRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        dashOnCooldown = true;
        controller.SetMovementLocked(true);
        playerHealth.ForceInvincibility(dashDuration);
        SetEnemyCollisionIgnored(true);

        Vector2 dashDirection = controller.FacingDirection;
        controller.Rigidbody.linearVelocity = new Vector2(dashDirection.x * dashSpeed, 0f);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            controller.Rigidbody.linearVelocity = new Vector2(dashDirection.x * dashSpeed, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetEnemyCollisionIgnored(false);
        controller.SetMovementLocked(false);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        dashOnCooldown = false;
    }

    private IEnumerator ShootCooldownRoutine()
    {
        shootOnCooldown = true;
        yield return new WaitForSeconds(shootCooldown);
        shootOnCooldown = false;
    }

    private bool TryGetWallSlideDirection(out int wallDirection)
    {
        wallDirection = 0;

        Bounds bounds = playerCollider.bounds;
        float verticalOffset = bounds.extents.y * 0.35f;
        Vector2 leftUpperOrigin = new Vector2(bounds.min.x, bounds.center.y + verticalOffset);
        Vector2 leftLowerOrigin = new Vector2(bounds.min.x, bounds.center.y - verticalOffset);
        Vector2 rightUpperOrigin = new Vector2(bounds.max.x, bounds.center.y + verticalOffset);
        Vector2 rightLowerOrigin = new Vector2(bounds.max.x, bounds.center.y - verticalOffset);
        Vector2 groundCheckOrigin = new Vector2(bounds.center.x, bounds.min.y);

        bool touchingLeftWall =
            Physics2D.Raycast(leftUpperOrigin, Vector2.left, wallCheckDistance, groundLayers) ||
            Physics2D.Raycast(leftLowerOrigin, Vector2.left, wallCheckDistance, groundLayers);
        bool touchingRightWall =
            Physics2D.Raycast(rightUpperOrigin, Vector2.right, wallCheckDistance, groundLayers) ||
            Physics2D.Raycast(rightLowerOrigin, Vector2.right, wallCheckDistance, groundLayers);
        bool nearGroundBelow = Physics2D.Raycast(groundCheckOrigin, Vector2.down, wallCheckDistance * 1.5f, groundLayers);
        bool isFalling = controller.Rigidbody.linearVelocity.y < 0f;

        if (touchingLeftWall && !nearGroundBelow && isFalling)
        {
            wallDirection = -1;
            return true;
        }

        if (touchingRightWall && !nearGroundBelow && isFalling)
        {
            wallDirection = 1;
            return true;
        }

        return false;
    }

    private bool ConsumeAbilityPress(ActiveAbilityConfig ability)
    {
        if (Keyboard.current == null || ability == null)
        {
            return false;
        }

        if (ability.activationKey == Key.None)
        {
            return false;
        }

        if (ability.activationKey == Key.Space && jumpInputConsumedThisFrame)
        {
            return false;
        }

        KeyControl keyControl = Keyboard.current[ability.activationKey];
        bool wasPressed = keyControl != null && keyControl.wasPressedThisFrame;
        if (wasPressed && ability.activationKey == Key.Space)
        {
            jumpInputConsumedThisFrame = true;
        }

        return wasPressed;
    }

    private Vector3 GetProjectileSpawnPosition()
    {
        Transform currentSpawnPoint = projectileSpawnPoint;
        if (currentSpawnPoint != null && currentSpawnPoint.IsChildOf(transform))
        {
            return currentSpawnPoint.position;
        }

        float horizontalOffset = controller != null ? controller.FacingDirection.x * 0.6f : 0f;
        return transform.position + new Vector3(horizontalOffset, 0f, 0f);
    }

    private void SetEnemyCollisionIgnored(bool ignored)
    {
        if (playerCollider == null)
        {
            return;
        }

        RefreshEnemyColliders();
        foreach (Collider2D enemyCollider in enemyColliders)
        {
            if (enemyCollider == null)
            {
                continue;
            }

            Physics2D.IgnoreCollision(playerCollider, enemyCollider, ignored);
        }
    }

    private void RefreshEnemyColliders()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1000f, enemyLayers);
        enemyColliders = hits;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCollider == null && !Application.isPlaying)
        {
            playerCollider = GetComponent<Collider2D>();
        }

        if (playerCollider == null)
        {
            return;
        }

        Bounds bounds = playerCollider.bounds;
        float verticalOffset = bounds.extents.y * 0.35f;
        Vector2 leftUpperOrigin = new Vector2(bounds.min.x, bounds.center.y + verticalOffset);
        Vector2 leftLowerOrigin = new Vector2(bounds.min.x, bounds.center.y - verticalOffset);
        Vector2 rightUpperOrigin = new Vector2(bounds.max.x, bounds.center.y + verticalOffset);
        Vector2 rightLowerOrigin = new Vector2(bounds.max.x, bounds.center.y - verticalOffset);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(leftUpperOrigin, leftUpperOrigin + Vector2.left * wallCheckDistance);
        Gizmos.DrawLine(leftLowerOrigin, leftLowerOrigin + Vector2.left * wallCheckDistance);
        Gizmos.DrawLine(rightUpperOrigin, rightUpperOrigin + Vector2.right * wallCheckDistance);
        Gizmos.DrawLine(rightLowerOrigin, rightLowerOrigin + Vector2.right * wallCheckDistance);
    }
}
