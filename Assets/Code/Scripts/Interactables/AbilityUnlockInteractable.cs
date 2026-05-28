using UnityEngine;

public class AbilityUnlockInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerPlanetAbilities.PlanetAbility ability;

    private bool used;

    public string InteractionId => "UnlockSkill";

    private void Start()
    {
        PlayerPlanetAbilities abilities = PlayerModeHandler.Instance != null
            ? PlayerModeHandler.Instance.GetComponent<PlayerPlanetAbilities>()
            : FindFirstObjectByType<PlayerPlanetAbilities>();

        if (abilities != null && abilities.IsAbilityUnlocked(ability))
        {
            used = true;
            gameObject.SetActive(false);
        }
    }

    public void Interact()
    {
        if (used)
        {
            return;
        }

        PlayerPlanetAbilities abilities;

        if (PlayerModeHandler.Instance != null)
        {
            abilities = PlayerModeHandler.Instance.GetComponent<PlayerPlanetAbilities>();
        }
        else
        {
            abilities = FindFirstObjectByType<PlayerPlanetAbilities>();
        }

        if (abilities == null)
        {
            return;
        }

        abilities.SetAbilityUnlocked(ability, true);
        used = true;
        gameObject.SetActive(false);
    }
}
