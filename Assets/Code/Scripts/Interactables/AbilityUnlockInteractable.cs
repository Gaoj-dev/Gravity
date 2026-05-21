using UnityEngine;

public class AbilityUnlockInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerPlanetAbilities.PlanetAbility ability;

    private bool used;

    public string InteractionId => "UnlockSkill";

    public void Interact()
    {
        if (used)
        {
            return;
        }

        PlayerPlanetAbilities abilities = PlayerModeHandler.Instance != null
            ? PlayerModeHandler.Instance.GetComponent<PlayerPlanetAbilities>()
            : FindFirstObjectByType<PlayerPlanetAbilities>();

        if (abilities == null)
        {
            return;
        }

        abilities.SetAbilityUnlocked(ability, true);
        used = true;
        gameObject.SetActive(false);
    }
}
