using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableEquipement", menuName = "New Shop Item/Random Gacha Equipement")]
public class RandomGachaEquipementData : SerializableScriptableObject
{
    public string displayName;
    [Header("Gain")]
    public PullingType pullAmount = PullingType.x1;
    [Header("Probabilities")]
    public UDictionary<RarityType, float> rarityPercentages = new()
    {
        { RarityType.Common, 80f },
        { RarityType.Rare, 15f },
        { RarityType.Epic, 4f },
        { RarityType.Legendary, 1f }
    };
    [Header("With")]
    public PaymantSubtype paySubType;
    public double payAmount;
    [JsonIgnore]
    public Sprite visualIconInShop;
}


public enum RarityType
{
    Common,
    Rare,
    Epic,
    Legendary,
    Exclusive
}

public enum PullingType
{
    x1 = 1,
    x10 = 10
}

public static class RarityColor
{
    public static Color Empty = Color.black;
    public static Color Common = Color.white;
    public static Color Rare = new Color(0, 100 / 255f, 1);
    public static Color Epic = new Color(175 / 255f, 0, 1);
    public static Color Legendary = new Color(1, 1, 0);

    public static Color GetColorByRarity(RarityType rarityType)
    {
        switch (rarityType)
        {
            case RarityType.Common:
                return Common;
            case RarityType.Rare:
                return Rare;
            case RarityType.Epic:
                return Epic;
            case RarityType.Legendary:
                return Legendary;
            default:
                return Empty;
        }
    }
}
