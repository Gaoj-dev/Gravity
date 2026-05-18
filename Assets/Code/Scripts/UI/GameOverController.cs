using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GameOverUI))]
public class GameOverController : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SaveMenuController saveMenuController;
    [SerializeField] private string startMenuSceneName = "Start Menu";

    private GameOverUI gameOverUI;
    private bool shown;

    private void Awake()
    {
        gameOverUI = GetComponent<GameOverUI>();

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (saveMenuController == null)
        {
            saveMenuController = FindFirstObjectByType<SaveMenuController>(FindObjectsInactive.Include);
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
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDied;
        }

        if (gameOverUI != null)
        {
            gameOverUI.ExitRequested -= ReturnToStartMenu;
            gameOverUI.LoadRequested -= OpenLoadMenu;
        }
    }

    private void HandlePlayerDied()
    {
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
        yield return new WaitForSeconds(waitTime);

        Time.timeScale = 0f;
        gameOverUI?.SetVisible(true);
    }

    private void ReturnToStartMenu()
    {
        Time.timeScale = 1f;
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

        gameOverUI?.SetVisible(false);
        saveMenuController.OpenLoadOnlyMenu(ReopenGameOver);
    }

    private void ReopenGameOver()
    {
        Time.timeScale = 0f;
        gameOverUI?.SetVisible(true);
    }
}
