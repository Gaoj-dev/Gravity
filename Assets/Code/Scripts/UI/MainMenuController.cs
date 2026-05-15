using UnityEngine;

[RequireComponent(typeof(MainMenuUI))]
public class MainMenuController : MonoBehaviour
{
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

    public void PlayMostRecentSave()
    {
        bool loaded = SaveGameManager.LoadMostRecentSlot();
        if (!loaded)
        {
            Debug.Log("No hay guardados recientes para cargar.");
        }
    }

    public void OpenLoadMenu()
    {
        if (saveMenuController == null)
        {
            saveMenuController = FindFirstObjectByType<SaveMenuController>(FindObjectsInactive.Include);
        }

        if (saveMenuController == null)
        {
            Debug.LogWarning("No SaveMenuController found for MainMenuController.", this);
            return;
        }

        saveMenuController.OpenLoadOnlyMenu();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Salir pulsado.");
#else
        Application.Quit();
#endif
    }
}
