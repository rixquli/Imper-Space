using System;
using BattlePass;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class DailyRewardCard : MonoBehaviour
{
    [SerializeField] private GameObject claimedObject;

    [SerializeField] private CanvasGroup canvasGroup;
    public Button claimButton;

    [SerializeField] private Image image;

    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private TextMeshProUGUI dayNumberText;

    [SerializeField] private Image windowBackgroundImage;

    [SerializeField] private Color unReachedColorForBar = new(30 / 255f, 30 / 255f, 30 / 255f, 116 / 255f);

    [SerializeField] private Color unReachedColorForPoint = new(16 / 255f, 47 / 255f, 54 / 255f, 116 / 255f);

    [SerializeField] private Color unReachedColorForWindow = new(77 / 255f, 77 / 255f, 77 / 255f, 1f);

    [SerializeField] private Color reachedColorForBar = new(200 / 255f, 200 / 255f, 200 / 255f, 116 / 255f);

    [SerializeField] private Color reachedColorForPoint = new(30 / 255f, 202 / 255f, 211 / 255f, 255 / 255f);

    [SerializeField] private Color reachedColorForWindow = Color.white;

    [SerializeField] private UIEffectsManager uiEffectsManager;

    [SerializeField] private LocalizedString localizedString;
    private int dayNumber = 0;

    private void OnEnable()
    {
        localizedString.Arguments = new object[] { dayNumber };
        localizedString.StringChanged += UpdateText;
    }

    private void OnDisable()
    {
        localizedString.StringChanged -= UpdateText;
    }

    public void Initialize(Ressource ressource, bool hasReached, bool hasClaimed, int dayNumber)
    {
        if (ressource == null)
            return;
        image.sprite = image.sprite = ItemDatabase.Instance.GetRessourceSprite(ressource.ressourceType);
        descriptionText.text = ressource.description;
        if (dayNumberText != null)
        {
            this.dayNumber = dayNumber;
            localizedString.Arguments[0] = dayNumber;
            localizedString.RefreshString();
        }

        SetClaimed(hasClaimed);

        SetupColor(hasReached);
    }

    public void Initialize(Equipement equipement, bool hasReached, bool hasClaimed, int dayNumber)
    {
        if (equipement == null)
            return;

        if (equipement.equipement != null && equipement.equipement?.icon != null)
            image.sprite = equipement.equipement.icon;

        descriptionText.text = equipement.description;
        if (dayNumberText != null)
        {
            this.dayNumber = dayNumber;
            localizedString.Arguments[0] = dayNumber;
            localizedString.RefreshString();
        }

        if (hasClaimed)
        {
            claimedObject.SetActive(true);
            canvasGroup.alpha = 0.5f;
        }
        else
        {
            claimedObject.SetActive(false);
            canvasGroup.alpha = 1f;
        }

        SetupColor(hasReached);
    }

    private void UpdateText(string value)
    {
        dayNumberText.text = value;
    }

    private void SetupColor(bool hasReached)
    {
        bool hasReachedAndGotGoldPassIfThisCardNeedIt = hasReached;

        if (windowBackgroundImage != null)
            windowBackgroundImage.color = hasReachedAndGotGoldPassIfThisCardNeedIt
                ? reachedColorForWindow
                : unReachedColorForWindow;
    }

    public void SetClaimed(bool isClaimed)
    {
        if (isClaimed)
        {
            claimedObject.SetActive(true);
            canvasGroup.alpha = 0.5f;
        }
        else
        {
            claimedObject.SetActive(false);
            canvasGroup.alpha = 1f;
        }
    }

    public void StartClaimAnim()
    {
        uiEffectsManager.Run("Scale01");
    }
}
