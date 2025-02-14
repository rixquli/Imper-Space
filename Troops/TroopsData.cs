using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "Troops", menuName = "Troops")]
[System.Serializable]
public class TroopsData : SerializableScriptableObject
{
    public int id;
    public string displayName;
    [JsonIgnore]
    public Sprite image;
    [JsonIgnore]
    public GameObject prefab;
    [JsonIgnore]
    public GameObject previewPrefab;
    public UDictionary<int, int> troopPerCaserneLevel; // caserneLevel, troopAmount
    public int troopCost;
}
