using UnityEngine;

public class SavePointInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionId = "Save";
    [SerializeField] private SaveMenuController saveMenuController;

    public string InteractionId => interactionId;

    private void Awake()
    {
        if (saveMenuController == null)
        {
            saveMenuController = FindFirstObjectByType<SaveMenuController>();
        }
    }

    public void Interact()
    {
        Debug.Log("Save interact");
        if (saveMenuController == null)
        {
            saveMenuController = FindFirstObjectByType<SaveMenuController>();
        }

        if (saveMenuController == null)
        {
            Debug.LogWarning("No SaveMenuController found for SavePointInteractable.", this);
            return;
        }

        saveMenuController.OpenMenu();
    }
}
