using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuUI : MonoBehaviour
{
    private const float COMPACT_WIDTH_THRESHOLD = 700f;

    private UIDocument uiDocument;
    private VisualElement root;

    public event System.Action PlayRequested;
    public event System.Action LoadRequested;
    public event System.Action ExitRequested;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        UIToolkitPanelBootstrap.EnsureTextSettings(uiDocument);
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

    private void BindUi()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            return;
        }

        root = uiDocument.rootVisualElement;
        root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

        RegisterButton(root.Q<Button>("continue-button"), () => PlayRequested?.Invoke());
        RegisterButton(root.Q<Button>("load-button"), () => LoadRequested?.Invoke());
        RegisterButton(root.Q<Button>("exit-button"), () => ExitRequested?.Invoke());

        UpdateResponsiveState(root.resolvedStyle.width);
    }

    private void RegisterButton(Button button, System.Action callback)
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
