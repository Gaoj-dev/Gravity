using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveGameManager
{
    public const int SlotCount = 3;

    public readonly struct SaveSlotSnapshot
    {
        public SaveSlotSnapshot(string title, string date, string time, string sceneName, GameMode gameMode, Vector3 playerPosition, bool hasSave)
        {
            Title = title;
            Date = date;
            Time = time;
            SceneName = sceneName;
            GameMode = gameMode;
            PlayerPosition = playerPosition;
            HasSave = hasSave;
        }

        public string Title { get; }
        public string Date { get; }
        public string Time { get; }
        public string SceneName { get; }
        public GameMode GameMode { get; }
        public Vector3 PlayerPosition { get; }
        public bool HasSave { get; }
    }

    private static readonly SaveSlotSnapshot[] Slots =
    {
        new SaveSlotSnapshot("Guardado 1", "--/--/----", "--:--", string.Empty, GameMode.Space, Vector3.zero, false),
        new SaveSlotSnapshot("Guardado 2", "--/--/----", "--:--", string.Empty, GameMode.Space, Vector3.zero, false),
        new SaveSlotSnapshot("Guardado 3", "--/--/----", "--:--", string.Empty, GameMode.Space, Vector3.zero, false)
    };

    private static bool initialized;
    private static string pendingSceneName;
    private static SaveFileData pendingLoadData;

    public static event Action SlotsChanged;

    public static SaveSlotSnapshot GetSlot(int slotIndex)
    {
        EnsureInitialized();
        return IsValidSlotIndex(slotIndex) ? Slots[slotIndex] : default;
    }

    public static bool SaveToSlot(int slotIndex)
    {
        EnsureInitialized();

        if (!IsValidSlotIndex(slotIndex))
        {
            return false;
        }

        PlayerModeHandler player = UnityEngine.Object.FindFirstObjectByType<PlayerModeHandler>();
        if (player == null)
        {
            Debug.LogWarning("No active PlayerModeHandler found. Save cancelled.");
            return false;
        }

        DateTime now = DateTime.Now;
        Scene activeScene = SceneManager.GetActiveScene();
        SaveFileData saveFileData = BuildSaveFile(slotIndex, player, activeScene, now);

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string json = JsonUtility.ToJson(saveFileData, true);
            File.WriteAllText(GetSlotFilePath(slotIndex), json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Save failed for slot {slotIndex + 1}: {exception.Message}");
            return false;
        }

        Slots[slotIndex] = CreateSnapshotFromSaveFile(saveFileData);

        SlotsChanged?.Invoke();
        return true;
    }

    public static bool LoadFromSlot(int slotIndex)
    {
        EnsureInitialized();

        if (!IsValidSlotIndex(slotIndex) || !Slots[slotIndex].HasSave)
        {
            return false;
        }

        SaveFileData saveFileData = LoadSaveFile(slotIndex);
        if (saveFileData == null)
        {
            return false;
        }

        pendingSceneName = saveFileData.world.sceneName;
        pendingLoadData = saveFileData;

        GameModeManager.LoadSceneForMode(saveFileData.world.sceneName, ParseGameMode(saveFileData.world.gameMode));
        return true;
    }

    public static bool TryGetPendingPlayerPosition(string sceneName, out Vector3 playerPosition)
    {
        if (pendingLoadData != null && string.Equals(sceneName, pendingSceneName, StringComparison.Ordinal))
        {
            playerPosition = pendingLoadData.player.position.ToVector3();
            return true;
        }

        playerPosition = Vector3.zero;
        return false;
    }

    // Se consume una sola vez al terminar la carga de escena para aplicar el estado completo.
    public static bool TryConsumePendingLoad(string sceneName, out SaveFileData saveFileData)
    {
        if (pendingLoadData != null && string.Equals(sceneName, pendingSceneName, StringComparison.Ordinal))
        {
            saveFileData = pendingLoadData;
            pendingLoadData = null;
            pendingSceneName = string.Empty;
            return true;
        }

        saveFileData = null;
        return false;
    }

    private static void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        for (int i = 0; i < SlotCount; i++)
        {
            SaveFileData saveFileData = LoadSaveFile(i);
            if (saveFileData != null)
            {
                Slots[i] = CreateSnapshotFromSaveFile(saveFileData);
            }
        }

        initialized = true;
    }

    private static SaveFileData BuildSaveFile(int slotIndex, PlayerModeHandler player, Scene activeScene, DateTime now)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        PlayerGoldWallet playerGoldWallet = player.GetComponent<PlayerGoldWallet>();
        PlayerPlanetAbilities playerAbilities = player.GetComponent<PlayerPlanetAbilities>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        return new SaveFileData
        {
            version = 1,
            metadata = new SaveMetadataData
            {
                title = $"Guardado {slotIndex + 1}",
                savedAtIsoUtc = now.ToUniversalTime().ToString("O")
            },
            world = new WorldSaveData
            {
                sceneName = activeScene.name,
                gameMode = GameModeManager.CurrentMode.ToString()
            },
            player = new PlayerSaveData
            {
                position = new SerializableVector3(player.transform.position),
                rotationZ = player.transform.eulerAngles.z,
                linearVelocity = new SerializableVector2(rb != null ? rb.linearVelocity : Vector2.zero),
                angularVelocity = rb != null ? rb.angularVelocity : 0f,
                currentHealth = playerHealth != null ? playerHealth.CurrentHealth : 0f,
                goldCount = playerGoldWallet != null ? playerGoldWallet.GoldCount : 0,
                abilities = playerAbilities != null ? playerAbilities.CaptureSaveData() : new PlayerAbilitySaveData()
            }
        };
    }

    private static SaveSlotSnapshot CreateSnapshotFromSaveFile(SaveFileData saveFileData)
    {
        DateTime saveTime = ParseSavedTime(saveFileData.metadata.savedAtIsoUtc);
        return new SaveSlotSnapshot(
            string.IsNullOrWhiteSpace(saveFileData.metadata.title) ? "Guardado" : saveFileData.metadata.title,
            saveTime == default ? "--/--/----" : saveTime.ToLocalTime().ToString("dd/MM/yyyy"),
            saveTime == default ? "--:--" : saveTime.ToLocalTime().ToString("HH:mm"),
            saveFileData.world.sceneName,
            ParseGameMode(saveFileData.world.gameMode),
            saveFileData.player.position.ToVector3(),
            true
        );
    }

    private static SaveFileData LoadSaveFile(int slotIndex)
    {
        try
        {
            string path = GetSlotFilePath(slotIndex);
            if (!File.Exists(path))
            {
                return null;
            }

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            SaveFileData saveFileData = JsonUtility.FromJson<SaveFileData>(json);
            EnsureValidSaveFile(saveFileData);
            return saveFileData;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not load save slot {slotIndex + 1}: {exception.Message}");
            return null;
        }
    }

    private static string GetSlotFilePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex + 1}.json");
    }

    private static GameMode ParseGameMode(string gameMode)
    {
        return Enum.TryParse(gameMode, true, out GameMode parsedMode) ? parsedMode : GameMode.Space;
    }

    private static DateTime ParseSavedTime(string isoUtc)
    {
        if (DateTime.TryParse(isoUtc, null, DateTimeStyles.RoundtripKind, out DateTime parsedTime))
        {
            return parsedTime;
        }

        return default;
    }

    private static bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < SlotCount;
    }

    private static void EnsureValidSaveFile(SaveFileData saveFileData)
    {
        if (saveFileData == null)
        {
            return;
        }

        saveFileData.metadata ??= new SaveMetadataData();
        saveFileData.world ??= new WorldSaveData();
        saveFileData.player ??= new PlayerSaveData();
        saveFileData.player.abilities ??= new PlayerAbilitySaveData();
    }
}
