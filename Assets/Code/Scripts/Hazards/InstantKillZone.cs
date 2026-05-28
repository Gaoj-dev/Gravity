using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InstantKillZone : MonoBehaviour
{
    [SerializeField] private string playerLayerName = "Player";

    public static event Action GameOverRequested;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer(playerLayerName))
            return;

        if (GameOverRequested != null) GameOverRequested.Invoke();
    }
}
