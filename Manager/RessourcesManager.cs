using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RessourcesManager : MonoBehaviour, IDataPersistence
{
    public static RessourcesManager Instance;

    public double gemAmount { get; private set; }
    public double goldAmount { get; private set; }
    public List<ItemsAndAmount> items { get; private set; } = new List<ItemsAndAmount>();
    public List<EquipementsAndAmount> equipements { get; private set; } =
        new List<EquipementsAndAmount>();
    public UDictionary<EquipeSlot, EquipementData> equipedEquipements
    {
        get;
        private set;
    } = new UDictionary<EquipeSlot, EquipementData>();
    public AttributesData playerAttributesData { get; private set; }
    public List<Levels> levels { get; private set; } = new();
    public List<EnemyVillages> enemyVillages { get; private set; } = new();

    public UDictionary<int, double> costPerLevelToUpgradeTurret =
        new()
        {
            [1] = 0.0,
            [2] = 10.0,
            [3] = 25.0,
        };
    public UDictionary<int, double> costPerLevelToUpgradeWall = new() { [1] = 0.0 };
    public UDictionary<int, double> costPerPlayerCompanion =
        new()
        {
            [1] = 10.0,
            [2] = 15.0,
            [3] = 20.0,
            [4] = 40.0,
            [5] = 60.0,
            [6] = 80.0,
        };

    public BuildingList[] buildingList = { };

    public WatchPerDay watchPerDay;

    public event Action OnValueChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RefreshValue();
    }

    private void OnEnable()
    {
        RefreshValue();
        SceneManager.sceneLoaded += (scen, load) => RefreshValue();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= (scen, load) => RefreshValue();
    }

    public void RefreshValue()
    {
        if (ThreadUtility.IsMainThread())
            OnValueChanged?.Invoke();
    }

#if UNITY_EDITOR
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.I))
        // {
        //     AddGems(50);
        // }
    }
