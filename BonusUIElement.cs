using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
[RequireComponent(typeof(CanvasGroup))]
public class BonusUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cooldawnText;
    public PlayerEntity playerEntity;
    [SerializeField] private BonusBehavior.BonusType bonusType;
    private LayoutElement layoutElement;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
        canvasGroup = GetComponent<CanvasGroup>();
        HideUIElement();
    }

    private void Update()
    {
        if (playerEntity == null)
        {
            return;
        }

        if (playerEntity.bonusActiveTimeRemain[bonusType] > 0)
        {
            // Show the UI element
            if (layoutElement.ignoreLayout)
                ShowUIElement();
            cooldawnText.text = playerEntity.bonusActiveTimeRemain[bonusType].ToString("F1");
        }
        else
        {
            // Hide the UI element
            HideUIElement();
        }
    }

    public void SetPlayerEntity(PlayerEntity playerEntity)
    {
        this.playerEntity = playerEntity;
    }

    public void HideUIElement()
    {
        canvasGroup.alpha = 0;
        layoutElement.ignoreLayout = true;
    }

    public void ShowUIElement()
    {
        canvasGroup.alpha = 1;
        layoutElement.ignoreLayout = false;
    }
}
