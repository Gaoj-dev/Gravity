using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GameOverUI : MonoBehaviour
{
    private const float COMPACT_WIDTH_THRESHOLD = 700f;

    private UIDocument uiDocument;
    private VisualElement root;

    public event Action ExitRequested;
    public event Action LoadRequested;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        UIToolkitPanelBootstrap.EnsureTextSettings(uiDocument);
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

        RegisterButton(root.Q<Button>("gameover-exit-button"), HandleExitButtonClicked);
        RegisterButton(root.Q<Button>("gameover-load-button"), HandleLoadButtonClicked);

        UpdateResponsiveState(root.resolvedStyle.width);
    }

    private void HandleExitButtonClicked() { if (ExitRequested != null) ExitRequested.Invoke(); }

    private void HandleLoadButtonClicked() { if (LoadRequested != null) LoadRequested.Invoke(); }

    private static void RegisterButton(Button button, Action callback)
    {
        if (button == null)
        {
            return;
        }

        button.clicked -= callback;
        button.clicked += callback;
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
