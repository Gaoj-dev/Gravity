using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [Serializable]
    private class InteractionBinding
    {
        public string interactionId = "Enter";
        public Key key = Key.E;
        public float radius = 1.25f;
        public LayerMask interactableLayers = ~0;
        public Color gizmoColor = Color.yellow;
    }

    [Header("Detection")]
    [SerializeField] private Transform detectionOrigin;
    [SerializeField] private InteractionBinding[] interactionBindings =
    {
        new InteractionBinding()
    };

    private readonly HashSet<Collider2D> triggerContacts = new HashSet<Collider2D>();

    private void Awake()
    {
        if (detectionOrigin == null)
        {
            detectionOrigin = transform;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        foreach (InteractionBinding binding in interactionBindings)
        {
            if (!Keyboard.current[binding.key].wasPressedThisFrame)
            {
                continue;
            }

            Debug.Log($"Tecla pulsada: {binding.key} | Binding: {binding.interactionId}");
            IInteractable interactable = FindInteractableForBinding(binding);
            if (interactable != null)
            {
                Debug.Log("Interaction detected");
                interactable.Interact();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        triggerContacts.Add(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        triggerContacts.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        triggerContacts.Remove(other);
    }

    private IInteractable FindInteractableForBinding(InteractionBinding binding)
    {
        Vector2 origin = detectionOrigin.position;

        IInteractable interactableFromTrigger = FindInteractableFromTriggers(binding, origin);
        if (interactableFromTrigger != null)
        {
            return interactableFromTrigger;
        }

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(origin, binding.radius, binding.interactableLayers);
        foreach (Collider2D nearbyCollider in nearbyColliders)
        {
            IInteractable interactable = ResolveInteractable(nearbyCollider);
            if (interactable != null && MatchesInteraction(binding, interactable))
            {
                return interactable;
            }
        }

        return null;
    }

    // Reutiliza los contactos del trigger, pero sigue validando el radio del tipo de interaccion.
    private IInteractable FindInteractableFromTriggers(InteractionBinding binding, Vector2 origin)
    {
        List<Collider2D> invalidContacts = null;
        float maxDistanceSqr = binding.radius * binding.radius;

        foreach (Collider2D triggerContact in triggerContacts)
        {
            if (triggerContact == null)
            {
                if (invalidContacts == null)
                {
                    invalidContacts = new List<Collider2D>();
                }
                invalidContacts.Add(triggerContact);
                continue;
            }

            IInteractable interactable = ResolveInteractable(triggerContact);
            if (interactable == null || !MatchesInteraction(binding, interactable))
            {
                continue;
            }

            if (GetDistanceSqr(origin, triggerContact) <= maxDistanceSqr)
            {
                return interactable;
            }
        }

        if (invalidContacts != null)
        {
            foreach (Collider2D invalidContact in invalidContacts)
            {
                triggerContacts.Remove(invalidContact);
            }
        }

        return null;
    }

    private bool MatchesInteraction(InteractionBinding binding, IInteractable interactable)
    {
        return string.Equals(interactable.InteractionId, binding.interactionId, StringComparison.OrdinalIgnoreCase);
    }

    // Soporta interactuables en el propio collider, en su rigidbody o en padres.
    private IInteractable ResolveInteractable(Collider2D targetCollider)
    {
        if (targetCollider == null)
        {
            return null;
        }

        IInteractable interactable = FindInteractable(targetCollider.GetComponents<MonoBehaviour>());
        if (interactable != null)
        {
            return interactable;
        }

        if (targetCollider.attachedRigidbody != null)
        {
            interactable = FindInteractable(targetCollider.attachedRigidbody.GetComponents<MonoBehaviour>());
            if (interactable != null)
            {
                return interactable;
            }
        }

        return FindInteractable(targetCollider.GetComponentsInParent<MonoBehaviour>());
    }

    private IInteractable FindInteractable(MonoBehaviour[] behaviours)
    {
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IInteractable interactable)
            {
                return interactable;
            }
        }

        return null;
    }

    private float GetDistanceSqr(Vector2 origin, Collider2D targetCollider)
    {
        Vector2 closestPoint = targetCollider.ClosestPoint(origin);
        return (closestPoint - origin).sqrMagnitude;
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = detectionOrigin != null ? detectionOrigin : transform;
        if (interactionBindings == null)
        {
            return;
        }

        foreach (InteractionBinding binding in interactionBindings)
        {
            Gizmos.color = binding.gizmoColor;
            Gizmos.DrawWireSphere(origin.position, binding.radius);
        }
    }
}
