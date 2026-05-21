using System;
using UnityEngine;

[Serializable]
public class SaveFileData
{
    public int version = 1;
    public SaveMetadataData metadata = new SaveMetadataData();
    public WorldSaveData world = new WorldSaveData();
    public PlayerSaveData player = new PlayerSaveData();
}

[Serializable]
public class SaveMetadataData
{
    public string title = "Guardado";
    public string savedAtIsoUtc = string.Empty;
}

[Serializable]
public class WorldSaveData
{
    public string sceneName = string.Empty;
    public string gameMode = nameof(GameMode.Space);
}

[Serializable]
public class PlayerSaveData
{
    public SerializableVector3 position = new SerializableVector3();
    public float rotationZ;
    public SerializableVector2 linearVelocity = new SerializableVector2();
    public float angularVelocity;
    public float currentHealth;
    public int goldCount;
    public PlayerAbilitySaveData abilities = new PlayerAbilitySaveData();
}

[Serializable]
public class PlayerAbilitySaveData
{
    public bool doubleJump;
    public bool dash;
    public bool airDashUpgrade;
    public bool shoot;
}

[Serializable]
public struct SerializableVector2
{
    public float x;
    public float y;

    public SerializableVector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public SerializableVector2(Vector2 value)
    {
        x = value.x;
        y = value.y;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }
}

[Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SerializableVector3(Vector3 value)
    {
        x = value.x;
        y = value.y;
        z = value.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}
