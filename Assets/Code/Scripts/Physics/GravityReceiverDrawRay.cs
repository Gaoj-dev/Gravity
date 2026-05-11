using System.Collections.Generic;
using UnityEngine;

public class GravityReceiverDrawRay : MonoBehaviour
{
    [SerializeField] private float rayLengthMultiplier = 0.5f;
    [SerializeField] private Color rayColor = Color.cyan;

    public Vector2 GravityForce { get; private set; }

    private readonly Dictionary<int, Vector2> activeForces = new Dictionary<int, Vector2>();

    private void Update()
    {
        if (GravityForce == Vector2.zero)
        {
            return;
        }

        Vector2 direction = GravityForce.normalized;
        float magnitude = GravityForce.magnitude * rayLengthMultiplier;
        Debug.DrawRay(transform.position, direction * magnitude, rayColor);
    }

    public void SetGravityForce(int sourceId, Vector2 force)
    {
        activeForces[sourceId] = force;
        RecalculateGravityForce();
    }

    public void RemoveGravityForce(int sourceId)
    {
        if (activeForces.Remove(sourceId))
        {
            RecalculateGravityForce();
        }
    }

    public void ClearGravityForce()
    {
        activeForces.Clear();
        GravityForce = Vector2.zero;
    }

    private void RecalculateGravityForce()
    {
        Vector2 totalForce = Vector2.zero;

        foreach (Vector2 force in activeForces.Values)
        {
            totalForce += force;
        }

        GravityForce = totalForce;
    }
}
