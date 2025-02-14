using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableEquipement", menuName = "New Shop Item/Consumable Equipement")]
public class ConsumableEquipementData : SerializableScriptableObject
{
    [Header("Gain")]
    public PaymantSubtype gainSubType;
    public double gainAmount;
    [Header("With")]
    public PaymantSubtype paySubType;
    public double payAmount;
    [JsonIgnore]
    public Sprite visualIconInShop;
    [Header("Seulement pour les produits necessitant de la moula mais il faut ajouter manuelement le IAPButton et le congigurer")]
    public bool requireIAP = false;
    public string productId;
}
