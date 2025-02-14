using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoogleMobileAds.Api;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour, IDataPersistence
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private GameObject canvas;
    private ShopCardShipData[] shipCards;
    private ShopCardGemAndGoldData[] gemAndGoldCards;

    [Header("Popup elements")]
    [SerializeField]
    private Button popUpCloseButton;

    [SerializeField]
    private Button popUpConfirmButton;

    [SerializeField]
    private TextMeshProUGUI popUpText;

    [SerializeField]
    private GameObject popUpObject;

    [Header("Info Popup elements")]
    [SerializeField]
    private Button infoPopUpCloseButton;

    [SerializeField]
    private TextMeshProUGUI infoPopUpTitleText;

    [SerializeField]
    private UDictionary<EquiementsStats, TextMeshProUGUI> infoPopUpStatsText;

    [SerializeField]
    private UDictionary<RarityType, TextMeshProUGUI> infoPopUpRaritiesText;

    [SerializeField]
    private TextMeshProUGUI infoPopUpBulletPerSecondStatText; //TODO: integrer infoPopUpBulletPerSecondStatText au dictionnare

    [SerializeField]
    private Image infoPopUpImage;

    [SerializeField]
    private GameObject infoPopUpRaritiesObject;

    [SerializeField]
    private GameObject infoPopUpSatsObject;

    [SerializeField]
    private GameObject infoPopUpObject;

    [Header("Gems and Gold elements")]
    [SerializeField] private Button goldWithAdButton;

    [Header("Gacha machine elements")]
    [SerializeField]
    private GameObject gachaMachinePanel;

    [SerializeField]
    private Button adButtonForGachaMachine;

    [SerializeField]
    private RandomGachaEquipementData adGachaRandomEquipment;

    [Header("Reward elements")]
    [SerializeField]
    private Button rewardCloseButton;

    [SerializeField]
    private Button rewardEquipButton;

    [SerializeField]
    private Image rewardImage;

    [SerializeField]
    private TextMeshProUGUI rewardText;

    [SerializeField]
    private GameObject rewardObject;
    private EquipementData currentEarnEquipement;

    [Header("Buttons of each part")]
    [SerializeField]
    private UDictionary<Button, GameObject> partsWithButtons;

    [SerializeField] private Color unselectedColor = new Color(88f / 255f, 86f / 255f, 231f / 255f);
    [SerializeField] private Color selectedColor = new Color(0f / 255, 14f / 255, 96f / 255);
    private int currentShopPart = 0;
    private List<Button> partButtonsList;

    private UDictionary<NonConsumableEquipementData, bool> nonConsumableEquipements =
        new();
    private NonConsumableEquipementData currentNonConsumableEquipement;
    private ConsumableEquipementData currentConsumableEquipement;
    private RandomGachaEquipementData currentRandomGachaEquipement;
    private EquipementData[] currentRandomGachaEarnEquipement;

    public string environment = "production";

    private void Awake()
    {
        popUpObject.SetActive(false);
        popUpCloseButton.onClick.AddListener(ClosePopUp);
        popUpConfirmButton.onClick.AddListener(ConfirmPurchase);
        InitializeCards();

        infoPopUpObject.SetActive(false);
        infoPopUpCloseButton.onClick.AddListener(CloseInfoPopUp);

        // Ajoute l'evenement au bouton pour recupere gold ou gem avec pub
        goldWithAdButton.onClick.AddListener(() => AddGoldOrGemButtonClicked(RessourceType.Gold, 50));

        gachaMachinePanel.SetActive(false);
        adButtonForGachaMachine.onClick.AddListener(ShowAdForGacha);

        rewardObject.SetActive(false);
        rewardCloseButton.onClick.AddListener(CloseReward);
        rewardEquipButton.onClick.AddListener(EquipReward);

        partButtonsList = new List<Button>(partsWithButtons.Keys);
        AddListenerForPartButtons();
        ShowShopPart(currentShopPart);
        UpdatePartButtons();
    }

    private void InitializeCards()
    {
        shipCards = GetComponentsInChildren<ShopCardShipData>();
        foreach (ShopCardShipData card in shipCards)
        {
            if (card.data.requireIAP)
                AddIAPButton(card);
            card.gameObject.SetActive(true);
        }
        gemAndGoldCards = GetComponentsInChildren<ShopCardGemAndGoldData>();
        foreach (ShopCardGemAndGoldData card in gemAndGoldCards)
        {
            if (card.data.requireIAP)
                AddIAPButton(card);
            card.gameObject.SetActive(true);
        }
        CheckAlreadyBuyCard();
    }

    #region Part Buttons
    private void AddListenerForPartButtons()
    {
        foreach (var kvp in partsWithButtons)
        {
            Button button = kvp.Key;
            int index = partButtonsList.IndexOf(button);
            button?.onClick.AddListener(() => ShowShopPart(index));
        }
    }

    private void ShowShopPart(int index)
    {
        currentShopPart = index;
        for (int i = 0; i < partsWithButtons.Values.Count; i++)
        {
            partsWithButtons.Values[i].SetActive(i == index);
        }
        UpdatePartButtons();
        ClosePopUp();
    }

    private void UpdatePartButtons()
    {
        for (int i = 0; i < partsWithButtons.Keys.Count; i++)
        {
            Image image = partsWithButtons.Keys[i].gameObject.GetComponent<Image>();
            image.color = (i == currentShopPart) ? selectedColor : unselectedColor;
        }
    }

    #endregion

    private void ClosePopUp()
    {
        popUpObject.SetActive(false);
        currentNonConsumableEquipement = null;
        currentConsumableEquipement = null;
        currentRandomGachaEquipement = null;
    }

    private void ConfirmPurchase()
    {
        // TODO: handle error
        if (currentShopPart == 0)
            ProcessNonConsumableEquipementPayment();
        else if (currentShopPart == 1)
            ProcessConsumableEquipementPayment();
        else if (currentShopPart == 2)
            ProcessRandomEquipementPayment();
        // ProcessNonConsumableEquipementPayment();
        popUpObject.SetActive(false);
    }

    public void OpenPopUp(ConsumableEquipementData data)
    {
        currentConsumableEquipement = data;
        popUpText.text = data.payAmount.ToString() + " " + data.paySubType.ToString();
        popUpObject.SetActive(true);
    }

    public void OpenPopUp(NonConsumableEquipementData data)
    {
        currentNonConsumableEquipement = data;
        popUpText.text = data.amount.ToString() + " " + data.subType.ToString();
        popUpObject.SetActive(true);
    }

    public void OpenPopUp(RandomGachaEquipementData data)
    {
        currentRandomGachaEquipement = data;
        popUpText.text = data.payAmount.ToString() + " " + data.paySubType.ToString();
        popUpObject.SetActive(true);
    }

    private string GetPullingScene(RandomGachaEquipementData gachaEquipementData)
    {
        return gachaEquipementData.pullAmount switch
        {
            PullingType.x1 => "Pulling",
            PullingType.x10 => "PullingX10",
            _ =>
                null // Default case
            ,
        };
    }

    private IEnumerator ShowGachaMachinePanel()
    {
        DataPersistenceManager.instance.SaveGame();
        AsyncOperation loading = SceneManager.LoadSceneAsync(
            GetPullingScene(currentRandomGachaEquipement),
            LoadSceneMode.Additive
        );

        while (!loading.isDone)
        {
            yield return null;
        }

        mainCamera.gameObject.SetActive(false);
        canvas.SetActive(false);

        PullingManager.Instance.lobbyCanvas = canvas;
        PullingManager.Instance.lobbyCamera = mainCamera;
        PullingManager.Instance.currentEarnEquipement = currentRandomGachaEarnEquipement;
    }

    public void OpenReward()
    {
        rewardImage.sprite = currentEarnEquipement.icon;
        rewardText.text = currentEarnEquipement.displayName;
        rewardEquipButton.gameObject.SetActive(true);
        rewardObject.SetActive(true);
    }

    public void OpenReward(string text, Sprite sprite)
    {
        rewardImage.sprite = sprite;
        rewardText.text = text;
        rewardEquipButton.gameObject.SetActive(false);
        rewardObject.SetActive(true);
    }

    private void CloseReward()
    {
        rewardObject.SetActive(false);
        gachaMachinePanel.SetActive(false);
        currentEarnEquipement = null;
        currentRandomGachaEarnEquipement = null;
    }

    private void EquipReward()
    {
        RessourcesManager.Instance.SetEquipedEquipments(
            currentEarnEquipement.slot,
            currentEarnEquipement
        );
        CloseReward();
    }

    #region Info Popup

    private void CloseInfoPopUp()
    {
        infoPopUpObject.SetActive(false);
    }

    public void OpenInfoPopUp(EquipementData currentInfoPopupEquipement)
    {
        infoPopUpSatsObject.SetActive(true);
        infoPopUpRaritiesObject.SetActive(false);
        UpdateStatTexts(currentInfoPopupEquipement);
        infoPopUpImage.sprite = currentInfoPopupEquipement.icon;
        infoPopUpTitleText.text = currentInfoPopupEquipement.displayName;
        infoPopUpObject.SetActive(true);
    }

    public void OpenInfoPopUp(RandomGachaEquipementData currentInfoPopupEquipement)
    {
        infoPopUpSatsObject.SetActive(false);
        infoPopUpRaritiesObject.SetActive(true);
        UpdateRaritiesTexts(currentInfoPopupEquipement);
        infoPopUpImage.sprite = currentInfoPopupEquipement.visualIconInShop;
        infoPopUpTitleText.text = currentInfoPopupEquipement.displayName;
        infoPopUpObject.SetActive(true);
    }

    public void UpdateRaritiesTexts(RandomGachaEquipementData currentInfoPopupEquipement)
    {
        if (currentInfoPopupEquipement == null)
            return;

        infoPopUpRaritiesText[RarityType.Common].text = GetRarity(
            currentInfoPopupEquipement,
            RarityType.Common
        );
        infoPopUpRaritiesText[RarityType.Rare].text = GetRarity(
            currentInfoPopupEquipement,
            RarityType.Rare
        );
        infoPopUpRaritiesText[RarityType.Epic].text = GetRarity(
            currentInfoPopupEquipement,
            RarityType.Epic
        );
        infoPopUpRaritiesText[RarityType.Legendary].text = GetRarity(
            currentInfoPopupEquipement,
            RarityType.Legendary
        );
    }

    public void UpdateStatTexts(EquipementData currentInfoPopupEquipement)
    {
        if (currentInfoPopupEquipement == null)
            return;
        if (currentInfoPopupEquipement.slot == EquipeSlot.Ship)
        {
            infoPopUpStatsText[EquiementsStats.Health].text = GetShipStat(
                currentInfoPopupEquipement,
                EquiementsStats.Health
            );
            infoPopUpStatsText[EquiementsStats.Shield].text = GetShipStat(
                currentInfoPopupEquipement,
                EquiementsStats.Shield
            );
            infoPopUpStatsText[EquiementsStats.AttackDamage].text = GetShipStat(
                currentInfoPopupEquipement,
                EquiementsStats.AttackDamage
            );
            infoPopUpStatsText[EquiementsStats.HealthRegen].text = GetShipStat(
                currentInfoPopupEquipement,
                EquiementsStats.HealthRegen
            );
            infoPopUpStatsText[EquiementsStats.ShieldRegen].text = GetShipStat(
                currentInfoPopupEquipement,
                EquiementsStats.ShieldRegen
            );
            infoPopUpStatsText[EquiementsStats.Speed].text = GetShipStat(
                currentInfoPopupEquipement,
                EquiementsStats.Speed
            );
            infoPopUpBulletPerSecondStatText.text =
                currentInfoPopupEquipement.bulletPerSecond.ToString();
        }
        else
        {
            infoPopUpStatsText[EquiementsStats.Health].text = GetStat(
                currentInfoPopupEquipement,
                EquiementsStats.Health
            );
            infoPopUpStatsText[EquiementsStats.Shield].text = GetStat(
                currentInfoPopupEquipement,
                EquiementsStats.Shield
            );
            infoPopUpStatsText[EquiementsStats.AttackDamage].text = GetStat(
                currentInfoPopupEquipement,
                EquiementsStats.AttackDamage
            );
            infoPopUpStatsText[EquiementsStats.HealthRegen].text = GetStat(
                currentInfoPopupEquipement,
                EquiementsStats.HealthRegen
            );
            infoPopUpStatsText[EquiementsStats.ShieldRegen].text = GetStat(
                currentInfoPopupEquipement,
                EquiementsStats.ShieldRegen
            );
            infoPopUpStatsText[EquiementsStats.Speed].text = GetStat(
                currentInfoPopupEquipement,
                EquiementsStats.Speed
            );
            infoPopUpBulletPerSecondStatText.text = "+0%";
        }
    }

    private string GetShipStat(EquipementData equipement, EquiementsStats statType)
    {
        if (equipement == null || equipement.stats == null || equipement.stats.Count == 0)
            return ""; // Changed to return 1 if the equipement or stats are null/empty

        float value = AttributesData.GetShipStat(equipement, statType);
        return value == 0 ? "" : Mathf.RoundToInt(value).ToString();
    }

    private string GetStat(EquipementData equipement, EquiementsStats statType)
    {
        if (equipement == null || equipement.stats == null || equipement.stats.Count == 0)
            return "+0%"; // Changed to return "0%" if the equipement or stats are null/empty

        float value = AttributesData.GetStat(equipement, statType);
        return value == 0 ? "+0%" : $"{(value >= 0 ? "+" : "−")}{Mathf.Round(Mathf.Abs(value))}%";
    }

    private string GetRarity(RandomGachaEquipementData equipement, RarityType rarityType)
    {
        if (
            equipement == null
            || equipement.rarityPercentages == null
            || equipement.rarityPercentages.Count == 0
        )
            return ""; // Changed to return "0%" if the equipement or stats are null/empty

        if (equipement.rarityPercentages.TryGetValue(rarityType, out float value))
        {
            return $"{value}%";
        }
        return ""; // Changed to return "0%" if the stat is not found
    }

    #endregion

    #region Non Consumable Equipement Purchase

    private void AddIAPButton(ShopCardShipData card)
    {
        CodelessIAPButton button = card.GetComponent<CodelessIAPButton>();
        if (button == null)
        {
            Debug.LogWarning($"CodelessIAPButton doesn't exist on card: {card.name}");
            return;
        }

        if (card.buyButton == null)
        {
            Debug.LogError($"BuyButton not found on card: {card.name}");
            return;
        }

        button.button = card.buyButton;
        button.productId = card.data.productId;

        button.onPurchaseComplete.AddListener(OnNonConsomablePurchaseComplete);
        button.onPurchaseFailed.AddListener(OnNonConsomablePurchaseFailed);

        Debug.Log($"IAP button added to card: {card.name}, Product ID: {button.productId}");
    }

    private void OnNonConsomablePurchaseComplete(Product product)
    {
        Debug.Log($"Purchase completed: {product.definition.id}");
        var card = GetShipCardByProductId(product.definition.id);
        if (card != null)
        {
            ProcessNonConsumableEquipementPaymentWithIAP(card.data);
        }
    }

    private void OnNonConsomablePurchaseFailed(Product product, PurchaseFailureDescription reason)
    {
        Debug.LogWarning($"Purchase failed: {product.definition.id}, reason: {reason}");
    }

    private ShopCardShipData GetShipCardByProductId(string productId)
    {
        foreach (ShopCardShipData card in shipCards)
        {
            if (card.data.productId == productId)
            {
                return card;
            }
        }
        return null;
    }

    private void CheckAlreadyBuyCard()
    {
        List<ShopCardShipData> cardsToDisable = new List<ShopCardShipData>();

        // Create a copy of nonConsumableEquipements to avoid modification during iteration
        var nonConsumableEquipementsCopy = new Dictionary<NonConsumableEquipementData, bool>(nonConsumableEquipements);

        foreach (ShopCardShipData card in shipCards)
        {
            if (
                nonConsumableEquipementsCopy.ContainsKey(card.data)
                && nonConsumableEquipementsCopy[card.data]
            )
            {
                cardsToDisable.Add(card);
            }
        }

        // Process the collected items after the iteration
        foreach (ShopCardShipData card in cardsToDisable)
        {
            card.gameObject.SetActive(false);
        }
    }

    private void ProcessNonConsumableEquipementPayment()
    {
        if (currentNonConsumableEquipement == null)
        {
            Debug.LogWarning("No equipment selected for purchase.");
            return;
        }

        if (
            nonConsumableEquipements.ContainsKey(currentNonConsumableEquipement)
            && nonConsumableEquipements[currentNonConsumableEquipement]
        )
        {
            Debug.LogWarning("This equipment has already been purchased.");
            return;
        }

        RessourcesManager.Instance.Buy(
            currentNonConsumableEquipement.subType,
            currentNonConsumableEquipement.amount,
            () =>
            {
                RessourcesManager.Instance.AddEquipements(currentNonConsumableEquipement.data, 1);
                nonConsumableEquipements[currentNonConsumableEquipement] = true;

                StartCoroutine(WaitForSaving());

                Debug.Log($"Purchased equipment: {currentNonConsumableEquipement?.name}");

                CheckAlreadyBuyCard();
                currentEarnEquipement = currentNonConsumableEquipement.data;
                OpenReward();
            },
            () =>
            {
                Debug.LogError("Error during the process");
            }
        );
    }

    private void ProcessNonConsumableEquipementPaymentWithIAP(
        NonConsumableEquipementData nonConsumableEquipementData
    )
    {
        Debug.Log("Payment validate");
        if (nonConsumableEquipementData == null)
        {
            Debug.LogWarning("No equipment selected for purchase.");
            return;
        }

        if (
            nonConsumableEquipements.ContainsKey(nonConsumableEquipementData)
            && nonConsumableEquipements[nonConsumableEquipementData]
        )
        {
            Debug.LogWarning("This equipment has already been purchased.");
            return;
        }

        nonConsumableEquipements[nonConsumableEquipementData] = true;
        RessourcesManager.Instance.AddEquipements(nonConsumableEquipementData.data, 1);

        // StartCoroutine(WaitForSaving());

        Debug.Log($"Purchased equipment: {nonConsumableEquipementData?.name}");

        CheckAlreadyBuyCard();
        currentEarnEquipement = nonConsumableEquipementData.data;
        Debug.LogError("before open reward");
        OpenReward();
        Debug.LogError("after open reward");
    }
    #endregion

    #region Gems And Gold Purchase
    private void AddIAPButton(ShopCardGemAndGoldData card)
    {
        CodelessIAPButton button = card.GetComponent<CodelessIAPButton>();
        if (button == null)
        {
            Debug.LogWarning($"CodelessIAPButton doesn't exist on card: {card.name}");
            return;
        }

        if (card.buyButton == null)
        {
            Debug.LogError($"BuyButton not found on card: {card.name}");
            return;
        }

        button.button = card.buyButton;
        button.productId = card.data.productId;

        button.onPurchaseComplete.AddListener(OnConsomablePurchaseComplete);
        button.onPurchaseFailed.AddListener(OnConsomablePurchaseFailed);

        Debug.Log($"IAP button added to card: {card.name}, Product ID: {button.productId}");
    }

    private void OnConsomablePurchaseComplete(Product product)
    {
        Debug.Log($"Purchase completed: {product.definition.id}");
        var card = GetGemAndGoldCardByProductId(product.definition.id);
        if (card != null)
        {
            ProcessConsumableEquipementPaymentWithIAP(card.data);
        }
    }

    private void OnConsomablePurchaseFailed(Product product, PurchaseFailureDescription reason)
    {
        Debug.LogWarning($"Purchase failed: {product.definition.id}, reason: {reason}");
    }

    private void ProcessConsumableEquipementPayment()
    {
        if (currentConsumableEquipement == null)
        {
            Debug.LogWarning("No equipment selected for purchase.");
            return;
        }

        RessourcesManager.Instance.Buy(
            currentConsumableEquipement.paySubType,
            currentConsumableEquipement.payAmount,
            () =>
            {
                switch (currentConsumableEquipement.gainSubType)
                {
                    case PaymantSubtype.Gold:
                        RessourcesManager.Instance.AddGold(currentConsumableEquipement.gainAmount);
                        OpenReward(currentConsumableEquipement.gainAmount.ToString(), ItemDatabase.Instance.ressourceSprites[RessourceType.Gold]);
                        break;
                    case PaymantSubtype.Gem:
                        RessourcesManager.Instance.AddGems(currentConsumableEquipement.gainAmount);
                        OpenReward(currentConsumableEquipement.gainAmount.ToString(), ItemDatabase.Instance.ressourceSprites[RessourceType.Gem]);
                        break;
                    default:
                        Debug.LogError("An error occured");
                        break;
                }

                StartCoroutine(WaitForSaving());

                Debug.Log($"Purchased equipment: {currentConsumableEquipement?.name}");
            },
            () =>
            {
                Debug.LogError("Error during the process");
            }
        );
    }

    private ShopCardGemAndGoldData GetGemAndGoldCardByProductId(string productId)
    {
        foreach (ShopCardGemAndGoldData card in gemAndGoldCards)
        {
            if (card.data.productId == productId)
            {
                return card;
            }
        }
        return null;
    }

    private void ProcessConsumableEquipementPaymentWithIAP(
        ConsumableEquipementData consumableEquipementData
    )
    {
        Debug.Log("Payment validate");
        if (consumableEquipementData == null)
        {
            Debug.LogWarning("No equipment selected for purchase.");
            return;
        }

        switch (consumableEquipementData.gainSubType)
        {
            case PaymantSubtype.Gold:
                RessourcesManager.Instance.AddGold(consumableEquipementData.gainAmount);
                OpenReward(consumableEquipementData.gainAmount.ToString(), ItemDatabase.Instance.ressourceSprites[RessourceType.Gold]);
                break;
            case PaymantSubtype.Gem:
                RessourcesManager.Instance.AddGems(consumableEquipementData.gainAmount);
                OpenReward(consumableEquipementData.gainAmount.ToString(), ItemDatabase.Instance.ressourceSprites[RessourceType.Gem]);
                break;
            default:
                Debug.LogError("An error occured");
                break;
        }

        // StartCoroutine(WaitForSaving());

        Debug.Log($"Purchased equipment: {consumableEquipementData?.name}");

        CheckAlreadyBuyCard();
    }

    private void AddGoldOrGemButtonClicked(RessourceType ressource, int amount)
    {
        AdmobManager.Instance.ShowRewardedAd(
            (Reward reward) =>
            {
                switch (ressource)
                {
                    case RessourceType.Gold:
                        RessourcesManager.Instance.AddGold(amount);
                        OpenReward(amount.ToString(), ItemDatabase.Instance.ressourceSprites[RessourceType.Gold]);
                        break;
                    case RessourceType.Gem:
                        RessourcesManager.Instance.AddGems(amount);
                        OpenReward(amount.ToString(), ItemDatabase.Instance.ressourceSprites[RessourceType.Gem]);
                        break;
                }
            }
        );
    }
    #endregion

    #region Random Items

    private void ShowAdForGacha()
    {
        if (RessourcesManager.Instance.watchPerDay.watchAmountForFreePulling >= RessourcesManager.Instance.watchPerDay.maxWatchAmountForFreePulling)
        {
            Debug.LogError("You have reached the maximum number of free pulls for today.");
            return;
        }
        EquipementData[] reward = GetRandomEquipement(adGachaRandomEquipment);
        if (reward == null)
        {
            //TODO: handle error
            Debug.LogError("Error during the process");
            return;
        }

        currentRandomGachaEquipement = adGachaRandomEquipment;
        currentRandomGachaEarnEquipement = reward;

        AdmobManager.Instance.ShowRewardedAd(
            (Reward adMobReward) =>
            {

                UnityMainThreadDispatcher
                    .Instance()
                    .Enqueue(() =>
                    {
                        RessourcesManager.Instance.watchPerDay.watchAmountForFreePulling++;
                        RessourcesManager.Instance.AddEquipements(reward);
                        StartCoroutine(ShowGachaMachinePanel());
                        // StartCoroutine(WaitForSaving());
                    });

                Debug.Log(
                    $"Purchased equipment: {currentRandomGachaEquipement?.name} and got {reward}"
                );
            }
        );
    }

    private void ProcessRandomEquipementPayment()
    {
        if (currentRandomGachaEquipement == null)
        {
            Debug.LogWarning("No key selected for purchase.");
            return;
        }

        EquipementData[] reward = GetRandomEquipement(currentRandomGachaEquipement);

        if (reward == null)
        {
            //TODO: handle error
            Debug.LogError("Error during the process");
            return;
        }

        currentRandomGachaEarnEquipement = reward;

        RessourcesManager.Instance.Buy(
            currentRandomGachaEquipement.paySubType,
            currentRandomGachaEquipement.payAmount,
            () =>
            {
                RessourcesManager.Instance.AddEquipements(reward);
                StartCoroutine(ShowGachaMachinePanel());
                // StartCoroutine(WaitForSaving());

                Debug.Log(
                    $"Purchased equipment: {currentRandomGachaEquipement?.name} and got {reward}"
                );
            },
            () =>
            {
                Debug.LogError("Error during the process");
            }
        );
    }

    private EquipementData[] GetRandomEquipement(
        RandomGachaEquipementData randomGachaEquipementData
    )
    {
        if (randomGachaEquipementData == null)
        {
            Debug.LogWarning("No equipment selected for purchase.");
            return null;
        }

        List<EquipementData> toReturn = new();

        for (int i = 0; i < (int)randomGachaEquipementData.pullAmount; i++)
        {
            // Calculer la somme totale des pourcentages
            float totalPercentage = 0f;
            foreach (var percentage in randomGachaEquipementData.rarityPercentages.Values)
            {
                totalPercentage += percentage;
            }

            // Tirer un nombre aléatoire pour déterminer la rareté
            float randomValue = Random.Range(0, totalPercentage);
            float cumulativePercentage = 0f;

            RarityType selectedRarity = RarityType.Common;

            foreach (var entry in randomGachaEquipementData.rarityPercentages)
            {
                cumulativePercentage += entry.Value;
                if (randomValue < cumulativePercentage)
                {
                    selectedRarity = entry.Key;
                    break;
                }
            }

            // Filtrer les objets par la rareté sélectionnée
            List<EquipementData> filteredItems = ItemDatabase
                .Instance.equipementDataDictionary.Values.ToList()
                .FindAll(item => item.rarity == selectedRarity && item.canBePulled);
            Debug.Log($"Rarity {filteredItems.Count}");
            Debug.Log($"Rarity {Random.Range(0, filteredItems.Count)}");
            // Tirer un objet aléatoirement parmi les objets filtrés
            if (filteredItems.Count > 0)
            {
                int randomIndex = Random.Range(0, filteredItems.Count);
                Debug.Log($"Rarity {filteredItems[randomIndex].displayName}");
                toReturn.Add(filteredItems[randomIndex]);
            }
        }

        return toReturn.ToArray();
    }
    #endregion

    #region Load and Save Data

    public void LoadData(GameData data)
    {
        if (data.nonConsumableEquipements == null || data.nonConsumableEquipements.Count == 0)
        {
            nonConsumableEquipements = new UDictionary<NonConsumableEquipementData, bool>();
        }
        else
        {
            foreach (var item in data.nonConsumableEquipements)
            {
                ItemDatabase.Instance.GetSerializableScriptableObjectById(item.Key, (equipement) =>
                {
                    if (equipement as NonConsumableEquipementData != null)
                    {
                        nonConsumableEquipements[equipement as NonConsumableEquipementData] = item.Value;
                    }
                });
            }
        }

        CheckAlreadyBuyCard();
        Debug.Log("Loaded non-consumable equipments data.");
    }

    public void SaveData(GameData data)
    {
        foreach (var kvp in nonConsumableEquipements)
        {
            data.nonConsumableEquipements[kvp.Key.UNIQUEID] = kvp.Value;
        }
        Debug.Log("Saved non-consumable equipments data.");
    }

    IEnumerator WaitForSaving()
    {
        yield return null;
        DataPersistenceManager.instance.SaveGame();
    }
    #endregion
}
