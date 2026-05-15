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
        AirDashUpgrade,
        Shoot
    }

    [Header("Ability Unlocks")]
    [SerializeField] private ActiveAbilityConfig doubleJump = new ActiveAbilityConfig { unlocked = false, activationKey = Key.Space };
    [SerializeField] private ActiveAbilityConfig dash = new ActiveAbilityConfig { unlocked = false, activationKey = Key.LeftShift };
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

    [Header("Shoot")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float shootCooldown = 0.25f;
    [SerializeField] private float shootAnimationSafetyTimeout = 0.5833333f;

    private PlayerPlanetController controller;
    private PlayerHealth playerHealth;
    private Collider2D playerCollider;
    private Collider2D[] enemyColliders = new Collider2D[0];

    private bool extraJumpAvailable;
    private bool dashOnCooldown;
    private bool isDashing;
    private bool isShootingAttack;
    private bool shootOnCooldown;
    private bool jumpInputConsumedThisFrame;
    private bool projectileSpawnedThisAttack;

    public bool IsAttacking2Active => isShootingAttack;
    public bool IsDashing => isDashing;

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
        isShootingAttack = false;
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
        HandleDash();
        HandleShoot();
    }

    public bool IsAbilityUnlocked(PlanetAbility ability)
    {
        return ability switch
        {
            PlanetAbility.DoubleJump => doubleJump.unlocked,
            PlanetAbility.Dash => dash.unlocked,
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

    private void HandleShoot()
    {
        if (!shoot.unlocked || shootOnCooldown || isShootingAttack || isDashing)
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

        StartShootAttack();
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        dashOnCooldown = true;
        controller.SetMovementLocked(true);
        playerHealth.ForceInvincibility(dashDuration);

        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemy"),
            true
        );

        float originalGravityScale = controller.Rigidbody.gravityScale;
        controller.Rigidbody.gravityScale = 0f;


        Vector2 dashDirection = controller.FacingDirection;
        controller.Rigidbody.linearVelocity = new Vector2(dashDirection.x * dashSpeed, 0f);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            controller.Rigidbody.linearVelocity = new Vector2(dashDirection.x * dashSpeed, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        controller.Rigidbody.gravityScale = originalGravityScale;

        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemy"),
            false
        );
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

    private void StartShootAttack()
    {
        isShootingAttack = true;
        projectileSpawnedThisAttack = false;
        controller.SetMovementLocked(true);
        controller.Rigidbody.linearVelocity = new Vector2(0f, controller.Rigidbody.linearVelocity.y);
        StartCoroutine(ShootAttackSafetyRoutine());
        StartCoroutine(ShootCooldownRoutine());
    }

    // Animation Event: llamalo en el frame exacto del clip Attack2 donde debe salir el proyectil.
    public void SpawnProjectileFromAnimationEvent()
    {
        if (!isShootingAttack || projectileSpawnedThisAttack || projectilePrefab == null)
        {
            return;
        }

        projectileSpawnedThisAttack = true;

        GameObject projectileInstance = Instantiate(projectilePrefab, GetProjectileSpawnPosition(), Quaternion.identity);
        PlayerProjectile projectile = projectileInstance.GetComponent<PlayerProjectile>();
        if (projectile == null)
        {
            projectile = projectileInstance.AddComponent<PlayerProjectile>();
        }

        projectile.Launch(controller.FacingDirection);
    }

    // Animation Event: llamalo al final del clip Attack2 para devolver el control al jugador.
    public void EndShootAttackFromAnimationEvent()
    {
        if (!isShootingAttack)
        {
            return;
        }

        controller.SetMovementLocked(false);
        isShootingAttack = false;
    }

    private IEnumerator ShootAttackSafetyRoutine()
    {
        yield return new WaitForSeconds(shootAnimationSafetyTimeout);

        if (!isShootingAttack)
        {
            yield break;
        }

        SpawnProjectileFromAnimationEvent();
        EndShootAttackFromAnimationEvent();
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

    private void RefreshEnemyColliders()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1000f, enemyLayers);
        enemyColliders = hits;
    }

    public PlayerAbilitySaveData CaptureSaveData()
    {
        return new PlayerAbilitySaveData
        {
            doubleJump = doubleJump.unlocked,
            dash = dash.unlocked,
            airDashUpgrade = airDashUpgrade.unlocked,
            shoot = shoot.unlocked
        };
    }

    public void ApplySaveData(PlayerAbilitySaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        doubleJump.unlocked = saveData.doubleJump;
        dash.unlocked = saveData.dash;
        airDashUpgrade.unlocked = saveData.airDashUpgrade;
        shoot.unlocked = saveData.shoot;
    }
}
