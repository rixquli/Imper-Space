using UnityEngine;

public class VillagesDataManager : MonoBehaviour
{
    public static VillagesDataManager Instance;

    public BuildingList[] buildingList = { };
    public GameObject[] ennemiesToSpawn;
    public int minEnemiesLevel = 1;
    public int maxEnemiesLevel = 3;
    public double rewardGold;
    public double rewardGem;
    public int villageNumber;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ne pas détruire cet objet entre les scènes
        }
        else
        {
            Destroy(gameObject); // Si une instance existe déjà, détruire celle-ci
        }
    }

    public void SetVillageData(
        BuildingList[] buildingList,
        GameObject[] ennemiesToSpawn,
        int minEnemiesLevel,
        int maxEnemiesLevel,
        RessourceType rewardType,
        double rewardAmount,
        int villageNumber
    )
    {
        this.buildingList = buildingList;
        this.ennemiesToSpawn = ennemiesToSpawn;
        this.minEnemiesLevel = minEnemiesLevel;
        this.maxEnemiesLevel = maxEnemiesLevel;
        switch (rewardType)
        {
            case RessourceType.Gold:
                this.rewardGold = rewardAmount;
                this.rewardGem = 0;
                break;
            case RessourceType.Gem:
                this.rewardGem = rewardAmount;
                this.rewardGold = 0;
                break;
        }
        this.villageNumber = villageNumber;
    }

    public void ResetVillageData()
    {
        buildingList = new BuildingList[] { };
        ennemiesToSpawn = new GameObject[] { };
        minEnemiesLevel = 0;
        maxEnemiesLevel = 0;
        rewardGold = 0;
        rewardGem = 0;
        villageNumber = 0;
    }
}
