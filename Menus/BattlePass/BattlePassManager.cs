using System.Collections.Generic;
using BattlePass;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;

public class BattlePassManager : MonoBehaviour, IDataPersistence
{
    public static BattlePassManager Instance;

    [Header("Prefab of cards")]
    [SerializeField]
    private GameObject goldCardPrefab;

    [SerializeField]
    private GameObject freeCardPrefab;

    [SerializeField]
    private GameObject emptyCardPrefab;

    [Header("Parent of cards")]
    [SerializeField]
    private RectTransform contentParent;

    [SerializeField]
    private Transform goldCardParent;

    [SerializeField]
    private Transform freeCardParent;

    [SerializeField]
    private Image progressBar;

    [SerializeField]
    private Button buyGoldPassButton;

    [Header("Popup")]
    [SerializeField]
    private PopupController popupController;

    public static float xpPerBattlePassLevel
    {
        get
        {
            // Calcul de l'expérience nécessaire pour chaque niveau exponentiel
            return 100 * Mathf.Pow(1.1f, BattlePassLevel);
        }
    }

    public List<BattlePassLevels> battlePassLevels { get; private set; } = new();
    public static int BattlePassLevel
    {
        get
        {
            return Mathf.FloorToInt((Mathf.Log(battlePassXp) - Mathf.Log(100f)) / Mathf.Log(1.1f));
        }
    }
    public static float battlePassXp
    {
        get
        {
            return DataPersistenceManager.instance.gameData.battlePassXp;
        }
    }
    private float progressBarPadding = 0;
    private float progressBarSpaceBetweenCells = 0;
    private float progressBarCellsSize = 0;
    private bool hasGoldPass = false;

