using System;
using DragAndDrop;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "Equipements", menuName = "Equipements/New Equipement")]
[Serializable]
public class EquipementData : SerializableScriptableObject
{
    public string displayName;
    [JsonIgnore]
    public Sprite icon;
    public RarityType rarity;
    public Color color = Color.white;
    public EquipeSlot slot;

    [Header("For ship stats = base stat for other = percentage of ship stats")]
    public UDictionary<EquiementsStats, float> stats;

    [Header("For Ship only")]
    public float bulletPerSecond = 2f;

    [Header("For Missile and Laser only")]
    [JsonIgnore]
    public GameObject prefab;

    [Header("For Random pulling")]
    [Tooltip("Is it's false this item can't be pulled")]
    public bool canBePulled = true;
}

[Serializable]
public struct EquipementStats
{
    public EquiementsStats key;
    public float value;
}

public enum EquiementsStats
{
    AttackDamage,
    AttackSpeed,
    Speed,
    Health,
    Shield,
    HealthRegen,
    ShieldRegen,
    DashAmount,
    DashRegen,
    DashDuration,
    DashSpeed,

    //TODO: en rajouter
}

public enum FiringMode
{
    Single,
    Burst,
    Automatic,
}
