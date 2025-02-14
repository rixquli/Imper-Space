using UnityEngine;
using BattlePass;
using Newtonsoft.Json;

[CreateAssetMenu(fileName = "Lvl.", menuName = "Battle Pass Level")]
public class BattlePassLevelData : SerializableScriptableObject
{
    [Header("Battle Pass Level")]
    public int level;

    [Header("Gold Pass Reward")]
    public BattlePassRewardType goldPassRewardType = BattlePassRewardType.None;

    public Equipement goldPassEquipement;
    public Ressource goldPassRessource;

    [Header("Free Pass Reward, let null if no reward")]
    public BattlePassRewardType freePassRewardType = BattlePassRewardType.None;

    public Equipement freePassEquipement;
    public Ressource freePassRessource;
}
namespace BattlePass
{

    public enum BattlePassRewardType
    {
        Equipement,
        Ressource,
        None
    }

    [System.Serializable]
    public class Equipement
    {
        public EquipementData equipement;
        public string description;
    }

    [System.Serializable]
    public class Ressource
    {
        public RessourceType ressourceType;
        public double amount;
        public string description;
    }
}

