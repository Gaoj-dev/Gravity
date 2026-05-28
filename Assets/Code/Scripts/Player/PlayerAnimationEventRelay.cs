using UnityEngine;

public class PlayerAnimationEventRelay : MonoBehaviour
{
    [SerializeField] private PlayerPlanetAbilities planetAbilities;
    [SerializeField] private PlayerMeleeAttack meleeAttack;

    private void Awake()
    {
        if (planetAbilities == null)
        {
            planetAbilities = GetComponentInParent<PlayerPlanetAbilities>();
        }

        if (meleeAttack == null)
        {
            meleeAttack = GetComponentInParent<PlayerMeleeAttack>();
        }
    }

    public void SpawnProjectileFromAnimationEvent()
    {
        if (planetAbilities != null) planetAbilities.SpawnProjectileFromAnimationEvent();
    }

    public void EndShootAttackFromAnimationEvent()
    {
        if (planetAbilities != null) planetAbilities.EndShootAttackFromAnimationEvent();
    }

    public void PerformMeleeHitAnimationEvent()
    {
        if (meleeAttack != null) meleeAttack.PerformMeleeHitAnimationEvent();
    }

    public void EndMeleeAttackFromAnimationEvent()
    {
        if (meleeAttack != null) meleeAttack.EndMeleeAttackFromAnimationEvent();
    }
}
