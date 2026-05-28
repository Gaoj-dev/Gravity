using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GameOverUI))]
public class GameOverController : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SaveMenuController saveMenuController;
    [SerializeField] private string startMenuSceneName = "StartMenu";
    [SerializeField] private float extraDelayAfterDeath = 0.5f;

    private GameOverUI gameOverUI;
    private bool shown;

    private void Awake()
    {
        shown = false;
        Time.timeScale = 1f;

        gameOverUI = GetComponent<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.SetVisible(false);
        }
    }

    private void Start()
    {
        if (PlayerModeHandler.Instance != null)
        {
            playerHealth = PlayerModeHandler.Instance.GetComponent<PlayerHealth>();
        }
        else if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (saveMenuController == null)
        {
            saveMenuController = FindFirstObjectByType<SaveMenuController>(FindObjectsInactive.Include);
        }

        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDied;
            playerHealth.Died += HandlePlayerDied;
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died += HandlePlayerDied;
        }

        if (gameOverUI != null)
        {
            gameOverUI.ExitRequested += ReturnToStartMenu;
            gameOverUI.LoadRequested += OpenLoadMenu;
        }

        InstantKillZone.GameOverRequested += HandlePlayerDied;
    }

    private void OnDisable()
    {
        Debug.Log("[GameOver] OnDisable llamado");
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDied;
        }

        if (gameOverUI != null)
        {
            gameOverUI.ExitRequested -= ReturnToStartMenu;
            gameOverUI.LoadRequested -= OpenLoadMenu;
        }

        InstantKillZone.GameOverRequested -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        Debug.Log($"[GameOver] HandlePlayerDied llamado. shown={shown}");
        if (shown)
        {
            return;
        }

        StartCoroutine(ShowAfterDeathAnimation());
    }

    private IEnumerator ShowAfterDeathAnimation()
    {
        shown = true;

        float waitTime = playerHealth != null ? Mathf.Max(0f, playerHealth.DeathAnimationDuration) : 0f;
        waitTime += extraDelayAfterDeath;
        Debug.Log($"[GameOver] Coroutine iniciado. Esperando {waitTime}s. gameOverUI={gameOverUI}");
        yield return new WaitForSecondsRealtime(waitTime);

        Debug.Log($"[GameOver] Delay terminado. Mostrando UI. gameOverUI={gameOverUI}");
        Time.timeScale = 0f;
        if (gameOverUI != null) gameOverUI.SetVisible(true);
    }

    private void ReturnToStartMenu()
    {
        Time.timeScale = 1f;
        GameModeManager.ClearSpaceReturnPosition();
        GameModeManager.SetMode(GameMode.Space);
        if (PlayerModeHandler.Instance != null)
            Destroy(PlayerModeHandler.Instance.gameObject);
        SceneManager.LoadScene(startMenuSceneName);
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

        if (gameOverUI != null) gameOverUI.SetVisible(false);
        saveMenuController.OpenLoadOnlyMenu(ReopenGameOver);
    }

    private void ReopenGameOver()
    {
        Time.timeScale = 0f;
        if (gameOverUI != null) gameOverUI.SetVisible(true);
    }
}
