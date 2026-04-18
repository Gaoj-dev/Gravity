using UnityEngine;
using UnityEngine.SceneManagement;

public class Planet : MonoBehaviour, IInteractable
{
    [SerializeField] private string sceneToLoad;

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
        SceneManager.LoadScene(sceneToLoad);
    }
}
