using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float interactionRadius = 1.25f;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField] private Transform detectionOrigin;

    private IInteractable currentInteractable;

    private void Awake()
    {
        if (detectionOrigin == null)
        {
            detectionOrigin = transform;
        }
    }

    private void Update()
    {
        RefreshNearbyInteractable();

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            currentInteractable?.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TrySetCurrentInteractable(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TrySetCurrentInteractable(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (currentInteractable != null && IsSameInteractable(other, currentInteractable))
        {
            currentInteractable = null;
        }
    }

    private void RefreshNearbyInteractable()
    {
        // Fallback por overlap para que siga funcionando aunque el trigger no esté configurado.
        Collider2D nearbyCollider = Physics2D.OverlapCircle(detectionOrigin.position, interactionRadius, interactableLayers);

        if (nearbyCollider == null)
        {
            currentInteractable = null;
            return;
        }

        currentInteractable = nearbyCollider.GetComponent<IInteractable>();
    }

    private void TrySetCurrentInteractable(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
        }
    }

    private bool IsSameInteractable(Collider2D other, IInteractable interactable)
    {
        return other.GetComponent<IInteractable>() == interactable;
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = detectionOrigin != null ? detectionOrigin : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin.position, interactionRadius);
    }
}
