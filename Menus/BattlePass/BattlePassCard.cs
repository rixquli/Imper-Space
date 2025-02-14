using BattlePass;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePassCard : MonoBehaviour
{
    [SerializeField] private bool isGoldCard = false;

    [SerializeField] private GameObject claimedObject;
    [SerializeField] private CanvasGroup canvasGroup;
    public Button claimButton;

    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private Image windowBackgroundImage;
    [SerializeField] private Image pointImage;
    [SerializeField] private TextMeshProUGUI pointText;
    [SerializeField] private Image barImage;

    [SerializeField] private UIEffectsManager uiEffectsManager;

    [SerializeField] private Color unReachedColorForBar = new(30 / 255f, 30 / 255f, 30 / 255f, 116 / 255f);
    [SerializeField] private Color unReachedColorForPoint = new(16 / 255f, 47 / 255f, 54 / 255f, 116 / 255f);
    [SerializeField] private Color unReachedColorForWindow = new(77 / 255f, 77 / 255f, 77 / 255f, 1f);

    [SerializeField] private Color reachedColorForBar = new(200 / 255f, 200 / 255f, 200 / 255f, 116 / 255f);
    [SerializeField] private Color reachedColorForPoint = new(30 / 255f, 202 / 255f, 211 / 255f, 255 / 255f);
    [SerializeField] private Color reachedColorForWindow = Color.white;

    private bool hasGoldPass;

    public void Initialize(Ressource ressource, bool hasReached, bool hasClaimed, bool hasGoldPass, int number)
    {
        if (ressource == null) return;
        image.sprite = ItemDatabase.Instance.GetRessourceSprite(ressource.ressourceType);
        descriptionText.text = ressource.description;
        if (pointText != null) pointText.text = number.ToString();

        if (hasClaimed) { claimedObject.SetActive(true); canvasGroup.alpha = 0.5f; }
        else { claimedObject.SetActive(false); canvasGroup.alpha = 1f; }

        this.hasGoldPass = hasGoldPass;
        SetupColor(hasReached, hasGoldPass);
    }
    public void Initialize(Equipement equipement, bool hasReached, bool hasClaimed, bool hasGoldPass, int number)
    {
        if (equipement == null) return;

        if (equipement?.equipement?.icon != null)
            image.sprite = equipement.equipement.icon;

        descriptionText.text = equipement.description;
        if (pointText != null) pointText.text = number.ToString();

        if (hasClaimed) { claimedObject.SetActive(true); canvasGroup.alpha = 0.5f; }
        else { claimedObject.SetActive(false); canvasGroup.alpha = 1f; }

        this.hasGoldPass = hasGoldPass;
        SetupColor(hasReached, hasGoldPass);
    }

    private void SetupColor(bool hasReached, bool hasGoldPass)
    {
        if (pointImage != null) pointImage.color = hasReached ? reachedColorForPoint : unReachedColorForPoint;

        bool hasReachedAndGotGoldPassIfThisCardNeedIt = hasReached && (!isGoldCard || hasGoldPass);

        if (windowBackgroundImage != null) windowBackgroundImage.color = hasReachedAndGotGoldPassIfThisCardNeedIt ? reachedColorForWindow : unReachedColorForWindow;
        if (barImage != null) barImage.color = hasReachedAndGotGoldPassIfThisCardNeedIt ? reachedColorForBar : unReachedColorForBar;
    }

    public void PlayClaimEffect()
    {
        claimedObject.SetActive(true);
        uiEffectsManager.Run("Scale01");
        canvasGroup.alpha = 0.5f;
    }
}
