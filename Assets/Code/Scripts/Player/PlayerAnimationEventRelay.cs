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
        planetAbilities?.SpawnProjectileFromAnimationEvent();
    }

    public void EndShootAttackFromAnimationEvent()
    {
        planetAbilities?.EndShootAttackFromAnimationEvent();
    }

    public void PerformMeleeHitAnimationEvent()
    {
        meleeAttack?.PerformMeleeHitAnimationEvent();
    }

    public void EndMeleeAttackFromAnimationEvent()
    {
        meleeAttack?.EndMeleeAttackFromAnimationEvent();
    }
}