#endif

    public double costPerLevelToUpgradeEquipment(EquipementData equipementData)
    {
        int level = GetLevel(equipementData) - 1;
        return 25 * Mathf.Pow(2, Mathf.CeilToInt(level / 4));
    }
    public double costPerLevelToUpgradeEquipment(int level)
    {
        return 25 * Mathf.Pow(2, Mathf.CeilToInt((level - 1) / 4));
    }

    public void AddGems(double amount)
    {
        gemAmount += amount;
        // StartCoroutine(WaitForSaving());
        RefreshValue();
    }

    public void AddGold(double amount)
    {
        Debug.LogWarning("Adding gold: " + amount);
        goldAmount += amount;
        Debug.LogWarning("To: " + goldAmount);
        // StartCoroutine(WaitForSaving());
        RefreshValue();
    }

    // TODO: remove all those shits
    IEnumerator WaitForSaving()
    {
        yield return null;
        DataPersistenceManager.instance.SaveGame();
    }

    IEnumerator WaitForLoading()
    {
        yield return null;
        RefreshValue();
    }

    public void StartWaitForSave()
    {
        StartCoroutine(WaitForSaving());
    }

    public void StartWaitForSaveAndReloading()
    {
        StartCoroutine(WaitForSaving());
        StartCoroutine(WaitForLoading());
        // RefreshValue();
    }

    public void StartWaitForReloading()
    {
        StartCoroutine(WaitForLoading());
        // RefreshValue();
    }

    public void AddItems(List<ItemsAndAmount> itemsAndAmounts)
    {
        foreach (ItemsAndAmount itemAndAmount in itemsAndAmounts)
        {
            bool itemExists = false;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].data == itemAndAmount.data)
                {
                    items[i] = new ItemsAndAmount(
                        items[i].data,
                        items[i].amount + itemAndAmount.amount
                    );
                    itemExists = true;
                    break;
                }
            }

            if (!itemExists)
            {
                items.Add(new ItemsAndAmount(itemAndAmount.data, itemAndAmount.amount));
            }
        }

        // StartCoroutine(WaitForSaving());
        RefreshValue();
    }

    public void AddEquipements(EquipementData[] equipementsAndAmounts)
    {
        foreach (EquipementData equipementAndAmount in equipementsAndAmounts)
        {
            bool equipementExists = false;
            for (int i = 0; i < equipements.Count; i++)
            {
                if (equipements[i].data == equipementAndAmount)
                {
                    equipementExists = true;
                    break;
                }
            }

            if (!equipementExists)
            {
                equipements.Add(new EquipementsAndAmount(equipementAndAmount));
            }
        }

        // StartCoroutine(WaitForSaving());
        RefreshValue();
    }

    public void AddEquipements(List<EquipementsAndAmount> equipementsAndAmounts)
    {
        foreach (EquipementsAndAmount equipementAndAmount in equipementsAndAmounts)
        {
            bool equipementExists = false;
            for (int i = 0; i < equipements.Count; i++)
            {
                if (equipements[i].data == equipementAndAmount.data)
                {
                    equipementExists = true;
                    break;
                }
            }

            if (!equipementExists)
            {
                equipements.Add(
                    new EquipementsAndAmount(equipementAndAmount.data, equipementAndAmount.amount)
                );
            }
        }

        // StartCoroutine(WaitForSaving());
        RefreshValue();
    }

    public void UpgradeEquipement(EquipementData equipement)
    {
        var curEquipement = equipements.Find(e => e.data == equipement);
        UpgradeEquipement(curEquipement);
    }

    public void UpgradeEquipement(EquipementsAndAmount equipement)
    {
        equipement = equipements.Find(e => e.data == equipement.data);

        int newLevel = equipement.level + 1;
        Buy(
            PaymantSubtype.Gold,
            costPerLevelToUpgradeEquipment(newLevel),
            () =>
            {
                for (int i = 0; i < equipements.Count; i++)
                {
                    if (equipements[i].data == equipement.data)
                    {
                        equipements[i] = new EquipementsAndAmount(
                            equipements[i].data,
                            equipements[i].amount,
                            equipements[i].level + 1
                        );
                        break;
                    }
                }

                // StartCoroutine(WaitForSaving());
                RefreshValue();
            },
            () =>
            {
                Debug.LogError("Error during the process");
            }
        );
    }

    public int GetLevel(EquipementData equipement)
    {
        for (int i = 0; i < equipements.Count; i++)
        {
            if (equipements[i].data == equipement)
            {
                return equipements[i].level;
            }
        }
        return 1;
    }

    public void AddEquipements(EquipementData equipementsAndAmounts, int amount = 1)
    {
        bool equipementExists = false;
        for (int i = 0; i < equipements.Count; i++)
        {
            if (equipements[i].data == equipementsAndAmounts)
            {
                equipementExists = true;
                break;
            }
        }
        Debug.LogError("equipementExists: " + equipementExists);
        if (!equipementExists)
        {
            equipements.Add(new EquipementsAndAmount(equipementsAndAmounts, amount));
        }

        Debug.LogError("OnValueChanged is about to be invoked");
        RefreshValue();
    }

    public void SetEquipedEquipments(EquipementData equipementData)
    {
        if (
            equipedEquipements.ContainsKey(equipementData.slot)
            && equipedEquipements[equipementData.slot] == equipementData
        )
            return;
        Debug.Log("set " + equipementData.slot + " equipement: " + equipementData?.displayName);
        equipedEquipements[equipementData.slot] = equipementData;
        RefreshValue();
    }

    public void SetEquipedEquipments(EquipeSlot slot, EquipementData equipementData)
    {
        if (equipedEquipements.ContainsKey(slot) && equipedEquipements[slot] == equipementData)
            return;
        Debug.Log("set " + slot + " equipement: " + equipementData?.displayName);
        equipedEquipements[slot] = equipementData;
        RefreshValue();
    }

    public void SetPlayerAttributes(AttributesData attributesData)
    {
        playerAttributesData = attributesData;
        // DataPersistenceManager.instance.SaveGame();
        // RefreshValue();
    }

    public void SetLevelCompleted(int levelNumber)
    {
        if (levelNumber < 0 || levels == null || levels.Count == 0)
            return;

        Levels currentLevel = levels.Find(l => ((LevelInLevelSelectionData)l?.data)?.levelNumber == levelNumber);
        if (currentLevel != null)
        {
            currentLevel.hasCompleted = true;
        }

        Levels nextLevel = levels.Find(l => ((LevelInLevelSelectionData)l?.data)?.requiredLevelNumber == levelNumber);
        if (nextLevel != null || ((LevelInLevelSelectionData)nextLevel.data).levelNumber == 1)
        {
            nextLevel.hasReached = true;
        }

        // DataPersistenceManager.instance.SaveGame();
    }

    public void UpdateLevelHasReached()
    {
        // Return if the levels list is null or empty
        if (levels == null || levels.Count == 0)
            return;

        for (int i = 0; i < levels.Count; i++)
        {
            Levels currentLevel = levels[i];

            // Skip if current level or its data is null
            if (currentLevel == null || currentLevel.data == null)
                continue;

            LevelInLevelSelectionData currentLevelData = currentLevel.data;

            // Skip if required level number is invalid
            if (currentLevelData?.requiredLevelNumber < 1)
                continue; // Assuming level numbers start from 1

            // Find the required level based on its level number
            Levels requiredLevel = levels.Find(l =>
                l != null && l.data != null && ((LevelInLevelSelectionData)l.data)?.levelNumber == currentLevelData.requiredLevelNumber
            );

            // Update hasReached if the required level is completed or if it's the first level
            if (
                (requiredLevel != null && requiredLevel.hasCompleted)
                || currentLevelData.levelNumber == 1
            )
            {
                currentLevel.hasReached = true;
            }
        }
    }

    public void SetEnemyVillageCompleted(int villageNumber)
    {
        if (villageNumber < 0 || enemyVillages == null || enemyVillages.Count == 0)
            return;

        EnemyVillages currentLevel = enemyVillages.Find(l =>
            l != null && l.data != null && l.data != null && l.data.villageNumber == villageNumber
        );
        if (currentLevel != null)
        {
            currentLevel.hasCompleted = true;
        }
    }

    public void UpdateEnemyVillageHasReached()
    {
        // Return if the enemyVillages list is null or empty
        if (enemyVillages == null || enemyVillages.Count == 0)
            return;

        for (int i = 0; i < enemyVillages.Count; i++)
        {
            EnemyVillages currentLevel = enemyVillages[i];

            // Skip if current level or its data is null
            if (currentLevel == null || currentLevel.data == null)
                continue;

            // Skip if required level number is invalid
            if (currentLevel.data?.requiredVillageNumber < 1)
                continue; // Assuming level numbers start from 1

            // Find the required level based on its level number
            EnemyVillages requiredLevel = enemyVillages.Find(l =>
                l?.data?.villageNumber == currentLevel.data.requiredVillageNumber
            );

            // Update hasReached if the required level is completed or if it's the first level
            if (
                (requiredLevel != null && requiredLevel.hasCompleted)
                || currentLevel.data.villageNumber == 1
            )
            {
                currentLevel.hasReached = true;
            }
        }
    }

    public bool Buy(PaymantSubtype subtype, double amount, Action callback, Action callbackError)
    {
        switch (subtype)
        {
            case PaymantSubtype.Gold:
                if (goldAmount >= amount)
                {
                    goldAmount -= amount;
                    callback();
                    RefreshValue();
                    return true;
                }
                else
                {
                    callbackError();
                    return false;
                }
            case PaymantSubtype.Gem:
                if (gemAmount >= amount)
                {
                    gemAmount -= amount;
                    callback();
                    RefreshValue();
                    return true;
                }
                else
                {
                    callbackError();
                    return false;
                }
            default:
                callbackError();
                return false;
        }
    }

    public EquipementData GetPreviousEquipement(EquipementData equipementData)
    {
        EquipementData previousEquipement = null;
        for (int i = 0; i < equipements.Count; i++)
        {
            if (equipements[i].data == equipementData)
            {
                Debug.LogError("Current equipement: " + equipements[i].data.displayName);
                if (i == 0)
                {
                    previousEquipement = equipements[equipements.Count - 1].data;
                }
                else
                {
                    previousEquipement = equipements[i - 1].data;
                }
                break;
            }
        }
        Debug.LogError("Previous equipement: " + previousEquipement?.displayName);
        return previousEquipement;
    }

    public EquipementData GetNextEquipement(EquipementData equipementData)
    {
        EquipementData nextEquipement = null;
        for (int i = 0; i < equipements.Count; i++)
        {
            if (equipements[i].data == equipementData)
            {
                if (i == equipements.Count - 1)
                {
                    nextEquipement = equipements[0].data;
                }
                else
                {
                    nextEquipement = equipements[i + 1].data;
                }
                break;
            }
        }

        return nextEquipement;
    }

    public void LoadData(GameData data)
    {
        Debug.Log("Loading data...");
        Debug.Log("Current gold: " + goldAmount);
        this.gemAmount = data.gemAmount;
        this.goldAmount = data.goldAmount;
        this.items = data.items;
        this.equipements = data.equipements;
        this.equipedEquipements = data.equipedEquipements;
        this.playerAttributesData = data.playerAttributesData;
        this.levels = data.levels;
        this.enemyVillages = data.enemyVillages;
        this.watchPerDay = data.watchPerDay;
        Debug.Log("Loaded gold: " + goldAmount);

        UpdateLevelHasReached();
        UpdateEnemyVillageHasReached();

        RefreshValue();
    }

    public void SaveData(GameData data)
    {
        UpdateLevelHasReached();
        UpdateEnemyVillageHasReached();
        Debug.Log("Saving data...");
        Debug.Log("Current gold: " + goldAmount);
        data.gemAmount = this.gemAmount;
        data.goldAmount = this.goldAmount;
        data.items = this.items;
        data.equipements = this.equipements;
        data.equipedEquipements = this.equipedEquipements;
        data.playerAttributesData = this.playerAttributesData;
        data.levels = this.levels;
        data.enemyVillages = this.enemyVillages;
        data.watchPerDay = this.watchPerDay;
        Debug.Log("Saved gold: " + data.goldAmount);
    }
}

[Serializable]
public class BuildingList
{
    [Serializable]
    public class Buildings
    {
        public int level;
        public Vector2 position;
        public float rotationZ;
        [JsonIgnore]
        public GameObject gameObject;
        public bool isPurchase = false;
        public double price;
        public int hdvLevelRequired;
    }

    public GadgetsData gadgetsData;
    public UDictionary<int, double> pricePerBuilding;
    public List<Buildings> buildings = new();
    public int currentPlacedBuilding;
}
