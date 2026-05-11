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
    private static readonly int IsAttacking2Hash = Animator.StringToHash("IsAttacking2");
    private static readonly int IsHitHash = Animator.StringToHash("IsHit");
    private static readonly int IsDeadHash = Animator.StringToHash("isDead");

    private Rigidbody2D rb;

    [SerializeField] private PlayerPlanetController planetController;
    [SerializeField] private PlayerSpaceController spaceController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
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

        // Velocidad global del rigidbody
        Vector2 worldVelocity = rb.linearVelocity;

        // Ejes locales del objeto (en 2D usamos right y up del transform)
        Vector2 localRight = transform.right;
        Vector2 localUp    = transform.up;

        // Proyección sobre los ejes locales → velocidad en espacio local
        float horizontalSpeed = Vector2.Dot(worldVelocity, localRight);
        float verticalSpeed   = Vector2.Dot(worldVelocity, localUp);

        bool isJumping = false;

        if (planetController != null && planetController.enabled)
        {
            isJumping = planetController.IsJumping;
        }
        else if (spaceController != null && spaceController.enabled)
        {
            isJumping = spaceController.IsJumping;
        }

        animator.SetFloat(HorizontalSpeedHash, Mathf.Abs(horizontalSpeed));
        animator.SetFloat(VerticalSpeedHash, verticalSpeed);
        animator.SetBool(IsJumpingHash, isJumping);
        animator.SetBool(IsAttacking1Hash, meleeAttack != null && meleeAttack.IsAttacking1Active);
        animator.SetBool(IsAttacking2Hash, planetAbilities != null && planetAbilities.IsAttacking2Active);
        animator.SetBool(IsHitHash, playerHealth != null && playerHealth.IsHit);
        animator.SetBool(IsDeadHash, playerHealth != null && playerHealth.IsDead);
    }
}
