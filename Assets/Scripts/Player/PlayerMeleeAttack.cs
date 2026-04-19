using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerPlanetController))]
public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackCooldown = 0.2f;
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.25f, 0.9f);
    [SerializeField] private float attackDistance = 0.9f;
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private LayerMask hittableLayers = ~0;
    [SerializeField] private Transform attackOrigin;

    private PlayerPlanetController planetController;
    private float nextAttackTime;

    private void Awake()
    {
        planetController = GetComponent<PlayerPlanetController>();

        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }
    }

    private void Update()
    {
        if (Mouse.current == null || !enabled)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;

        Vector2 facingDirection = planetController.FacingDirection;
        Vector2 attackCenter = (Vector2)attackOrigin.position + facingDirection * attackDistance;
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackCenter, attackBoxSize, 0f, hittableLayers);
        HashSet<AttackReceiver> hitReceivers = new HashSet<AttackReceiver>();

        foreach (Collider2D hit in hits)
        {
            AttackReceiver receiver = ResolveReceiver(hit);
            if (receiver == null || !hitReceivers.Add(receiver))
            {
                continue;
            }

            receiver.ReceiveAttack(attackDamage, facingDirection, knockbackForce);
        }
    }

    // Permite que el collider golpeado este en un hijo del enemigo.
    private AttackReceiver ResolveReceiver(Collider2D hit)
    {
        if (hit == null)
        {
            return null;
        }

        AttackReceiver receiver = hit.GetComponent<AttackReceiver>();
        if (receiver != null)
        {
            return receiver;
        }

        if (hit.attachedRigidbody != null)
        {
            receiver = hit.attachedRigidbody.GetComponent<AttackReceiver>();
            if (receiver != null)
            {
                return receiver;
            }
        }

        return hit.GetComponentInParent<AttackReceiver>();
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = attackOrigin != null ? attackOrigin : transform;
        Vector2 facing = transform.right;
        PlayerPlanetController controller = Application.isPlaying ? planetController : GetComponent<PlayerPlanetController>();

        if (controller != null)
        {
            facing = controller.FacingDirection;
        }

        Vector2 attackCenter = (Vector2)origin.position + facing.normalized * attackDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackCenter, attackBoxSize);
    }
}
