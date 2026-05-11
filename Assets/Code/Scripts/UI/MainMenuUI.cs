using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuUI : MonoBehaviour
{
    private const float COMPACT_WIDTH_THRESHOLD = 700f;

    [SerializeField] private string titleText = "Gravity Frontier";
    [SerializeField] private UnityEvent onContinuePressed;
    [SerializeField] private UnityEvent onLoadPressed;
    [SerializeField] private UnityEvent onExitPressed;

    private UIDocument uiDocument;
    private VisualElement root;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
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
            return;

        root = uiDocument.rootVisualElement;

        root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

        Label titleLabel = root.Q<Label>("menu-title");
        if (titleLabel != null)
            titleLabel.text = titleText;

        // ✅ Se pasan directamente los métodos como System.Action
        RegisterButton(root.Q<Button>("continue-button"), HandleContinuePressed);
        RegisterButton(root.Q<Button>("load-button"), HandleLoadPressed);
        RegisterButton(root.Q<Button>("exit-button"), HandleExitPressed);

        UpdateResponsiveState(root.resolvedStyle.width);
    }

    // ✅ Cambiado el parámetro de UnityAction a System.Action
    private void RegisterButton(Button button, System.Action callback)
    {
        if (button == null)
            return;

        button.clicked -= callback;
        button.clicked += callback;
    }

    private void HandleContinuePressed()
    {
        onContinuePressed?.Invoke();
        Debug.Log("Continuar pulsado.");
    }

    private void HandleLoadPressed()
    {
        onLoadPressed?.Invoke();
        Debug.Log("Cargar pulsado.");
    }

    private void HandleExitPressed()
    {
        onExitPressed?.Invoke();
#if UNITY_EDITOR
        Debug.Log("Salir pulsado.");
#else
        Application.Quit();
#endif
    }

    private void OnGeometryChanged(GeometryChangedEvent geometryChangedEvent)
    {
        UpdateResponsiveState(geometryChangedEvent.newRect.width);
    }

    private void UpdateResponsiveState(float width)
    {
        if (root == null)
            return;

        root.EnableInClassList("compact", width > 0f && width < COMPACT_WIDTH_THRESHOLD);
    }
}