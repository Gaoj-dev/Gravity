using UnityEngine;

public class Planet : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionId = "Enter";
    [SerializeField] private string sceneToLoad = "TestWorld1";

    public string InteractionId => interactionId;

    public void Interact()
    {
        PlayInteractionAnimation();
        ChangeScene();
    }

    private void PlayInteractionAnimation()
    {
        // Animación futura
    }

    private void ChangeScene()
    {
        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogWarning($"Planet {name} has no scene assigned.", this);
            return;
        }

        if (PlayerModeHandler.Instance != null && GameModeManager.CurrentMode == GameMode.Space)
            GameModeManager.StoreSpaceReturnPosition(PlayerModeHandler.Instance.transform.position);

        GameModeManager.LoadSceneForMode(sceneToLoad, GameMode.Planet);
    }
}
