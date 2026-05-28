using UnityEngine;
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

    private static Vector3? spaceReturnPosition;

    public static bool HasSpaceReturnPosition => spaceReturnPosition.HasValue;

    public static void StoreSpaceReturnPosition(Vector3 position)
    {
        spaceReturnPosition = position;
    }

    public static void ClearSpaceReturnPosition()
    {
        spaceReturnPosition = null;
    }

    public static bool TryGetSpaceReturnPosition(out Vector3 position)
    {
        if (spaceReturnPosition.HasValue)
        {
            position = spaceReturnPosition.Value;
            return true;
        }
        position = Vector3.zero;
        return false;
    }

    public static bool TryConsumeSpaceReturnPosition(out Vector3 position)
    {
        if (spaceReturnPosition.HasValue)
        {
            position = spaceReturnPosition.Value;
            spaceReturnPosition = null;
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    public static void SetMode(GameMode mode)
    {
        if (CurrentMode == mode)
        {
            return;
        }

        CurrentMode = mode;
        if (ModeChanged != null) ModeChanged.Invoke(CurrentMode);
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
