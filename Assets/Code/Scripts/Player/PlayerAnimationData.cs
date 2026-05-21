using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimationData : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMeleeAttack meleeAttack;
    [SerializeField] private PlayerPlanetAbilities planetAbilities;
    [SerializeField] private PlayerHealth playerHealth;

    private static readonly int HorizontalSpeedHash = Animator.StringToHash("HorizontalSpeed");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int IsAttacking1Hash = Animator.StringToHash("IsAttacking1");
    private static readonly int AttackTriggerHash = Animator.StringToHash("AttackTrigger");
    private static readonly int IsAttacking2Hash = Animator.StringToHash("IsAttacking2");
    private static readonly int IsDashHash = Animator.StringToHash("IsDash");
    private static readonly int HitTriggerHash = Animator.StringToHash("HitTrigger");
    private static readonly int DeathTriggerHash = Animator.StringToHash("DeathTrigger");

    private Rigidbody2D rb;
    private bool wasAttacking1LastFrame;
    private bool prevIsHit;
    private bool prevIsDead;
    private bool prevAnimIsDead;

    [SerializeField] private PlayerPlanetController planetController;
    [SerializeField] private PlayerSpaceController spaceController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("[Anim] No se encontro Animator en el jugador ni en sus hijos.", this);
        }

        if (planetController == null)
        {
            planetController = GetComponent<PlayerPlanetController>();
        }

        if (spaceController == null)
        {
            spaceController = GetComponent<PlayerSpaceController>();
        }

        if (meleeAttack == null)
        {
            meleeAttack = GetComponent<PlayerMeleeAttack>();
        }

        if (planetAbilities == null)
        {
            planetAbilities = GetComponent<PlayerPlanetAbilities>();
        }

        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        if (animator == null)
        {
            return;
        }

        Vector2 worldVelocity = rb.linearVelocity;
        Vector2 localRight = transform.right;
        Vector2 localUp    = transform.up;

        float horizontalSpeed = Vector2.Dot(worldVelocity, localRight);
        float verticalSpeed   = Vector2.Dot(worldVelocity, localUp);

        bool isHit  = playerHealth != null && playerHealth.IsHit;
        bool isDead = playerHealth != null && playerHealth.IsDead;
        bool animIsDead = isDead && !isHit;

        // Respawn: isDead paso de true a false — resetear el animator al estado inicial
        if (prevIsDead && !isDead)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        bool isJumping = false;
        bool isDashing = planetAbilities != null && planetAbilities.IsDashing;
        bool isGrounded = false;

        if (planetController != null && planetController.enabled)
        {
            isJumping = planetController.IsJumping;
            isGrounded = planetController.IsGrounded;
        }
        else if (spaceController != null && spaceController.enabled)
        {
            isJumping = spaceController.IsJumping;
        }

        if (isDead || isHit)
        {
            isJumping = false;
            isDashing = false;
            verticalSpeed = 0f;
            horizontalSpeed = 0f;
        }
        else if (isDashing)
        {
            isJumping = false;
            verticalSpeed = 0f;
        }
        else if (isGrounded)
        {
            verticalSpeed = 0f;
        }

        animator.SetFloat(HorizontalSpeedHash, Mathf.Abs(horizontalSpeed));
        animator.SetFloat(VerticalSpeedHash, verticalSpeed);
        animator.SetBool(IsJumpingHash, isJumping);

        bool attackingNow = meleeAttack != null && meleeAttack.IsAttacking1Active;
        animator.SetBool(IsAttacking1Hash, attackingNow);
        if (attackingNow && !wasAttacking1LastFrame)
        {
            animator.SetTrigger(AttackTriggerHash);
        }

        wasAttacking1LastFrame = attackingNow;
        animator.SetBool(IsAttacking2Hash, planetAbilities != null && planetAbilities.IsAttacking2Active);
        animator.SetBool(IsDashHash, isDashing);

        if (isHit && !prevIsHit)
        {
            animator.SetTrigger(HitTriggerHash);
        }

        if (animIsDead && !prevAnimIsDead)
        {
            animator.SetTrigger(DeathTriggerHash);
        }

        prevIsHit     = isHit;
        prevIsDead    = isDead;
        prevAnimIsDead = animIsDead;
    }
}
