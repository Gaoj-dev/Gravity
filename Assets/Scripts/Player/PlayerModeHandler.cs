using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GravityReceiver))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerSpaceController))]
[RequireComponent(typeof(PlayerPlanetController))]
public class PlayerModeHandler : MonoBehaviour
{
    [SerializeField] private bool persistAcrossScenes = true;
    [SerializeField] private Interactor spaceInteractor;
    [SerializeField] private PlayerMeleeAttack planetMeleeAttack;
    [SerializeField] private Vector3 defaultSceneSpawnPosition = Vector3.zero;

    private static PlayerModeHandler instance;

    private Rigidbody2D rb;
    private GravityReceiver gravityReceiver;
    private PlayerSpaceController spaceController;
    private PlayerPlanetController planetController;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        rb = GetComponent<Rigidbody2D>();
        gravityReceiver = GetComponent<GravityReceiver>();
        spaceController = GetComponent<PlayerSpaceController>();
        planetController = GetComponent<PlayerPlanetController>();

        if (spaceInteractor == null)
        {
            spaceInteractor = GetComponent<Interactor>();
        }

        if (planetMeleeAttack == null)
        {
            planetMeleeAttack = GetComponent<PlayerMeleeAttack>();
        }

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        ApplyMode(GameModeManager.CurrentMode);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameModeManager.ModeChanged += ApplyMode;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameModeManager.ModeChanged -= ApplyMode;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        MovePlayerToSceneSpawn();
        ApplyMode(GameModeManager.CurrentMode);
    }

    private void ApplyMode(GameMode mode)
    {
        bool isSpaceMode = mode == GameMode.Space;

        if (spaceController != null)
        {
            spaceController.enabled = isSpaceMode;
        }

        if (planetController != null)
        {
            planetController.enabled = !isSpaceMode;
        }

        if (spaceInteractor != null)
        {
            spaceInteractor.enabled = isSpaceMode;
        }

        if (planetMeleeAttack != null)
        {
            planetMeleeAttack.enabled = !isSpaceMode;
        }

        if (gravityReceiver != null)
        {
            gravityReceiver.enabled = isSpaceMode;

            if (!isSpaceMode)
            {
                gravityReceiver.ClearGravityForce();
            }
        }

        if (rb != null)
        {
            rb.angularVelocity = 0f;

            if (!isSpaceMode)
            {
                rb.rotation = 0f;
            }
        }
    }

    private void MovePlayerToSceneSpawn()
    {
        transform.position = defaultSceneSpawnPosition;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
