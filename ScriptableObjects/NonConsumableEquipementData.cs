using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "NonConsumableEquipement", menuName = "New Shop Item/Non Consumable Equipement")]
public class NonConsumableEquipementData : SerializableScriptableObject
{
    public EquipementData data;
    public PaymantSubtype subType;
    public double amount;
    [JsonIgnore]
    public Sprite visualIconInShop;
    [Header("Seulement pour les produits necessitant de la moula")]
    public bool requireIAP = false;
    public string productId;
}

public enum PaymantSubtype
{
    Gold,
    Gem,
    Money,
}
public enum RessourceType
{
    Gold,
    Gem
}