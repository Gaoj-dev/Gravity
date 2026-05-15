using UnityEngine;

[RequireComponent(typeof(SaveMenuUI))]
public class SaveMenuController : MonoBehaviour
{
    [SerializeField] private bool pauseGameplayWhileOpen = true;

    private SaveMenuUI saveMenuUI;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        saveMenuUI = GetComponent<SaveMenuUI>();
        RefreshSlotView();
        saveMenuUI.SetVisible(false);
    }

    private void OnEnable()
    {
        if (saveMenuUI == null)
        {
            saveMenuUI = GetComponent<SaveMenuUI>();
        }

        saveMenuUI.SaveRequested += HandleSaveRequested;
        saveMenuUI.LoadRequested += HandleLoadRequested;
        saveMenuUI.CloseRequested += CloseMenu;
        SaveGameManager.SlotsChanged += RefreshSlotView;
    }

    private void OnDisable()
    {
        if (saveMenuUI != null)
        {
            saveMenuUI.SaveRequested -= HandleSaveRequested;
            saveMenuUI.LoadRequested -= HandleLoadRequested;
            saveMenuUI.CloseRequested -= CloseMenu;
        }

        SaveGameManager.SlotsChanged -= RefreshSlotView;
        ResumeGameplay();
    }

    public void OpenMenu()
    {
        OpenMenu(SaveMenuUI.SaveMenuMode.SaveAndLoad);
    }

    public void OpenLoadOnlyMenu()
    {
        OpenMenu(SaveMenuUI.SaveMenuMode.LoadOnly);
    }

    public void OpenMenu(SaveMenuUI.SaveMenuMode mode)
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;
        RefreshSlotView();
        saveMenuUI.SetMode(mode);
        saveMenuUI.SetVisible(true);
        PauseGameplay();
    }

    public void CloseMenu()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        saveMenuUI.SetVisible(false);
        ResumeGameplay();
    }

    private void HandleSaveRequested(int slotIndex)
    {
        if (SaveGameManager.SaveToSlot(slotIndex))
        {
            RefreshSlotView();
        }
    }

    private void HandleLoadRequested(int slotIndex)
    {
        ResumeGameplay();
        isOpen = false;
        saveMenuUI.SetVisible(false);
        SaveGameManager.LoadFromSlot(slotIndex);
    }

    private void RefreshSlotView()
    {
        if (saveMenuUI == null)
        {
            return;
        }

        for (int i = 0; i < SaveGameManager.SlotCount; i++)
        {
            SaveGameManager.SaveSlotSnapshot slot = SaveGameManager.GetSlot(i);
            saveMenuUI.SetSlotDisplayData(i, slot.Title, slot.Date, slot.Time);
        }
    }

    private void PauseGameplay()
    {
        if (pauseGameplayWhileOpen)
        {
            Time.timeScale = 0f;
        }
    }

    private void ResumeGameplay()
    {
        if (pauseGameplayWhileOpen)
        {
            Time.timeScale = 1f;
        }
    }
}
