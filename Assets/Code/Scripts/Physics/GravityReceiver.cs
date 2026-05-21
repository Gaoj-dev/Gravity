using System.Collections.Generic;
using UnityEngine;

public class GravityReceiver : MonoBehaviour
{
    public Vector2 GravityForce { get; private set; }

    private readonly Dictionary<int, Vector2> activeForces = new Dictionary<int, Vector2>();

    private void Update()
    {
        if (GravityForce != Vector2.zero)
        {
            Vector2 dir = GravityForce.normalized;
            float magnitude = GravityForce.magnitude * 0.5f;
            Debug.DrawRay(transform.position, dir * magnitude, Color.cyan);
        }

        if (GravityForce != Vector2.zero)
        {
            float angle = Mathf.Atan2(GravityForce.y, GravityForce.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void AddGravityForce(Vector2 force)
    {
        SetGravityForce(0, force);
    }

    public void SetGravityForce(int sourceId, Vector2 force)
    {
        activeForces[sourceId] = force;
        RecalculateGravityForce();
    }

    public void ClearGravityForce()
    {
        activeForces.Clear();
        GravityForce = Vector2.zero;
    }

    public void RemoveGravityForce(int sourceId)
    {
        if (activeForces.Remove(sourceId))
        {
            RecalculateGravityForce();
        }
    }

    public Vector2 GetGravityDirection()
    {
        return GravityForce.normalized;
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
