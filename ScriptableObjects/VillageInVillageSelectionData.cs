using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "VillagePoint", menuName = "Village Point")]
public class VillageInVillageSelectionData : SerializableScriptableObject
{
    [Header("Level Description")]
    public int villageNumber;
    public int requiredVillageNumber;
    public string villageName;
    [JsonIgnore]
    public BuildingList[] BuildingList;
    [JsonIgnore]
    public GameObject[] enemyToSpawn;
    public int enemiesMinLevel;
    public int enemiesMaxLevel;


    [Header("Gain")]
    public RessourceType gainSubType;
    public double gainAmount;

}
