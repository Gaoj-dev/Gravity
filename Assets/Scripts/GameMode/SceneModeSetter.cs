using UnityEngine;

public class SceneModeSetter : MonoBehaviour
{
    [SerializeField] private GameMode sceneMode = GameMode.Space;

    private void Awake()
    {
        GameModeManager.SetMode(sceneMode);
    }
}
