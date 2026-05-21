using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MainMenuUI))]
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string newGameSceneName = "TutorialArea";
    [SerializeField] private SaveMenuController saveMenuController;

    private MainMenuUI mainMenuUI;

    private void Awake()
    {
        mainMenuUI = GetComponent<MainMenuUI>();

        if (saveMenuController == null)
        {
            saveMenuController = FindFirstObjectByType<SaveMenuController>(FindObjectsInactive.Include);
        }
    }

    private void OnEnable()
    {
        if (mainMenuUI == null)
        {
            mainMenuUI = GetComponent<MainMenuUI>();
        }

        if (mainMenuUI == null)
        {
            return;
        }

        mainMenuUI.PlayRequested += PlayMostRecentSave;
        mainMenuUI.LoadRequested += OpenLoadMenu;
        mainMenuUI.ExitRequested += ExitGame;
    }

    private void OnDisable()
    {
        if (mainMenuUI == null)
        {
            return;
        }

        mainMenuUI.PlayRequested -= PlayMostRecentSave;
        mainMenuUI.LoadRequested -= OpenLoadMenu;
        mainMenuUI.ExitRequested -= ExitGame;
    }

    private void PlayMostRecentSave()
    {
        if (!SaveGameManager.LoadMostRecentSlot())
        {
            SceneManager.LoadScene(newGameSceneName);
        }
    }

    private void OpenLoadMenu()
    {
        if (saveMenuController == null)
        {
            saveMenuController = FindFirstObjectByType<SaveMenuController>(FindObjectsInactive.Include);
        }

        if (saveMenuController == null)
        {
            return;
        }

        saveMenuController.OpenLoadOnlyMenu();
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
