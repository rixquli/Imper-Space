using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "Gadgets", menuName = "Gadgets/New Gadget")]
public class GadgetsData : SerializableScriptableObject
{
    public int gadgetId;        // Unique identifier for the gadget
    public UDictionary<int, double> upgradeStateAndPrice;
    [JsonIgnore]
    public UDictionary<int, Sprite> upgradeStateAndSprite;
    // TODO: utilisé ce pricePerBuilding au lieu de celui de VillageRessourceManager 
    public UDictionary<Vector2, Vector2> areaSizePlacement;
    public string displayName;  // Display name of the gadget
    [JsonIgnore]
    public Sprite visual;
    [JsonIgnore]
    public GameObject prefab;   // Prefab of the gadget to instantiate in the game
}
