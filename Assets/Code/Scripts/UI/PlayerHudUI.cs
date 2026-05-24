using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PlayerHudUI : MonoBehaviour
{
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;

    private UIDocument uiDocument;
    private PlayerHealth playerHealth;
    private PlayerGoldWallet goldWallet;
    private VisualElement healthContainer;
    private Label goldLabel;
    private int cachedMax = -1;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        if (PlayerModeHandler.Instance != null)
        {
            playerHealth = PlayerModeHandler.Instance.GetComponent<PlayerHealth>();
            goldWallet = PlayerModeHandler.Instance.GetComponent<PlayerGoldWallet>();
        }

        if (playerHealth != null)
        {
            playerHealth.Damaged += RefreshHealth;
            playerHealth.Died += RefreshHealth;
            RefreshHealth();
        }

        if (goldWallet != null)
        {
            goldWallet.GoldChanged += RefreshGold;
            RefreshGold();
        }
    }

    private void OnEnable()
    {
        if (uiDocument.rootVisualElement == null)
            return;

        healthContainer = uiDocument.rootVisualElement.Q<VisualElement>("health-container");
        goldLabel = uiDocument.rootVisualElement.Q<Label>("gold-label");
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Damaged -= RefreshHealth;
            playerHealth.Died -= RefreshHealth;
        }

        if (goldWallet != null)
        {
            goldWallet.GoldChanged -= RefreshGold;
        }
    }

    private void RefreshHealth()
    {
        if (playerHealth != null)
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void RefreshGold()
    {
        if (goldLabel != null && goldWallet != null)
            goldLabel.text = $"Oro: {goldWallet.GoldCount}";
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthContainer == null)
            return;

        if (max != cachedMax)
        {
            healthContainer.Clear();
            for (int i = 0; i < max; i++)
            {
                var heart = new VisualElement();
                heart.AddToClassList("heart");
                healthContainer.Add(heart);
            }
            cachedMax = max;
        }

        for (int i = 0; i < healthContainer.childCount; i++)
        {
            VisualElement heart = healthContainer[i];
            heart.style.backgroundImage = new StyleBackground(i < current ? heartFull : heartEmpty);
            heart.EnableInClassList("heart--empty", i >= current);
        }
    }
}
