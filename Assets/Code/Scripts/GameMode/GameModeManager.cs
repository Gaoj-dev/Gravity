using UnityEngine.SceneManagement;

public enum GameMode
{
    Space,
    Planet
}

public static class GameModeManager
{
    public static GameMode CurrentMode { get; private set; } = GameMode.Space;

    public static event System.Action<GameMode> ModeChanged;

    public static void SetMode(GameMode mode)
    {
        if (CurrentMode == mode)
        {
            return;
        }

        CurrentMode = mode;
        ModeChanged?.Invoke(CurrentMode);
    }

    public static void LoadSceneForMode(string sceneName, GameMode mode)
    {
        SetMode(mode);
        BeginSceneTransition(sceneName);
        SceneManager.LoadScene(sceneName);
    }

    // Punto de extension para un fade u otra transicion visual.
    public static void BeginSceneTransition(string targetSceneName)
    {
    }
}
