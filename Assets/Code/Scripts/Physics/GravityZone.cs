using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class GravityZone : MonoBehaviour
{
    [SerializeField] private float attractionForce = 5f; // Fuerza máxima en el centro

    private float maxDistance;

    private const float MIN_DISTANCE = 0.01f;

    private void Awake()
    {
        InitializeGravityRange();
    }

    private void InitializeGravityRange()
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();

        if (collider == null)
        {
            Debug.LogError($"{nameof(GravityZone)} requiere un CircleCollider2D en el mismo GameObject.");
            enabled = false;
            return;
        }

        float worldScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        maxDistance = collider.radius * worldScale;
    }


    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.attachedRigidbody)
        {
            return;
        }

        Rigidbody2D rb = other.attachedRigidbody;
        Vector2 force = CalculateGravityForce(rb.position);

        if (force == Vector2.zero)
        {
            return;
        }

        rb.AddForce(force, ForceMode2D.Force);

        GravityReceiver receiver = other.GetComponent<GravityReceiver>();
        if (receiver != null)
        {
            receiver.SetGravityForce(GetInstanceID(), force);
            return;
        }

        GravityReceiverDrawRay drawRayReceiver = other.GetComponent<GravityReceiverDrawRay>();
        if (drawRayReceiver != null)
        {
            drawRayReceiver.SetGravityForce(GetInstanceID(), force);
            return;
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GravityReceiver receiver = other.GetComponent<GravityReceiver>();
        if (receiver != null)
        {
            receiver.RemoveGravityForce(GetInstanceID());
            return;
        }

        GravityReceiverDrawRay drawRayReceiver = other.GetComponent<GravityReceiverDrawRay>();
        if (drawRayReceiver != null)
        {
            drawRayReceiver.RemoveGravityForce(GetInstanceID());
            return;
        }

    }

    private Vector2 CalculateGravityForce(Vector2 bodyPosition)
    {
        Vector2 direction = (Vector2)transform.position - bodyPosition;
        float distance = direction.magnitude;

        if (distance < MIN_DISTANCE || distance >= maxDistance)
            return Vector2.zero;

        float normalizedDistance = 1f - (distance / maxDistance);
        float scaledForce = attractionForce * Mathf.Clamp01(normalizedDistance);

        return direction.normalized * scaledForce;
    }
}
