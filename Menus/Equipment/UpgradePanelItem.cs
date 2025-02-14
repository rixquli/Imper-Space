using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelItem : MonoBehaviour
{
    [SerializeField] private float height;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image[] points;
    public Button upgradeButton;

    private void Awake()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 sizeDelta = rectTransform.sizeDelta;
        sizeDelta.y = height; // Assignez la nouvelle hauteur souhaitée
        rectTransform.sizeDelta = sizeDelta;
    }

    public void Initialize(EquipementsAndAmount equipement)
    {
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => Button_Click(equipement));
        image.sprite = equipement.data.icon;
        levelText.text = equipement.level.ToString();
        SetupPoints(equipement.level);
        SetupButtonPrices(equipement.level);
    }

    private void Button_Click(EquipementsAndAmount equipement)
    {
        if (equipement.level > 3) return;
        RessourcesManager.Instance.UpgradeEquipement(equipement);
    }

    private void SetupPoints(int level)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i].color = i < level ? new Color(points[i].color.r, points[i].color.g, points[i].color.b, 1f) : new Color(points[i].color.r, points[i].color.g, points[i].color.b, 0f);
        }
    }

    private void SetupButtonPrices(int level)
    {
        if (level > 3)
        {
            upgradeButton.GetComponent<CanvasGroup>().alpha = 0f;
            return;
        }
        string amountToPayForUpgrade = RessourcesManager.Instance.costPerLevelToUpgradeEquipment(level + 1).ToString();
        TextMeshProUGUI textMeshPro = upgradeButton.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            textMeshPro.text = amountToPayForUpgrade;
        }

    }
}
