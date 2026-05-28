using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SaveMenuUI : MonoBehaviour
{
    public enum SaveMenuMode
    {
        SaveAndLoad,
        LoadOnly
    }

    private const float COMPACT_WIDTH_THRESHOLD = 760f;

    [Serializable]
    private class SaveSlotViewData
    {
        public string title = "Guardado 1";
        public string date = "--/--/----";
        public string time = "--:--";
        public UnityEvent onSavePressed;
        public UnityEvent onLoadPressed;
    }

    [SerializeField] private SaveSlotViewData[] slots =
    {
        new SaveSlotViewData { title = "Guardado 1" },
        new SaveSlotViewData { title = "Guardado 2" },
        new SaveSlotViewData { title = "Guardado 3" }
    };

    [SerializeField] private UnityEvent onClosePressed;

    private UIDocument uiDocument;
    private bool eventsRegistered;
    private VisualElement root;
    private SaveMenuMode currentMode = SaveMenuMode.SaveAndLoad;

    public event Action<int> SaveRequested;
    public event Action<int> LoadRequested;
    public event Action CloseRequested;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        UIToolkitPanelBootstrap.EnsureTextSettings(uiDocument);
        EnsureSlotCount();
    }

    private void Start()
    {
        SetVisible(false);
    }

    private void OnEnable()
    {
        BindUi();
    }

    private void OnDisable()
    {
        if (root != null)
        {
            root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
    }

    public void SetSlotTimestamp(int slotIndex, DateTime timestamp)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            return;
        }

        slots[slotIndex].date = timestamp.ToString("dd/MM/yyyy");
        slots[slotIndex].time = timestamp.ToString("HH:mm");
        BindUi();
    }

    public void SetSlotDisplayData(int slotIndex, string title, string date, string time)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            return;
        }

        slots[slotIndex].title = title;
        slots[slotIndex].date = date;
        slots[slotIndex].time = time;
        BindUi();
    }

    public void SetMode(SaveMenuMode mode)
    {
        currentMode = mode;
        BindUi();
    }

    public void SetVisible(bool visible)
    {
        BindUi();

        if (root == null)
        {
            return;
        }

        if (visible) root.style.display = DisplayStyle.Flex;
        else root.style.display = DisplayStyle.None;
        if (visible) root.pickingMode = PickingMode.Position;
        else root.pickingMode = PickingMode.Ignore;
    }

    private void BindUi()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            return;
        }

        root = uiDocument.rootVisualElement;
        root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

        if (!eventsRegistered)
        {
            RegisterButton(root.Q<Button>("close-button"), HandleClosePressed);
        }

        for (int i = 0; i < slots.Length; i++)
        {
            SaveSlotViewData slot = slots[i];
            int slotNumber = i + 1;

            Label titleLabel = root.Q<Label>($"slot-{slotNumber}-title");
            Label dateLabel = root.Q<Label>($"slot-{slotNumber}-date");
            Label timeLabel = root.Q<Label>($"slot-{slotNumber}-time");
            Button saveButton = root.Q<Button>($"slot-{slotNumber}-save-button");
            Button loadButton = root.Q<Button>($"slot-{slotNumber}-load-button");

            if (titleLabel != null)
            {
                titleLabel.text = slot.title;
            }

            if (dateLabel != null)
            {
                dateLabel.text = slot.date;
            }

            if (timeLabel != null)
            {
                timeLabel.text = slot.time;
            }

            if (saveButton != null)
            {
                if (currentMode == SaveMenuMode.SaveAndLoad) saveButton.style.display = DisplayStyle.Flex;
                else saveButton.style.display = DisplayStyle.None;
            }

            if (loadButton != null)
            {
                loadButton.style.display = DisplayStyle.Flex;
            }

            if (!eventsRegistered)
            {
                int capturedIndex = i;
                RegisterButton(saveButton, () => HandleSavePressed(capturedIndex));
                RegisterButton(loadButton, () => HandleLoadPressed(capturedIndex));
            }
        }

        eventsRegistered = true;
        UpdateResponsiveState(root.resolvedStyle.width);
    }

    private void RegisterButton(Button button, Action callback)
    {
        if (button == null)
        {
            return;
        }

        button.clicked -= callback;
        button.clicked += callback;
    }

    private void HandleClosePressed()
    {
        if (onClosePressed != null) onClosePressed.Invoke();
        if (CloseRequested != null) CloseRequested.Invoke();
    }

    private void HandleSavePressed(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex) || currentMode == SaveMenuMode.LoadOnly)
        {
            return;
        }

        if (slots[slotIndex].onSavePressed != null) slots[slotIndex].onSavePressed.Invoke();
        if (SaveRequested != null) SaveRequested.Invoke(slotIndex);
        Debug.Log($"Guardar pulsado en slot {slotIndex + 1}.");
    }

    private void HandleLoadPressed(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            return;
        }

        if (slots[slotIndex].onLoadPressed != null) slots[slotIndex].onLoadPressed.Invoke();
        if (LoadRequested != null) LoadRequested.Invoke(slotIndex);
        Debug.Log($"Cargar pulsado en slot {slotIndex + 1}.");
    }

    private void EnsureSlotCount()
    {
        if (slots != null && slots.Length == 3)
        {
            return;
        }

        SaveSlotViewData[] safeSlots = new SaveSlotViewData[3];
        for (int i = 0; i < safeSlots.Length; i++)
        {
            safeSlots[i] = slots != null && i < slots.Length && slots[i] != null
                ? slots[i]
                : new SaveSlotViewData { title = $"Guardado {i + 1}" };
        }

        slots = safeSlots;
    }

    private bool IsValidSlotIndex(int slotIndex)
    {
        return slots != null && slotIndex >= 0 && slotIndex < slots.Length;
    }

    private void OnGeometryChanged(GeometryChangedEvent geometryChangedEvent)
    {
        UpdateResponsiveState(geometryChangedEvent.newRect.width);
    }

    private void UpdateResponsiveState(float width)
    {
        if (root == null)
        {
            return;
        }

        root.EnableInClassList("compact", width > 0f && width < COMPACT_WIDTH_THRESHOLD);
    }
}