    public static float CurrentBattlePassXpOnCurrentLevel
    {
        get
        {
            return battlePassXp - GetXpForLevel(BattlePassLevel);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        AddIAPButton();
    }

    private void Start()
    {
        RessourcesManager.Instance.OnValueChanged += Refresh;
        LoadAndCheckData();
    }

    private void Update()
    {
        UpdateProgressBar();
    }

    private void OnDestroy()
    {
        RessourcesManager.Instance.OnValueChanged -= Refresh;
    }

    public static int GetXpForLevel(int level)
    {
        if (level <= 1)
        {
            return Mathf.FloorToInt(100 * 1.1f);
        }
        return Mathf.FloorToInt(100 * Mathf.Pow(1.1f, GetXpForLevel(level - 1)));
    }

    public int GetXpToNextLevel(int level)
    {
        return Mathf.FloorToInt(100 * Mathf.Pow(1.1f, level + 1));
    }

    private void UpdateCardList()
    {
        DeleteChildsObject(goldCardParent);
        DeleteChildsObject(freeCardParent);
        if (battlePassLevels == null || battlePassLevels.Count <= 0)
            return;

        for (int i = 1; i < battlePassLevels.Count + 1; i++)
        {
            BattlePassLevels currentBattlePassLevels = battlePassLevels.Find(l =>
                l.data != null && l.data != null && l.data.level == i
            );

            if (currentBattlePassLevels == null)
                continue;

            if (HasReachThisLevel(i))
            {
                currentBattlePassLevels.hasReached = true;
            }

            if (currentBattlePassLevels.data.goldPassRewardType != BattlePassRewardType.None)
            {
                GameObject cardObject = Instantiate(goldCardPrefab, goldCardParent);

                if (cardObject.TryGetComponent<BattlePassCard>(out var battlePassCard))
                {
                    if (
                        currentBattlePassLevels.data.goldPassRewardType
                        == BattlePassRewardType.Ressource
                    )
                        battlePassCard.Initialize(
                            currentBattlePassLevels.data.goldPassRessource,
                            currentBattlePassLevels.hasReached,
                            currentBattlePassLevels.hasClaimedGoldCard,
                            hasGoldPass,
                            i
                        );
                    else if (
                        currentBattlePassLevels.data.goldPassRewardType
                        == BattlePassRewardType.Equipement
                    )
                        battlePassCard.Initialize(
                            currentBattlePassLevels.data.goldPassEquipement,
                            currentBattlePassLevels.hasReached,
                            currentBattlePassLevels.hasClaimedGoldCard,
                            hasGoldPass,
                            i
                        );
                    battlePassCard.claimButton.onClick.RemoveAllListeners();
                    battlePassCard.claimButton.onClick.AddListener(
                        () => HandleClaimButtonClick(currentBattlePassLevels, true, battlePassCard)
                    );
                }
            }
            else
            {
                Instantiate(emptyCardPrefab, goldCardParent);
            }

            if (currentBattlePassLevels.data.freePassRewardType != BattlePassRewardType.None)
            {
                GameObject cardObject = Instantiate(freeCardPrefab, freeCardParent);

                if (cardObject.TryGetComponent<BattlePassCard>(out var battlePassCard))
                {
                    if (
                        currentBattlePassLevels.data.freePassRewardType
                        == BattlePassRewardType.Ressource
                    )
                        battlePassCard.Initialize(
                            currentBattlePassLevels.data.freePassRessource,
                            currentBattlePassLevels.hasReached,
                            currentBattlePassLevels.hasClaimedFreeCard,
                            hasGoldPass,
                            i
                        );
                    else if (
                        currentBattlePassLevels.data.freePassRewardType
                        == BattlePassRewardType.Equipement
                    )
                        battlePassCard.Initialize(
                            currentBattlePassLevels.data.freePassEquipement,
                            currentBattlePassLevels.hasReached,
                            currentBattlePassLevels.hasClaimedFreeCard,
                            hasGoldPass,
                             i
                        );
                    battlePassCard.claimButton.onClick.RemoveAllListeners();
                    battlePassCard.claimButton.onClick.AddListener(
                        () => HandleClaimButtonClick(currentBattlePassLevels, false, battlePassCard)
                    );
                }
            }
            else
            {
                Instantiate(emptyCardPrefab, freeCardParent);
            }
        }
    }

    private void HandleClaimButtonClick(BattlePassLevels level, bool isGoldCard, BattlePassCard battlePassCard)
    {
        // Vérifiez si le niveau a été atteint et non réclamé
        if (
            (isGoldCard && (!level.hasReached || level.hasClaimedGoldCard || !hasGoldPass))
            || (!isGoldCard && (!level.hasReached || level.hasClaimedFreeCard))
        )
        {
            Debug.LogWarning("Level not eligible for claiming or already claimed.");
            return;
        }

        // Traitement des récompenses en fonction du type de carte
        if (isGoldCard)
        {
            // Traitement pour les récompenses de Gold Pass
            switch (level.data.goldPassRewardType)
            {
                case BattlePassRewardType.Equipement:
                    HandleReward(level.data.goldPassEquipement);
                    break;
                case BattlePassRewardType.Ressource:
                    HandleReward(level.data.goldPassRessource);
                    break;
                default:
                    Debug.LogWarning("Unknown reward type for Gold Pass.");
                    break;
            }
        }
        else
        {
            // Traitement pour les récompenses de Free Pass
            switch (level.data.freePassRewardType)
            {
                case BattlePassRewardType.Equipement:
                    HandleReward(level.data.freePassEquipement);
                    break;
                case BattlePassRewardType.Ressource:
                    HandleReward(level.data.freePassRessource);
                    break;
                default:
                    Debug.LogWarning("Unknown reward type for Free Pass.");
                    break;
            }
        }

        // Marquer le niveau comme réclamé
        if (isGoldCard)
            level.hasClaimedGoldCard = true;
        else
            level.hasClaimedFreeCard = true;
        Debug.Log("Reward claimed for level: " + level);
        DataPersistenceManager.instance.SaveGame();
        RessourcesManager.Instance.StartWaitForReloading();
        battlePassCard.PlayClaimEffect();
    }

    private void HandleReward(object reward)
    {
        // Exemple de traitement des récompenses
        // Ici, vous pouvez ajouter la logique pour traiter l'équipement ou la ressource
        if (reward is Equipement equipement)
        {
            RessourcesManager.Instance.AddEquipements(equipement.equipement);
        }
        else if (reward is Ressource ressource)
        {
            switch (ressource.ressourceType)
            {
                case RessourceType.Gold:
                    RessourcesManager.Instance.AddGold(ressource.amount);
                    break;
                case RessourceType.Gem:
                    RessourcesManager.Instance.AddGems(ressource.amount);
                    break;
                default:
                    Debug.LogWarning("Unknown reward type for Free Pass.");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Unknown reward type.");
        }
    }

    private void DeleteChildsObject(Transform parentObject)
    {
        if (parentObject != null && parentObject.transform.childCount > 0)
        {
            // Get the first child
            foreach (Transform child in parentObject.transform)
            {
                Destroy(child.gameObject);
            }
            // Destroy the first child GameObject
        }
    }

    private bool HasReachThisLevel(int level)
    {
        if (level <= 0)
        {
            Debug.LogWarning("Level must be greater than 0.");
            return false;
        }

        // Calcule l'expérience totale requise pour atteindre le niveau donné
        float requiredXp = GetXpForLevel(level);

        // Vérifie si l'expérience actuelle est supérieure ou égale à l'expérience requise
        return battlePassXp >= requiredXp;
    }

    private void UpdateProgressBar()
    {
        // Variables d'exemple
        float cellSize = progressBarCellsSize; // Taille de la cellule
        float spacing = progressBarSpaceBetweenCells; // Espacement entre les cellules
        float paddingLeft = progressBarPadding; // Padding gauche
        int totalLevels = battlePassLevels.Count; // Nombre total de niveaux
        float xpPerLevel = xpPerBattlePassLevel; // XP nécessaire par niveau
        float currentXp = battlePassXp; // XP actuel

        // Calcul de la distance entre deux points (taille de la cellule + espacement)
        float D2C = cellSize + spacing;

        // Calcul du padding total (gauche + 1/2 taille d'une cellule)
        float padding = paddingLeft + (cellSize / 2f);

        // Calcul de la taille maximale de la barre de progression
        float maxSize = (D2C * (totalLevels - 1)) + padding;

        // Calcul de la progression actuelle
        float totalProgress = currentXp / xpPerLevel * D2C;
        float X = (padding + totalProgress) / maxSize;

        // Assurez-vous que X ne dépasse jamais 1
        X = Mathf.Clamp01(X);

        // Mise à jour de la barre de progression
        progressBar.fillAmount = X;
    }

    private void UpdateProgressBarSize()
    {
        if (progressBar == null)
            return;

        RectTransform rectTransform = progressBar.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogWarning("ProgressBar RectTransform not found.");
            return;
        }

        RectTransform goldCardRectTransform = goldCardPrefab.GetComponent<RectTransform>();
        GridLayoutGroup gridLayout = goldCardParent.GetComponent<GridLayoutGroup>();

        if (goldCardRectTransform == null || gridLayout == null)
        {
            Debug.LogWarning("GoldCard RectTransform or GridLayoutGroup not found.");
            return;
        }

        float cardWidth = gridLayout.cellSize.x;
        float cellGap = gridLayout.spacing.x * 1.225f;
        float padding = gridLayout.padding.left;
        float distanceBetweenTwoCardCenter = cardWidth + cellGap;

        // Debug.LogError($"cardWidth: {cardWidth}, cellGap:{cellGap}, padding:{padding}, battlePassLevels.Count: {battlePassLevels.Count}");

        float totalWidth = distanceBetweenTwoCardCenter * (battlePassLevels.Count - 1) + padding;

        rectTransform.sizeDelta = new Vector2(totalWidth, rectTransform.sizeDelta.y);
        progressBarPadding = gridLayout.padding.left;
        progressBarCellsSize = cardWidth;
        progressBarSpaceBetweenCells = cellGap;
    }

    private void UpdateContentsSize()
    {
        if (goldCardPrefab == null || freeCardPrefab == null || goldCardParent == null)
        {
            return;
        }

        RectTransform goldCardRectTransform = goldCardPrefab.GetComponent<RectTransform>();
        RectTransform freeCardRectTransform = freeCardPrefab.GetComponent<RectTransform>();
        GridLayoutGroup gridLayout = goldCardParent.GetComponent<GridLayoutGroup>();

        if (goldCardRectTransform == null || gridLayout == null || freeCardRectTransform == null)
        {
            Debug.LogWarning("GoldCard RectTransform or GridLayoutGroup not found.");
            return;
        }

        float cardWidth = gridLayout.cellSize.x;
        float distanceBetweenTwoCardCenter = cardWidth;

        float totalWidth = distanceBetweenTwoCardCenter * (battlePassLevels.Count - 4) + 50;

        contentParent.sizeDelta = new Vector2(totalWidth, contentParent.sizeDelta.y);

        goldCardRectTransform.sizeDelta = new Vector2(totalWidth, contentParent.sizeDelta.y);
        freeCardRectTransform.sizeDelta = new Vector2(totalWidth, contentParent.sizeDelta.y);
    }

    private void LoadAndCheckData()
    {
        // Charge tous les BattlePassLevelData présents dans le répertoire spécifié
        BattlePassLevelData[] levelsData = Resources.LoadAll<BattlePassLevelData>(
            "BattlePassLevels/"
        );

        // Vérifie si la liste actuelle est différente de la liste chargée
        bool isDifferent = levelsData.Length != battlePassLevels.Count;

        if (!isDifferent)
        {
            for (int i = 0; i < levelsData.Length; i++)
            {
                // Comparaison basée sur les données de l'objet (vous pouvez ajuster selon les propriétés nécessaires)
                if (levelsData[i] != battlePassLevels[i].data)
                {
                    isDifferent = true;
                    break;
                }
            }
        }

        // Si les données sont différentes, recharge la liste
        if (isDifferent)
        {
            battlePassLevels.Clear();

            foreach (var level in levelsData)
            {
                battlePassLevels.Add(new BattlePassLevels(level, false));
            }
        }
    }

    private void AddIAPButton()
    {
        CodelessIAPButton IAPButton = buyGoldPassButton.GetComponent<CodelessIAPButton>();
        if (IAPButton == null)
        {
            return;
        }

        IAPButton.button = buyGoldPassButton;

        IAPButton.onPurchaseComplete.AddListener(OnConsomablePurchaseComplete);
        IAPButton.onPurchaseFailed.AddListener(OnConsomablePurchaseFailed);
    }

    private void OnConsomablePurchaseComplete(Product product)
    {
        Debug.Log($"Purchase completed: {product.definition.id}");
        hasGoldPass = true;
        RessourcesManager.Instance.StartWaitForSaveAndReloading();
        popupController.ShowPopup();
    }

    private void OnConsomablePurchaseFailed(Product product, PurchaseFailureDescription reason)
    {
        Debug.LogWarning($"Purchase failed: {product.definition.id}, reason: {reason}");
    }

    private void Refresh()
    {
        if (battlePassLevels.Count == DataPersistenceManager.instance.gameData.battlePassLevels.Count && hasGoldPass == DataPersistenceManager.instance.gameData.hasGoldPass)
        {
            return;
        }
        battlePassLevels = DataPersistenceManager.instance.gameData.battlePassLevels;
        hasGoldPass = DataPersistenceManager.instance.gameData.hasGoldPass;
        UpdateCardList();
        UpdateProgressBarSize();
        UpdateContentsSize();
        if (hasGoldPass)
            buyGoldPassButton.gameObject.SetActive(false);
    }

    public void LoadData(GameData data)
    {
        Refresh();
    }

    public void SaveData(GameData data)
    {
        data.battlePassLevels = battlePassLevels;
        data.hasGoldPass = hasGoldPass;
        if (hasGoldPass && buyGoldPassButton != null)
            buyGoldPassButton.gameObject.SetActive(false);
    }
}
