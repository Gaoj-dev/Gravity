using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GravityReceiver))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerGoldWallet))]
[RequireComponent(typeof(PlayerSpaceController))]
[RequireComponent(typeof(PlayerPlanetController))]
[RequireComponent(typeof(PlayerPlanetAbilities))]
public class PlayerModeHandler : MonoBehaviour
{
    [SerializeField] private bool persistAcrossScenes = true;
    [SerializeField] private Interactor spaceInteractor;
    [SerializeField] private PlayerMeleeAttack planetMeleeAttack;
    [SerializeField] private PlayerPlanetAbilities planetAbilities;
    [SerializeField] private Vector3 defaultSceneSpawnPosition = Vector3.zero;

    private static PlayerModeHandler instance;

    private Rigidbody2D rb;
    private GravityReceiver gravityReceiver;
    private PlayerSpaceController spaceController;
    private PlayerPlanetController planetController;
    private PlayerHealth playerHealth;
    private PlayerGoldWallet playerGoldWallet;
    private PlayerPlanetAbilities playerAbilities;

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
        playerHealth = GetComponent<PlayerHealth>();
        playerGoldWallet = GetComponent<PlayerGoldWallet>();
        playerAbilities = GetComponent<PlayerPlanetAbilities>();

        if (spaceInteractor == null)
        {
            spaceInteractor = GetComponent<Interactor>();
        }

        if (planetMeleeAttack == null)
        {
            planetMeleeAttack = GetComponent<PlayerMeleeAttack>();
        }

        if (planetAbilities == null)
        {
            planetAbilities = GetComponent<PlayerPlanetAbilities>();
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
        MovePlayerToSceneSpawn(scene.name);
        ApplyMode(GameModeManager.CurrentMode);
        ApplyPendingSaveState(scene.name);
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

        if (planetMeleeAttack != null)
        {
            planetMeleeAttack.enabled = !isSpaceMode;
        }

        if (planetAbilities != null)
        {
            planetAbilities.enabled = !isSpaceMode;
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

            if (isSpaceMode)
            {
                rb.gravityScale = 0f;
            }

            if (!isSpaceMode)
            {
                rb.rotation = 0f;
            }
        }
    }

    private void MovePlayerToSceneSpawn(string sceneName)
    {
        Vector3 spawnPosition = defaultSceneSpawnPosition;
        if (SaveGameManager.TryGetPendingPlayerPosition(sceneName, out Vector3 savedPosition))
        {
            spawnPosition = savedPosition;
        }

        transform.position = spawnPosition;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void ApplyPendingSaveState(string sceneName)
    {
        if (!SaveGameManager.TryConsumePendingLoad(sceneName, out SaveFileData saveFileData) || saveFileData?.player == null)
        {
            return;
        }

        PlayerSaveData playerData = saveFileData.player;
        transform.position = playerData.position.ToVector3();
        transform.rotation = Quaternion.Euler(0f, 0f, playerData.rotationZ);

        if (rb != null)
        {
            rb.linearVelocity = playerData.linearVelocity.ToVector2();
            rb.angularVelocity = playerData.angularVelocity;
        }

        playerHealth?.SetCurrentHealth(playerData.currentHealth);
        playerGoldWallet?.SetGoldCount(playerData.goldCount);
        playerAbilities?.ApplySaveData(playerData.abilities);
    }
}
